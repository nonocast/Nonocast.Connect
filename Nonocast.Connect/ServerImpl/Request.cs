using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Nonocast.Connect {
	public class Request {
		public RequestHeader Header { get; private set; }
		public NetworkStream Stream { get; private set; }
		public string Url { get; private set; }
		public string Method { get; private set; }
		public string Path { get; private set; }
		public Dictionary<string, string> Params { get; private set; }
		public Dictionary<string, string> Query { get; private set; }
		public object Body { get; set; }
		public byte[] Raw { get; set; }

		private Request() {

		}

		public Request(RequestHeader header, NetworkStream stream)
			: this() {
			this.Header = header;
			this.Stream = stream;

			this.Method = this.Header.Method;
			//this.Url = this.Header.RequestUri;
			this.Url = Uri.UnescapeDataString(this.Header.RequestUri);

			this.Params = new Dictionary<string, string>();
			this.Query = new Dictionary<string, string>();

			string query = string.Empty;
			this.Path = this.Url.Split(new char[] { '?' }).FirstOrDefault();
			if (this.Url.Contains('?')) {
				query = this.Url.Substring(this.Url.IndexOf('?'));
				query = query.TrimStart(new char[] { '?' });
			}

			string qp = @"(?<key>[^=&]+)(=(?<value>[^&]*))?";
			var ms = Regex.Matches(query, qp);
			foreach (Match each in ms) {
				this.Query[each.Groups["key"].ToString()] = each.Groups["value"].ToString();
			}
		}
	}
}
