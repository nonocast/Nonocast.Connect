using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Nonocast.Connect.WebSocket.Contract {
	static public class ByteHelper {
		static public byte[] Merge(params byte[][] arg) {
			int length = 0;
			foreach(byte[] each in arg) {
				length += each.Length;
			}

			byte[] result = new byte[length];

			int current = 0;
			foreach(byte[] each in arg) {
				Array.Copy(each, 0, result, current, each.Length);
				current += each.Length;
			}
			return result;
		}

		static public byte[] Reverse(byte[] arg) {
			byte[] result = new byte[arg.Length];
			Array.Copy(arg, result, arg.Length);
			Array.Reverse(result);
			return result;
		}

		static public string ToString(byte[] data) {
			return ToString(data, data.Length);
		}

		static public string ToString(byte[] data, int count) {
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < count; ++i) {
				sb.AppendFormat("{0:x2} ", data[i]);
			}
			return sb.ToString().Trim();
		}

		static public bool Equal(byte[] arg1, byte[] arg2) {
			if(arg1.Length != arg2.Length) return false;

			for(int i = 0; i < arg1.Length; ++i) {
				if(arg1[i] != arg2[i]) {
					return false;
				}
			}

			return true;
		}

		static public byte[] GetBytes(short arg) {
			byte[] result = new byte[2];
			result[0] = (byte)(arg & 0x00ff);		//  low
			result[1] = (byte)(arg >> 8 & 0xff);	// high
			return result;
		}

		static public short GetShort(byte[] data) {
			if(data.Length != 2) throw new ArgumentException();
			return (short)(data[1] << 8 | data[0]);
		}

		static public short GetShort(byte high, byte low) {
			return GetShort(new byte[] { high, low });
		}

		static public byte[] ToBytes(object obj) {
			int objSize = Marshal.SizeOf(obj);
			byte[] result = new byte[objSize];

			IntPtr buffer = Marshal.AllocHGlobal(objSize);
			Marshal.StructureToPtr(obj, buffer, true);

			Marshal.Copy(buffer, result, 0, objSize);
			Marshal.FreeHGlobal(buffer);

			return result;

		}

		static public void FromBytes(object obj, byte[] data) {
			int size = Marshal.SizeOf(obj);
			IntPtr buffer = Marshal.AllocHGlobal(size);
			try {
				Marshal.Copy(data, 0, buffer, size);
				Marshal.PtrToStructure(buffer, obj);
			} catch {
				
			} finally {
				Marshal.FreeHGlobal(buffer);
			}
		}
	}
}
