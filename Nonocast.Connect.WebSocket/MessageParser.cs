using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nonocast.Connect.WebSocket.Contract;

namespace Nonocast.Connect.WebSocket {
	public class FrameParser {
		public event Action<Message> MessageReceived;

		public FrameParser() {
			buffer = new MemoryStream();
		}

		public void Push(byte[] data, int count) {
			buffer.Seek(0, SeekOrigin.End);
			buffer.Write(data, 0, count);

			Message message = null;
			while ((message = Scan()) != null) {
				if (MessageReceived != null) MessageReceived(message);
			}
		}

		private Message Scan() {
			Message result = null;

			if (buffer.Length < 1 + 1) return null;

			buffer.Seek(1, SeekOrigin.Begin);
			int payloadLength = 0;
			int payloadLengthBytes = 0;

			var lengthFirstByte = buffer.ReadByte();
			if (lengthFirstByte == 0x7E) {
				if (buffer.Length < 1 + 3) return null;
				payloadLengthBytes = 3;
				payloadLength = buffer.ReadByte() << 8 | buffer.ReadByte();
			} else if (lengthFirstByte == 0x7F) {
				if (buffer.Length < 1 + 9) return null;
				payloadLengthBytes = 9;
				buffer.Seek(4, SeekOrigin.Current);
				payloadLength = buffer.ReadByte() << 24 | buffer.ReadByte() << 16 | buffer.ReadByte() << 8 | buffer.ReadByte();
			} else {
				// < 0x7E (126)
				payloadLengthBytes = 1;
				payloadLength = lengthFirstByte;
			}

			// frame ok
			int frameLength = 1 + payloadLengthBytes + payloadLength;
			if (buffer.Length >= frameLength) {
				byte[] frameBuffer = new byte[frameLength];
				buffer.Position = 0;
				buffer.Read(frameBuffer, 0, frameLength);
				result = ServerFrame.Parse(frameBuffer).Message;

				// truncate
				buffer.Position = 0;
				buffer.Write(buffer.GetBuffer(), frameLength, (int)buffer.Length - frameLength);
				buffer.SetLength(buffer.Length - frameLength);
			}

			buffer.Seek(0, SeekOrigin.End);

			return result;
		}

		private void Reset() {

		}

		private MemoryStream buffer;
	}
}
