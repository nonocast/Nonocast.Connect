using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Nonocast.Connect.WebSocket;

namespace Nonocast.Connect {
	public class WebSocket6455 : IWebSocketServer {
		public event Action<string> MessageReceived;
		public List<NetworkStream> Clients { get; private set; }

		public WebSocket6455() {
			Clients = new List<NetworkStream>();

		}

		public void Handle(Request req, Response res) {
			if (!IsMatch(req)) return;

			var stream = req.Stream;
			var parser = new FrameParser();
			parser.MessageReceived += (message) => {
				if (message is TextMessage) {
					if (MessageReceived != null) MessageReceived((message as TextMessage).Content);
				}
			};
			parser.Close += () => {
				lock (Clients) { Clients.Remove(stream); }
			};

			var handshake = ComputeHandshake(req.Header.Properties["Sec-WebSocket-Key"]);

			var header = new RequestHeader();
			header.StartLine = "HTTP/1.1 101 Switching Protocols";
			header.Properties.Add("Upgrade", "websocket");
			header.Properties.Add("Connection", "Upgrade");
			header.Properties.Add("Sec-WebSocket-Accept", handshake);
			res.WriteHeader(header);

			lock (Clients) { Clients.Add(stream); }
			ConsoleHelper.WriteLine(ConsoleColor.White, "WebSocket running...");

			byte[] buffer = new byte[4096];
			int readCount = 0;

			try {
				while ((readCount = stream.Read(buffer, 0, buffer.Length)) > 0) {
					parser.Push(buffer, readCount);
				}
			} finally {
				try { lock (Clients) { Clients.Remove(stream); } } catch { }
				res.JustDone();
			}
		}

		private bool IsMatch(Request req) {
			var header = req.Header;
			return header.Properties.ContainsKey("Connection") && header.Properties.ContainsKey("Upgrade") &&
				   header.Properties["Connection"].ToLower().Contains("upgrade") && header.Properties["Upgrade"].ToLower() == "websocket" &&
				   header.Properties.ContainsKey("Sec-WebSocket-Key");
		}

		private string ComputeHandshake(string key) {
			var challenge = key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
			byte[] handshakeBytes = null;
			using (var sha1 = SHA1.Create()) {
				handshakeBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(challenge));
			}
			return Convert.ToBase64String(handshakeBytes);
		}

		private void WriteMessage(NetworkStream stream, string message) {
			var data = new ServerFrame(new TextMessage(message)).ToBytes();
			stream.Write(data, 0, data.Length);
		}

		public void Emit(string message) {
			List<NetworkStream> clients = null;
			lock (Clients) { clients = new List<NetworkStream>(Clients); }
			foreach (var each in clients) {
				try {
					WriteMessage(each, message);
				} catch (Exception ex) {
					Console.WriteLine(ex.Message);
					each.Close();
					Clients.Remove(each);
				}
			}
		}
	}
}
