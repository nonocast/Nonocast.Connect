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
			ws.ConnectionChanged += (isConnected) => {
				if(isConnected)
					Console.WriteLine("Connected");
				else
					Console.WriteLine("Disconnected");
			};
			ws.Connecting += () => Console.WriteLine("Connecting...");
			ws.Open();

			Console.WriteLine("press any key to exit.");
			while(true) {
				try {
					string line = Console.ReadLine();
					if(line == "exit") break;
					ws.Emit(line);
				} catch(SocketException) { }
			}
			ws.Close();
		}
	}
}
