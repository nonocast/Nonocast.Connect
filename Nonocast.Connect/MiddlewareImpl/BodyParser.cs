using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Nonocast.Connect {
	// json, urlencoded, multipart
	public class BodyParser : Middleware {
		public void Handle(Request req, Response res) {
			if (req.Header.Properties.ContainsKey("Content-Type") && req.Header.Properties.ContainsKey("Content-Length")) {
				try {
					var contentType = req.Header.Properties["Content-Type"].ToLower();

					if (contentType == "application/json") {
						if (json != null) json(req, res);
					} else if (contentType == "application/x-www-form-urlencoded") {
						if (urlencoded != null) urlencoded(req, res);
					} else if (contentType == "multipart/form-data") {
						if (multipart != null) multipart(req, res);
					}
				} catch {
					res.Status(500).End();
				}
			}
		}

		public BodyParser Json() {
			json = (req, res) => {
				var contentLength = Convert.ToInt32(req.Header.Properties["Content-Length"]);
				byte[] buffer = new byte[contentLength];
				req.Stream.Read(buffer, 0, contentLength);
				req.Raw = buffer;
				req.Body = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(buffer));
			};
			return this;
		}

		public BodyParser Urlencoded() {
			urlencoded = (req, res) => { };
			return this;
		}

		public BodyParser Multipart() {
			multipart = (req, res) => { };
			return this;
		}

		private Action<Request, Response> json;
		private Action<Request, Response> urlencoded;
		private Action<Request, Response> multipart;
	}
}
