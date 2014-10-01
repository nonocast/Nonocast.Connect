using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Nonocast.Connect.Shell {
	public class Program {
		static void Main(string[] args) {
			log4net.Config.XmlConfigurator.Configure();

			IWebSocketServer ws = new WebSocket6455();
			ws.MessageReceived += (message) => { Console.WriteLine(message); };

			var app = new Nonocast.Connect.WebApp();
			app.Use(ws);
			app.Use(new Morgan());
			app.Get("/", (req, res) => { res.Html("<h1>hello world</h1>"); });
			app.Get("/bala", (req, res) => { ws.Emit("balabala..."); res.Html("OK"); });

			var server = new Server(app).Listen(new int[] { 8000 });

			Console.WriteLine("listening on port {0}", server.Port);
			Console.ReadLine();
		}
	}
}
