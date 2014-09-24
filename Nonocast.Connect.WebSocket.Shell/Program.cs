using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nonocast.Connect.WebSocket.Shell {
	class Program {
		static void Main(string[] args) {
			var ws = new WebSocket("ws://localhost:8000/x");
			ws.Open();

			Console.WriteLine("press any key to exit.");
			Console.ReadLine();
			ws.Close();
		}
	}
}
