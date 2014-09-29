using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Nonocast.Connect.WebSocket.Contract {
	public class FrameParser {
		public event Action<Message> MessageReceived;
		public event Action Close;

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
			int maskBytes = 0;

			var secondByte = buffer.ReadByte();
			var lengthFirstByte = secondByte & 0x7F;
			maskBytes = (secondByte & 0x80) == 0x80 ? 4 : 0;
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
			int frameLength = 1 + payloadLengthBytes + maskBytes + payloadLength;
			if (buffer.Length >= frameLength) {
				byte[] frameBuffer = new byte[frameLength];
				buffer.Position = 0;
				buffer.Read(frameBuffer, 0, frameLength);

				try {
					var frame = CreateFrame(frameBuffer);
					if (frame is CloseFrame) {
						if (Close != null) Close();
					}
					result = frame.Message;
				} catch (Exception ex) {
					Console.WriteLine(ex.Message);
				}

				// truncate
				buffer.Position = 0;
				buffer.Write(buffer.GetBuffer(), frameLength, (int)buffer.Length - frameLength);
				buffer.SetLength(buffer.Length - frameLength);
			}

			buffer.Seek(0, SeekOrigin.End);

			return result;
		}

		private Frame CreateFrame(byte[] data) {
			if (data.Length < 2) throw new InvalidDataException();

			byte opcode = (byte)(data[0] & 0x0F);

			Frame result = null;

			int offset = 1;

			byte firstByteOfPayloadLength = (byte)(data[1] & 0x7F);
			if (firstByteOfPayloadLength == 0x7E) {
				offset += 3;
			} else if (firstByteOfPayloadLength == 0x7F) {
				offset += 9;
			} else {
				offset += 1;
			}

			int maskBytes = (data[1] & 0x80) == 0x80 ? 4 : 0;
			offset += maskBytes;

			var payload = new byte[data.Length - offset];
			Array.Copy(data, offset, payload, 0, data.Length - offset);

			if (opcode == 0x01) {
				if (maskBytes > 0) {
					byte[] masks = new byte[4];
					Array.Copy(data, offset - 4, masks, 0, 4);

					for (var i = 0; i < payload.Length; i++) {
						payload[i] = (byte)(payload[i] ^ masks[i % 4]);
					}
				}
				var message = TextMessage.Parse(payload);
				if (maskBytes > 0) {
					result = new ClientFrame(message);
				} else {
					result = new ServerFrame(message);
				}
			} else if (opcode == 0x08) {
				result = new CloseFrame();
			}

			return result;
		}

		private void Reset() {

		}

		private MemoryStream buffer;
	}

}
