using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Nonocast.Connect {
	public class WebApp : WebApplication {
		public List<Middleware> Middlewares { get; private set; }
		public Dictionary<string, string> Config { get; private set; }

		public WebApplication Use(Middleware middleware) {
			this.Middlewares.Add(middleware);
			return this;
		}

		public void Set(string field, string value) {
			Config[field] = value;
		}

		public string Get(string field) {
			return Config[field];
		}

		public void Handle(Request req, Response res) {
			res.Context["view"] = this.Config["view"];

			foreach (var each in this.Middlewares) {
				if (res.Done) break;

				if (res.Error != null && !(each is ErrorHandler)) {
					continue;
				}

				try {
					each.Handle(req, res);
				} catch (Exception ex) {
					res.Error = ex;
				}
			}
		}

		public void Get(string pattern, RequestAction handle) {
			this.Middlewares.Add(new Router("GET", pattern, handle));
		}

		public void Post(string pattern, RequestAction handle) {
			this.Middlewares.Add(new Router("POST", pattern, handle));
		}

		public void Put(string pattern, RequestAction handle) {
			this.Middlewares.Add(new Router("PUT", pattern, handle));
		}

		public void Delete(string pattern, RequestAction handle) {
			this.Middlewares.Add(new Router("DELETE", pattern, handle));
		}

		public void Head(string pattern, RequestAction handle) {
			this.Middlewares.Add(new Router("HEAD", pattern, handle));
		}

		public WebApp() {
			this.Config = new Dictionary<string, string>();
			this.Config["view"] = "./view";
			this.Middlewares = new List<Middleware>();
		}
	}
}
