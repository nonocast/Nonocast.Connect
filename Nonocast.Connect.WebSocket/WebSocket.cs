using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Nonocast.Connect.WebSocket {
	public class WebSocket {
		public event Action<string> MessageReceived;

		public WebSocket(string url) {
			this.url = url;
			var p = new Uri(this.url);
			this.hostname = p.Host;
			this.port = p.Port;
			this.parser = new MessageParser();
			this.parser.MessageReceived += (message) => {
				if (!string.IsNullOrEmpty(message) && MessageReceived != null) MessageReceived(message);
			};
		}

		public void Open() {
			client = new TcpClient();
			client.Connect(hostname, port);
			this.stream = client.GetStream();
			Handshake();
			new Thread(new ParameterizedThreadStart(Process)).Start(this.stream);

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

			var header = new RequestHeader(stream);

			byte[] buffer = new byte[4096];
			int readCount = 0;

			try {
				while ((readCount = stream.Read(buffer, 0, buffer.Length)) > 0) {
					parser.Push(buffer, readCount);
				}
			} catch (IOException) {
				// ignore
			} catch {
				// ignore
			} finally {
				// Console.WriteLine("Thread Exit...");
			}
		}

		public void Close() {
			client.Close();
		}

		private MessageParser parser;
		private TcpClient client;
		private NetworkStream stream;
		private string hostname;
		private int port;
		private string url;
	}
}
