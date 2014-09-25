using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

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

			//Console.WriteLine(req.Header.StartLine);
			//foreach (var each in req.Header.Properties) {
			//	Console.WriteLine("{0}: {1}", each.Key, each.Value);
			//}

			var handshake = ComputeHandshake(req.Header.Properties["Sec-WebSocket-Key"]);
			WriteStartLine(stream, "HTTP/1.1 101 Switching Protocols");
			WriteHeaders(stream, new Dictionary<string, string> {
						{"Upgrade", "websocket"},
						{"Connection", "Upgrade"},
						{"Sec-WebSocket-Accept", handshake}});

			Clients.Add(stream);
			ConsoleHelper.WriteLine(ConsoleColor.White, "WebSocket running...");

			byte[] buffer = new byte[4096];
			int readCount = 0;

			while ((readCount = stream.Read(buffer, 0, buffer.Length)) > 0) {
				string message = ParseReceiveData(buffer, readCount);
				if (!string.IsNullOrEmpty(message) && MessageReceived != null) MessageReceived(message);
			}
		}

		private bool IsMatch(Request req) {
			var header = req.Header;
			return header.Properties.ContainsKey("Connection") && header.Properties.ContainsKey("Upgrade") &&
				   header.Properties["Connection"].ToLower().Contains("upgrade") && header.Properties["Upgrade"].ToLower() == "websocket" &&
				   header.Properties.ContainsKey("Sec-WebSocket-Key");
		}

		private static void WriteStartLine(NetworkStream stream, string arg) {
			arg += Environment.NewLine;
			byte[] buffer = Encoding.ASCII.GetBytes(arg);
			stream.Write(buffer, 0, buffer.Length);
		}

		private void WriteHeaders(NetworkStream stream, Dictionary<string, string> headers) {
			var sb = new StringBuilder();
			foreach (var each in headers) {
				sb.AppendLine(string.Format("{0}: {1}", each.Key, each.Value));
			}
			sb.Append(Environment.NewLine);
			byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());
			stream.Write(buffer, 0, buffer.Length);
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
			var messageBuffer = Encoding.UTF8.GetBytes(message);
			if (messageBuffer.Length < 126) {
				var buffer = new Byte[messageBuffer.Length + 2];
				buffer[0] = 0x81;
				buffer[1] = (byte)messageBuffer.Length;
				Array.Copy(messageBuffer, 0, buffer, 2, messageBuffer.Length);
				stream.Write(buffer, 0, buffer.Length);
				//stream.WriteByte(0x81);
				//stream.WriteByte((byte)messageBuffer.Length);
				//stream.Write(messageBuffer, 0, messageBuffer.Length);
			}
		}

		public void Emit(string message) {
			List<NetworkStream> clients = null;
			lock (Clients) { clients = new List<NetworkStream>(Clients); }
			foreach (var each in clients) {
				try {
					WriteMessage(each, message);
				} catch {
					each.Close();
				}
			}
		}

		private string ParseReceiveData(byte[] recBytes, int recByteLength) {
			if (recByteLength < 2)
				return null;

			bool fin = (recBytes[0] & 0x80) == 0x80; // 1bit，1表示最后一帧  
			if (!fin) {
				Console.WriteLine("recData exception: 超过一帧"); // 超过一帧暂不处理  
				return null;
			}

			bool mask_flag = (recBytes[1] & 0x80) == 0x80; // 是否包含掩码  
			if (!mask_flag) {
				Console.WriteLine("recData exception: 没有Mask"); // 不包含掩码的暂不处理  
				return null;
			}

			int payload_len = recBytes[1] & 0x7F; // 数据长度  

			byte[] masks = new byte[4];
			byte[] payload_data;
			if (payload_len == 126) {
				Array.Copy(recBytes, 4, masks, 0, 4);
				payload_len = (UInt16)(recBytes[2] << 8 | recBytes[3]);
				payload_data = new byte[payload_len];
				Array.Copy(recBytes, 8, payload_data, 0, payload_len);
			} else if (payload_len == 127) {
				Array.Copy(recBytes, 10, masks, 0, 4);
				byte[] uInt64Bytes = new byte[8];
				for (int i = 0; i < 8; i++) {
					uInt64Bytes[i] = recBytes[9 - i];
				}
				UInt64 len = BitConverter.ToUInt64(uInt64Bytes, 0);

				payload_data = new byte[len];
				for (UInt64 i = 0; i < len; i++)
					payload_data[i] = recBytes[i + 14];
			} else {
				Array.Copy(recBytes, 2, masks, 0, 4);
				payload_data = new byte[payload_len];
				Array.Copy(recBytes, 6, payload_data, 0, payload_len);
			}

			for (var i = 0; i < payload_len; i++)
				payload_data[i] = (byte)(payload_data[i] ^ masks[i % 4]);


			return Encoding.UTF8.GetString(payload_data);
		}
	}
}
