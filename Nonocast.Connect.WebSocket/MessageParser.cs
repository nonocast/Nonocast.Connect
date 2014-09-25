using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nonocast.Connect.WebSocket {
	public class MessageParser {
		public event Action<string> MessageReceived;

		public MessageParser() {
			buffer = new ByteQueue();
		}

		public void Push(byte[] fragment, int count) {
			var p = new byte[count];
			Array.Copy(fragment, p, count);
			buffer.Enqueue(p);

			string message = null;
			while ((message = Scan()) != null) {
				if (MessageReceived != null) MessageReceived(message);
			}
		}

		private string Scan() {
			if (buffer.Count > 2) {
				var header = buffer.Peek(2);
				var length = (int)header[1];
				if (buffer.Count >= 2 + length) {
					// frame ok
					buffer.DequeueByte();
					buffer.DequeueByte();
					return Encoding.UTF8.GetString(buffer.Dequeue(length));
				}
			}
			return null;
		}

		private void Reset() {

		}

		private ByteQueue buffer;
	}
}
