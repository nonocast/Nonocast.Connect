using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nonocast.Connect.WebSocket.Shell {
	class Program {
		static void Main(string[] args) {
			var ws = new WebSocket("ws://localhost:8000/x");
			ws.MessageReceived += (message) => Console.WriteLine(message);
			ws.Open();
			ws.Emit("hello world");

			Console.WriteLine("press any key to exit.");
			string line = null;
			while ((line = Console.ReadLine()) != null) {
				ws.Emit("world hello");
			}
			ws.Close();
		}
	}
}
