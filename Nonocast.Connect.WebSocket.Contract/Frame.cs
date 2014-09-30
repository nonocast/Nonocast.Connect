using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nonocast.Connect.WebSocket.Contract {
	public interface Frame {
		Message Message { get; }
		byte[] ToBytes();
	}

	public abstract class FrameBase : Frame {
		public Message Message { get; private set; }

		public FrameBase() { }

		public FrameBase(Message message)
			: this() {
			this.Message = message;
		}

		public byte[] ToBytes() {
			var payload = Message.ToBytes();

			using (var stream = new MemoryStream()) {
				WriteStartByte(stream);
				WritePayloadLength(stream, payload.Length);
				WriteMaskFlag(stream);
				WriteMask(stream);
				WritePayload(stream, payload);
				return stream.ToArray();
			}
		}

		protected virtual void WriteMaskFlag(Stream stream) { }

		protected virtual void WriteMask(Stream stream) { }

		protected virtual void WritePayload(Stream stream, byte[] payload) {
			stream.Write(payload, 0, payload.Length);
		}

		private void WritePayloadLength(Stream stream, int payload) {
			if (payload < 0x7E) {
				stream.WriteByte((byte)payload);
			} else if (payload < 0x10000) {
				stream.WriteByte(0x7E);
				var bytes = BitConverter.GetBytes(payload);
				stream.Write(new byte[] { bytes[1], bytes[0] }, 0, 2);
			} else {
				stream.WriteByte(0x7F);
				var bytes = BitConverter.GetBytes(payload);
				stream.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, bytes[3], bytes[2], bytes[1], bytes[0] }, 0, 8);
			}
		}

		private void WriteStartByte(Stream stream) {
			byte result = 0x00;
			if (Message.IsFinal) {
				result |= 0x80;
				result |= Message.Opcode;
			}
			stream.WriteByte(result);
		}
	}

	public class ClientFrame : FrameBase {
		public ClientFrame(Message message)
			: base(message) {
			this.masks = new byte[4];
			random.NextBytes(this.masks);
		}

		protected override void WriteMaskFlag(Stream stream) {
			if (stream is MemoryStream) {
				var p = stream as MemoryStream;
				if (p.Length >= 2) {
					p.GetBuffer()[1] |= 0x80;
				}
			}
		}

		protected override void WriteMask(Stream stream) {
			stream.Write(this.masks, 0, this.masks.Length);
		}

		protected override void WritePayload(Stream stream, byte[] payload) {
			for (var i = 0; i < payload.Length; i++) {
				payload[i] = (byte)(payload[i] ^ masks[i % 4]);
			}
			base.WritePayload(stream, payload);
		}


		private byte[] masks;
		private Random random = new Random((int)DateTime.Now.Ticks);
	}

	public class ServerFrame : FrameBase {
		public ServerFrame(Message message)
			: base(message) {

		}
	}

	public class CloseFrame : FrameBase {

	}
}
