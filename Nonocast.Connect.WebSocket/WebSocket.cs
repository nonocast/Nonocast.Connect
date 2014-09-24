using System;
using System.Collections.Generic;
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
		}

		public void Open() {
			client = new TcpClient();
			client.Connect(hostname, port);
			this.stream = client.GetStream();
			Handshake();
			new Thread(new ParameterizedThreadStart(Process)).Start(this.stream);

		}

		private void Handshake() {
			string content = @"GET /x HTTP/1.1
Upgrade: websocket
Connection: Upgrade
Sec-WebSocket-Key: HnUJNuLm5FDSnTM4b1Qnug==
Sec-WebSocket-Version: 13

";
			var data = Encoding.ASCII.GetBytes(content);
			this.stream.Write(data, 0, data.Length);
		}

		private void Process(object arg) {
			var stream = arg as NetworkStream;

			byte[] buffer = new byte[4096];
			int readCount = 0;

			while ((readCount = stream.Read(buffer, 0, buffer.Length)) > 0) {
				string message = ParseReceiveData(buffer, readCount);
				if (!string.IsNullOrEmpty(message) && MessageReceived != null) MessageReceived(message);
			}
		}

		private string ParseReceiveData(byte[] buffer, int readCount) {
			var payload = new byte[readCount - 2];
			Array.Copy(buffer, 2, payload, 0, readCount - 2);
			return Encoding.UTF8.GetString(payload);
		}

		public void Close() {
			client.Close();
		}

		private TcpClient client;
		private NetworkStream stream;
		private string hostname;
		private int port;
		private string url;
	}
}
