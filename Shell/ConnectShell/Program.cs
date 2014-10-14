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
			app.Use(new BodyParser().Json());
			app.Get("/", (req, res) => { res.Html("<h1>hello world</h1>"); });
			app.Post("/login", (req, res) => {
				Console.WriteLine(req.Body);
				res.Status(200).End();
			});
			app.Get("/bala", (req, res) => { ws.Emit("balabala..."); res.Html("OK"); });
			//app.Post("/session/:id", (req, res) => { });
			app.Patch("/session/:id", (req, res) => {
				res.Send(req.Params["id"]);
			});

			app.Use(new GenericMiddleware((req, res) => { res.Status(404).End(); }));
			app.Use(new ErrorMiddleware((req, res) => { res.Status(500).End(); }));

			var server = new Server(app).Listen(new int[] { 8000 });

			Console.WriteLine("listening on port {0}", server.Port);
			Console.ReadLine();
		}
	}
}
