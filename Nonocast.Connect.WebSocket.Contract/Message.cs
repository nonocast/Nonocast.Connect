using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nonocast.Connect.WebSocket.Contract {
	public interface Message {
		bool IsFinal { get; set; }
		byte Opcode { get; }
		byte[] ToBytes();
	}

	public abstract class MessageBase : Message {
		public bool IsFinal { get { return final; } set { final = value; } }
		public abstract byte Opcode { get; }
		public abstract byte[] ToBytes();

		public MessageBase() {

		}

		protected bool final = true;
	}

	public class TextMessage : MessageBase {
		public override byte Opcode { get { return 0x01; } }

		public string Content { get; set; }

		public TextMessage() : base() { }

		public TextMessage(string content)
			: this() {
			this.Content = content;
		}

		public override byte[] ToBytes() {
			return Encoding.UTF8.GetBytes(Content);
		}

		public static Message Parse(byte[] data) {
			return new TextMessage(Encoding.UTF8.GetString(data));
		}
	}
}
