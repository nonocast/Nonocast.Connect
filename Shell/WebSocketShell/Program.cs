using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Nonocast.Connect.WebSocket.Shell {
	class Program {
		static void Main(string[] args) {
			//var ws = new WebSocket("ws://localhost:8000/x");
			var ws = new WebSocket("ws://192.168.10.251:8080/x");
			ws.MessageReceived += (message) => Console.WriteLine(message);
			ws.Connecting += () => Console.WriteLine("Connecting");
			ws.ConnectionChanged += (state) => {
				if(state) Console.WriteLine("Connected");
				else Console.WriteLine("Disconnected");
			};
			ws.Open();

			string line = Console.ReadLine();
			while(true) {
				if(line == "exit") break;
				try { ws.Emit(line); } catch { }
			}

			ws.Close();
		}
	}
}
