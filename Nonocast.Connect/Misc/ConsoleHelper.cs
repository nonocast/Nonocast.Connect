using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nonocast.Connect {
	public static class ConsoleHelper {
		public static bool Enable { get; set; }

		public static void WriteLine(ConsoleColor fg, string value) {
			if (!Enable) return;
			lock (locker) {
				Console.ForegroundColor = fg;
				Console.Write("[{0}]:\t", Thread.CurrentThread.ManagedThreadId);
				Console.WriteLine(value);
				Restore();
			}
		}

		public static void WriteLine(ConsoleColor fg, string format, object arg0) {
			if (!Enable) return;
			lock (locker) {
				Console.ForegroundColor = fg;
				Console.Write("[{0}]:\t", Thread.CurrentThread.ManagedThreadId);
				Console.WriteLine(format, arg0);
				Restore();
			}
		}

		public static void WriteLine(ConsoleColor fg, string format, params object[] arg) {
			if (!Enable) return;
			lock (locker) {
				Console.ForegroundColor = fg;
				Console.Write("[{0}]:\t", Thread.CurrentThread.ManagedThreadId);
				Console.WriteLine(format, arg);
				Restore();
			}
		}

		public static void Write(RequestHeader header) {
			if (!Enable) return;
			lock (locker) {
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine("[{0}]:\tServiceRequestHeader >>>", Thread.CurrentThread.ManagedThreadId);
				// Console.WriteLine(header);
				Console.WriteLine(header.StartLine);
				Console.WriteLine("<<<");
				Restore();
			}
		}

		private static void Restore() {
			Console.BackgroundColor = defaultbg;
			Console.ForegroundColor = defaultfg;
		}

		static ConsoleHelper() {
			Enable = true;

			defaultbg = Console.BackgroundColor;
			defaultfg = Console.ForegroundColor;
		}

		private static ConsoleColor defaultfg;
		private static ConsoleColor defaultbg;
		private static object locker = new object();
	}
}
