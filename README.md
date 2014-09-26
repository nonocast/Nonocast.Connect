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
- WebSocket (RFC 6455) 
- 
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

```csharp
static void Main(string[] args) {
	var app = new Nonocast.Connect.WebApp();
	app.Get("/", (req, res) => { res.Html("<h1>hello world</h1>"); });

	var server = new Server(app).Listen(new int[] { 80, 8000, 7005 });

	Console.WriteLine("listening on port {0}", server.Port);
	Console.ReadLine();
}
```

###授权

[GPL v2](LICENSE)
