using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Nonocast.Connect {
	public class Response {
		public Exception Error { get; set; }
		public Dictionary<string, string> Context { get; private set; }
		public bool Done { get; private set; }
		public int Code { get; set; }
		public NetworkStream Stream { get; private set; }
		public bool Reserved { get; private set; }	// for websocket

		public Response Header(string field, object value) {
			headers[field] = value;
			return this;
		}

		public Response Status(int code) {
			this.Code = code;
			return this;
		}

		// .cshtml for Razor
		public Response Render(string view, object model = null) {
			var fullpath = Path.Combine(this.Context["view"], view + ".cshtml");

			if (File.Exists(fullpath)) {
				var razor = File.ReadAllText(fullpath, Encoding.UTF8);
				return Html(RazorEngine.Razor.Parse(razor, model));
			}

			return this;
		}

		public Response Html(string body) {
			this.headers["Content-Type"] = "text/html";
			return Send(body);
		}

		public Response Send(object body) {
			this.headers["Content-Type"] = "application/json; charset=utf-8";
			return Send(JsonConvert.SerializeObject(body, Formatting.Indented));
		}

		public Response Send(string body) {
			var data = Encoding.UTF8.GetBytes(body);
			this.headers["Content-Length"] = data.Length;
			WriteHeader();
			Stream.Write(data, 0, data.Length);
			Stream.Close();
			this.Done = true;
			return this;
		}

		public Response SendFile(string path) {
			this.headers["Content-Type"] = Mimes[Path.GetExtension(path)];
			this.headers["Content-Length"] = new FileInfo(path).Length;
			WriteHeader();
			using (var fs = new FileStream(path, FileMode.Open)) {
				fs.CopyTo(Stream);
			}
			Stream.Close();
			this.Done = true;
			return this;
		}

		public Response Json(object body) {
			return Send(body);
		}

		public Response End() {
			WriteHeader();
			Stream.Close();
			this.Done = true;
			return this;
		}

		public Response Reserve() {
			this.Reserved = true;
			return JustDone();
		}

		public Response JustDone() {
			this.Done = true;
			return this;
		}

		public Response WriteHeader(RequestHeader header) {
			var data = Encoding.ASCII.GetBytes(header.ToString());
			Stream.Write(data, 0, data.Length);
			return this;
		}

		private Response WriteHeader() {
			var header = new StringBuilder();
			foreach (var each in headers) {
				header.AppendFormat("{0}: {1}\n", each.Key, each.Value);
			}
			string p = string.Format("HTTP/1.1 {0} {1}\n{2}\n", this.Code, Codes[this.Code], header);

			var data = Encoding.UTF8.GetBytes(p);
			Stream.Write(data, 0, data.Length);
			return this;
		}

		private Response() {
			this.Done = false;
			this.Code = 200;
			this.headers = new Dictionary<string, object>();
			this.headers.Add("Content-Type", "plain/text");
			this.Context = new Dictionary<string, string>();
		}

		public Response(NetworkStream stream)
			: this() {
			this.Stream = stream;
		}

		static Response() {
			// http://zh.wikipedia.org/wiki/HTTP状态码
			Codes.Add(200, "OK");
			Codes.Add(201, "Created");
			Codes.Add(202, "Accepted");
			Codes.Add(301, "Moved Permanently");
			Codes.Add(302, "Found");
			Codes.Add(303, "See Other");
			Codes.Add(304, "Not Modified");
			Codes.Add(400, "Bad Request");
			Codes.Add(401, "Unauthorized");
			Codes.Add(402, "Payment Required");
			Codes.Add(403, "Forbidden");
			Codes.Add(404, "Not Found");
			Codes.Add(405, "Method Not Allowed");
			Codes.Add(406, "Not Acceptable");
			Codes.Add(408, "Request Timeout");
			Codes.Add(409, "Conflict");
			Codes.Add(410, "Gone");
			Codes.Add(500, "Internal Server Error");
			Codes.Add(501, "Not Implemented");
			Codes.Add(502, "Bad Gateway");
			Codes.Add(503, "Service Unavailable");

			Mimes.Add(".jpg", "image/jpeg");
			Mimes.Add(".png", "image/png");
			Mimes.Add(".ico", "image/ico");
			Mimes.Add(".txt", "text/plain");
			Mimes.Add(".css", "text/css");
			Mimes.Add(".html", "text/html");
			Mimes.Add(".htm", "text/html");
			Mimes.Add(".mp4", "text/html");
			Mimes.Add(".js", "application/javascript");
			Mimes.Add("mp3", "audio/mpeg");
			Mimes.Add("mp4", "video/mp4");
			Mimes.Add("mpeg", "video/mpeg");
			Mimes.Add("mpg", "video/mpeg");
			Mimes.Add("zip", "application/zip");
			Mimes.Add("avi", "video/x-msvideo");
			Mimes.Add("bmp", "image/bmp");
			Mimes.Add("jpeg", "image/jpeg");
			Mimes.Add("ppt", "application/vnd.ms-powerpoint");
			Mimes.Add("xml", "application/xml");
			Mimes.Add("xls", "application/vnd.ms-excel");
			Mimes.Add("torrent", "application/octet-stream");
			Mimes.Add("flv", "video/x-flv");
			Mimes.Add("wma", "audio/x-ms-wma");
			Mimes.Add("wmv", "video/x-ms-wmv");
			Mimes.Add("apk", "application/vnd.android.package-archive");
		}

		private Dictionary<string, object> headers;
		private static Dictionary<int, string> Codes = new Dictionary<int, string>();
		private static Dictionary<string, string> Mimes = new Dictionary<string, string>();
	}

}
