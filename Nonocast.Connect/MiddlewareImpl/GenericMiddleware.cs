using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Nonocast.Connect {
	public class GenericMiddleware : Middleware {
		public GenericMiddleware(RequestAction handle) {
			this.handle = handle;
		}

		public void Handle(Request req, Response res) {
			handle(req, res);
		}

		private RequestAction handle;
	}

	public class ErrorMiddleware : Middleware, ErrorHandler {
		public ErrorMiddleware(RequestAction handle) {
			this.handle = handle;
		}

		public void Handle(Request req, Response res) {
			if (res.Error == null) return;
			handle(req, res);
		}

		private RequestAction handle;
	}
}
