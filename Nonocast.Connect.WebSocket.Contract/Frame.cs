﻿using System;
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
				stream.Write(payload, 0, payload.Length);
				return stream.ToArray();
			}
		}
		protected virtual void WriteMaskFlag(Stream stream) { }

		protected virtual void WriteMask(Stream stream) { }

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
				stream.Write(new byte[] { bytes[7], bytes[6], bytes[5], bytes[4] }, 0, 4);
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

		}

		protected override void WriteMaskFlag(Stream stream) {
			stream.Seek(2, SeekOrigin.Begin);
			var p = stream.ReadByte();
			stream.Seek(2, SeekOrigin.Begin);
			stream.WriteByte((byte)(p | 0x80));
			stream.Seek(0, SeekOrigin.End);
		}

		protected override void WriteMask(Stream stream) {
			var result = new byte[4];
			random.NextBytes(result);
			stream.Write(result, 0, result.Length);
		}

		private Random random = new Random((int)DateTime.Now.Ticks);
	}

	public class ServerFrame : FrameBase {
		public ServerFrame(Message message)
			: base(message) {

		}

		public static Frame Parse(byte[] data) {
			if (data.Length < 3) throw new InvalidDataException();

			ServerFrame result = null;

			int offset = 1;
			if (data[1] == 0x7E) {
				offset += 3;
			} else if (data[1] == 0x7F) {
				offset += 9;
			} else {
				offset += 1;
			}

			var payload = new byte[data.Length - offset];
			Array.Copy(data, offset, payload, 0, data.Length - offset);

			byte opcode = (byte)(data[0] & 0x01);

			if (opcode == 0x01) {
				var message = TextMessage.Parse(payload);
				result = new ServerFrame(message);
			}

			return result;
		}
	}
}