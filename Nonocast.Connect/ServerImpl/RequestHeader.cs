using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nonocast.Connect {
	public class RequestHeader {
		public string Method { get; private set; }
		public string RequestUri { get; private set; }

		public string StartLine {
			get { return startline; }
			set {
				startline = value;
				if (!string.IsNullOrEmpty(startline)) {
					// https://annevankesteren.nl/2007/10/http-methods
					var startlineRule = new Regex(@"^(GET|HEAD|POST|PUT|DELETE|PATCH|TRACE|CONNECT) (.+) HTTP/1.1$");
					if (startlineRule.IsMatch(startline)) {
						var match = startlineRule.Match(startline);
						Method = match.Groups[1].Value.Trim();
						RequestUri = match.Groups[2].Value.Trim();
					}
				}
			}
		}

		public Dictionary<string, string> Properties { get; set; }

		public RequestHeader() {
			Properties = new Dictionary<string, string>();
		}

		public RequestHeader(NetworkStream stream)
			: this() {
			do {
				// ConsoleHelper.WriteLine(ConsoleColor.DarkCyan, "+HEADER");
				StartLine = ReadLine(stream);
				// ConsoleHelper.WriteLine(ConsoleColor.DarkCyan, "-HEADER");
			} while (string.IsNullOrWhiteSpace(StartLine));

			string line = null;
			var propertyRule = new Regex(@"(.+?):(.+)");

			while (!string.IsNullOrEmpty(line = ReadLine(stream))) {
				if (propertyRule.IsMatch(line)) {
					var match = propertyRule.Match(line);
					Properties[match.Groups[1].Value.Trim()] = match.Groups[2].Value.Trim();
				}
			}
		}

		public static readonly string NewLine = "\r\n"; //websockets specs demand \r\n at header line end

		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append(StartLine);
			sb.Append (NewLine);
			foreach (var each in Properties) {
				sb.AppendFormat("{0}: {1}{2}", each.Key, each.Value, NewLine);
			}
			sb.Append (NewLine);
			return sb.ToString();
		}

		private string ReadLine(NetworkStream stream) {
			var sb = new StringBuilder();
			var buffer = new List<byte>();

			while (stream.CanRead) {
				// Console.Write(".");
				int value = stream.ReadByte();
				if (-1 == value) { break; }

				buffer.Add((byte)value);

				if (buffer.Last() == '\n') {
					var line = Encoding.Default.GetString(buffer.ToArray());
					return line.Substring(0, line.Length - 2);
				}
			}

			throw new InvalidOperationException();
		}

		private string startline;
	}
}