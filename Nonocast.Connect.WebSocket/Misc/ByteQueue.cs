using System;
using System.Collections.Generic;
using System.Text;

namespace Nonocast.Connect.WebSocket.Contract {
	public class ByteQueue {
		public int Count { get { return buffer.Count; } }
		public bool IsEmpty { get { return buffer.Count == 0; } }

		public void Enqueue(byte[] arg) {
			Enqueue(arg, arg.Length);
		}

		public void Enqueue(byte[] arg, int count) {
			Enqueue(arg, 0, count);
		}

		public void Enqueue(byte[] arg, int offset, int count) {
			if (count < 0 || offset < 0) throw new ArgumentException();

			for (int i = offset; i < count; ++i) {
				buffer.Add(arg[i]);
			}
		}

		public byte[] Dequeue() {
			return Dequeue(this.Count);
		}

		public byte[] Dequeue(int count) {
			if (count < 1) throw new ArgumentException();

			byte[] result = new byte[count];

			for (int i = 0; i < count; ++i) {
				result[i] = buffer[0];
				buffer.RemoveAt(0);
			}

			return result;
		}

		public void EnqueueByte(byte arg) {
			buffer.Add(arg);
		}

		public byte DequeueByte() {
			byte result = buffer[0];
			buffer.RemoveAt(0);
			return result;
		}

		public byte PeekByte() {
			if (IsEmpty) throw new InvalidOperationException();
			return buffer[0];
		}

		public override string ToString() {
			return ByteHelper.ToString(buffer.ToArray());
		}

		public byte[] Peek(int count) {
			return Peek(count, 0);
		}

		public byte[] Peek(int count, int offset) {
			if (count + offset > Count) throw new ArgumentException();

			byte[] result = new byte[count];
			for (int i = 0; i < count; ++i) {
				result[i] = buffer[i + offset];
			}
			return result;
		}

		private List<byte> buffer = new List<byte>();
	}
}
