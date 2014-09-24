using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Nonocast.Connect {
	public class Server {
		public int Port { get; private set; }

		private Server() {
			ConsoleHelper.Enable = true;
		}

		public Server(WebApplication app)
			: this() {
			this.app = app;
		}

		public Server Listen(int port) {
			return Listen(new int[] { port });
		}

		public Server Listen(int[] ports) {
			foreach (var each in ports) {
				try {
					listener = new TcpListener(IPAddress.Any, each);
					listener.Start();
					this.Port = each;
					break;
				} catch (SocketException) {
					// ignore, try next port
				}
			}


			listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), listener);
			return this;
		}

		public void Close() {
			listener.Stop();
		}

		private void AcceptCallback(IAsyncResult ar) {
			var listener = ar.AsyncState as TcpListener;

			try {
				var client = listener.EndAcceptTcpClient(ar);
				if (client != null) {
					new Thread(new ParameterizedThreadStart(NewTcpClient)).Start(client);
				}
			} catch (ObjectDisposedException ex) {
				// ignore
				ConsoleHelper.WriteLine(ConsoleColor.DarkRed, ex.Message);
			} finally {
				try { listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), listener); } catch { }
			}
		}

		private void NewTcpClient(object obj) {
			ConsoleHelper.WriteLine(ConsoleColor.Red, "Thread Enter\t(Total: {0})", ++threadCount);

			var client = obj as TcpClient;
			var stream = client.GetStream();

			Request req = null;
			Response res = null;

			try {
				var header = new RequestHeader(stream);
				ConsoleHelper.Write(header);
				req = new Request(header, stream);
				res = new Response(stream);
				if (app != null) app.Handle(req, res);
			} catch (Exception ex) {
				ConsoleHelper.WriteLine(ConsoleColor.Yellow, "Thread Exception - {0}", ex.Message);
			} finally {
				try {
					try { stream.Close(); } catch { }
					try { client.Close(); } catch { }
				} finally {
					ConsoleHelper.WriteLine(ConsoleColor.Green, "Thread Exit\t(Total: {0})", --threadCount);
				}
			}
		}

		private TcpListener listener;
		private static int threadCount;
		private WebApplication app;
	}

	public interface WebApplication {
		WebApplication Use(Middleware middleware);
		void Set(string field, string value);
		string Get(string field);
		void Handle(Request req, Response res);
	}

	public delegate void RequestAction(Request req, Response res);
}
