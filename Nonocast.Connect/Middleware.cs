using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Nonocast.Connect {
	public interface Middleware {
		void Handle(Request req, Response res);
	}

	public interface ErrorHandler {

	}

	public interface IWebSocketServer : Middleware {
		event Action<string> MessageReceived;
		void Emit(string message);
	}
}
