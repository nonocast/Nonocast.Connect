using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Nonocast.Connect.WebSocket.Shell {
	class Program {
		static void Main(string[] args) {
			var ws = new WebSocket("ws://localhost:8000/x");
			ws.MessageReceived += (message) => Console.WriteLine(message);
			ws.Open();

			Console.WriteLine("press any key to exit.");
			while(true) {
				try {
					Thread.Sleep(100);
					ws.Emit("world hello");
				} catch(SocketException) { }
			}
			ws.Close();
		}
	}
}
