using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nonocast.Connect {
	public class Router : Middleware {
		public string Method { get; private set; }
		public string Pattern { get; private set; }
		public RequestAction InnerHandle { get; private set; }

		public Router(string method, string pattern, RequestAction handle) {
			this.Method = method;
			this.Pattern = pattern;
			this.InnerHandle = handle;
		}

		public void Handle(Request req, Response res) {
			if (IsMatchMethod(req) && IsMatchPattern(req)) {
				InnerHandle(req, res);
			}
		}

		private bool IsMatchMethod(Request req) {
			return req.Method == this.Method;
		}

		private bool IsMatchPattern(Request req) {
			string path = string.Empty;
			string query = string.Empty;

			var trimChars = new char[] { '/' };
			string p = Regex.Replace(Pattern.Trim(trimChars), @":[\w-]+", m => {
				var key = m.Groups[0].Value.Substring(1);
				return string.Format(@"(?<{0}>[\w-]+)", key);
			});

			p = string.Format("^{0}$", p);

			Regex r = new Regex(p);
			Match match = r.Match(req.Path.Trim(trimChars));
			if (match.Success) {
				req.Params.Clear();
				foreach (string groupName in r.GetGroupNames()) {
					if (groupName != "0") {
						req.Params[groupName] = match.Groups[groupName].Value;
					}
				}
			}

			return match.Success;
		}
	}
}
