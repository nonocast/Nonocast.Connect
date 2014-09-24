using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Nonocast.Connect {
	public class Morgan : Middleware {
		private static readonly ILog logger = LogManager.GetLogger(typeof(Morgan));

		public void Handle(Request req, Response res) {
			logger.DebugFormat("{0} {1}", req.Method, req.Url);
		}
	}
}
