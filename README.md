Nonocast.Connect
================

Nonocast.Connect的目标是让HTTP和WebSocket服务非常容易地嵌入桌面应用程序(.NET 4.0)，换句话说，让你桌面应用程序轻而易举的支持HTTP和WebSocket服务。框架设计参考了[senchalabs/connect](https://github.com/senchalabs/connect#readme)，采用Middleware指责链模型处理HTTP请求，所以Nonocast.Connect是一个基于"plugins"可扩展的HTTP服务框架。

Nonocast.Connect通过TCPListener自行实现HTTP协议解析，可根据需要自行修改实现，不受额外的约束和限制。通过自容器(self-contained), 无服务(serverless), 零配置(zero-configuration)使你的程序可以向上提供Restful API。

###你需要知道以下事项

- 基于.NET 4.0
- 采用C#编写
- 功能优先，效率不是第一目标
- 基于同步(sync)，而非异步(async)模型
- 可自行扩展中间件

###目前已实现的中间件

- Static
- Logger
- Router
- WebSocket
- ErrorHandler

###待实现的中间件

- BodyParser
- CookieParser
- SessionParser
- favicon
- fileuploader
- ...

###安装

Package-Install Nonocast.Connect

###起步

``` csharp
static void Main(string[] args) {
	var app = new Nonocast.Connect.WebApp();
	app.Get("/", (req, res) => { res.Html("<h1>hello world</h1>"); });

	var server = new Server(app).Listen(new int[] { 80, 8000, 7005 });

	Console.WriteLine("listening on port {0}", server.Port);
	Console.ReadLine();
}
```

如果需要启动WebSocket服务

``` csharp
static void Main(string[] args) {
	IWebSocketServer ws = new WebSocket6455();
	ws.MessageReceived += (message) => { Console.WriteLine(message); };

	var app = new Nonocast.Connect.WebApp();
	app.Use(ws);
	app.Get("/", (req, res) => { res.Html("<h1>hello world</h1>"); });
	app.Get("/bala", (req, res) => { ws.Emit("balabala..."); res.Html("OK"); });

	var server = new Server(app).Listen(new int[] { 8000 });

	Console.WriteLine("listening on port {0}", server.Port);
	Console.ReadLine();
}
```

###设计原理

``` csharp
public interface Middleware {
	void Handle(Request req, Response res);
}

public interface WebApplication {
	WebApplication Use(Middleware middleware);
	void Handle(Request req, Response res);
}


public class WebApp : WebApplication {
	public List<Middleware> Middlewares { get; private set; }

	public WebApplication Use(Middleware middleware) {
		this.Middlewares.Add(middleware);
		return this;
	}

	public void Handle(Request req, Response res) {
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
}
```

所以如果需要自定义Middleware只需要实现Middleware接口即可，比如Static，

```
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
```

然后在client方use即完成Middleware的插入。

Enjoy~

###页面渲染

目前Nonocast.Connect默认采用了Razor作为View渲染引擎。通过以下代码进行配置，

``` csharp
webapp.Set("view", <your-view-path>);
```

以Error.cshtml为例，
``` html
<doctype html>
<html>
<head>
  <title>error</title>
  <link rel="stylesheet" href="/stylesheets/style.css">
</head>
<body>
  <h1>@Model.error.Message</h1>
  <h2>@Model.error.Source</h2>
  <pre>@Model.error.StackTrace</pre>
</body>
</html>
```

Razor具体语法参考ASP.NET MVC相关资料。
通过Response.Render方法即可渲染页面，

``` csharp
app.Get("/", (req, res) => res.Render("index",  new { error = res.Error }));
```

###关于Nonocast.Connect.WebSocket

Nonocast.Connect.WebSocket是针对Nonocast.Connect中WebSocket服务所写的非常轻量级的WebSocket Client实现方式。
通过Nuget安装，

Install-Package Nonocast.Connect.WebSocket

``` csharp
static void Main(string[] args) {
	var ws = new WebSocket("ws://localhost:8000/x");
	ws.MessageReceived += (message) => Console.WriteLine(message);
	ws.Open();

	Console.WriteLine("press any key to exit.");
	Console.ReadLine();
	ws.Close();
}
```

###授权

[GPL v2](LICENSE)
