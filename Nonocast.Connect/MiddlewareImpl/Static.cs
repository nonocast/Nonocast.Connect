using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nonocast.Connect {
	public class Static : Middleware {
		public string Root { get; private set; }
		public Static(string root) {
			this.Root = root;
		}

		public void Handle(Request req, Response res) {
			if ("GET" != req.Method && "HEAD" != req.Method) return;
			var path = Path.Combine(this.Root, req.Path.TrimStart(new char[] { '/' }));
			if (File.Exists(path)) {
				res.SendFile(path);
			}
		}
	}
}