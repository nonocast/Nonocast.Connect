using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

namespace Nonocast.Connect.WebSocket {
	public class WebSocket {
		public event Action<string> MessageReceived;

		public WebSocket(string url) {
			this.url = url;
			var p = new Uri(this.url);
			this.hostname = p.Host;
			this.port = p.Port;
			this.parser = new FrameParser();
			this.parser.MessageReceived += (message) => {
				if (message is TextMessage) {
					if (MessageReceived != null) MessageReceived((message as TextMessage).Content);
				}
			};
		}

		public void Open() {
			if (opening) return;
			try {
				opening = true;
				client = new TcpClient();
				client.Connect(hostname, port);
				this.stream = client.GetStream();
				Handshake();
				new Thread(new ParameterizedThreadStart(Process)).Start(this.stream);
				Connected = true;
				opening = false;
			} catch { // offline and try reconnect
				Connected = false;
				opening = false;
			}
		}

		private void Handshake() {
			var header = new RequestHeader();
			header.StartLine = "GET /x HTTP/1.1";
			header.Properties.Add("Upgrade", "websocket");
			header.Properties.Add("Connection", "Upgrade");
			header.Properties.Add("Sec-WebSocket-Key", "HnUJNuLm5FDSnTM4b1Qnug==");
			header.Properties.Add("Sec-WebSocket-Version", "13");
			var data = Encoding.ASCII.GetBytes(header.ToString());
			this.stream.Write(data, 0, data.Length);
		}

		private void Process(object arg) {
			// Console.WriteLine("Thread Enter...");
			var stream = arg as NetworkStream;

			if (stream == null || stream.CanRead == false || stream.CanWrite == false || stream.DataAvailable == false) {
				Connected = false;
				return;
			}

			var header = new RequestHeader(stream);

			byte[] buffer = new byte[4096];
			int readCount = 0;

			try {
				while ((readCount = stream.Read(buffer, 0, buffer.Length)) > 0) {
					parser.Push(buffer, readCount);
				}
			} catch (IOException) {
				// ignore
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
				// RESET
			} finally {// offline and try reconnect
				Connected = false;
				// Console.WriteLine("Thread Exit...");
			}
		}

		public void Emit(string message) {
			if (Connected == false || stream == null) throw new SocketException();
			var buffer = new ClientFrame(new TextMessage(message)).ToBytes();
			try {
				stream.Write(buffer, 0, buffer.Length);
			} catch (IOException) { // offline and try reconnect
				Connected = false;
			}
		}

		public void Close() {
			try { if (stream != null) stream.Close(); } catch { }
			try { if (client != null) client.Close(); } catch { }
			stream = null;
			client = null;
			connected = false;
		}

		private void StartReconnect() {
			if (reconnectTimer != null && reconnectTimer.Enabled) return;

			Close();
			reconnectTimer = new System.Timers.Timer(2000);
			reconnectTimer.Elapsed += (sender, e) => Open();
			reconnectTimer.Start();
		}

		private void StopReconnect() {
			try { reconnectTimer.Stop(); } catch { }
			try { reconnectTimer.Close(); } catch { }
		}

		public bool Connected {
			get { return connected; }
			set {
				connected = value;
				if (value) {
					StopReconnect();
				} else {
					StartReconnect();
				}
			}
		}

		private FrameParser parser;
		private TcpClient client;
		private NetworkStream stream;
		private string hostname;
		private int port;
		private string url;
		private bool connected = false;
		private bool opening = false;
		private System.Timers.Timer reconnectTimer;
	}
}
