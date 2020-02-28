# What's Twino?

**Twino** is an extensible .NET Core TCP Server provides multiple protocols on same host.<br>
**Twino** is a complete Messaging Queue server library.<br>
**Twino** is a WebSocket server with advanced client management.<br>
**Twino** is an HTTP server with MVC Architecture.<br>
**Twino** is an IOC Library with service pools and scopes.<br>
**Twino** is a WebSocket Client.<br>

## Why Twino?

In a single application with single framework, you can have TCP Server, HTTP Server, WebSocket Server, Messaging Queue Server and with many extra features such as IOC, Authentication, Clients, Connectors etc.

- High performance (in many cases, nearly as fast as kestrel)
- Twino MQ is not a executable messaging queue server. It's a framework and you can create your own MQ server with a few interface implementations.
- MQ Protocol supports various message types. It's not just a queue server, it supports peer to peer messaging, request responsing and more.
- Twino has high scalable advanced websocket server with amazing client management.
- Multiple protocols can be used on same project, same port, same host.
- Twino.Mvc has nearly all features ASP.NET MVC has, and you write nearly same code.
- Twino has sweet client connectors for WebSocket and TMQ protocols.

**Read [All Features](https://github.com/mhelvacikoylu/twino/blob/v2/docs/Features.MD) to see all features of twino libraries.**

See **[All Twino NuGet Packages](https://github.com/mhelvacikoylu/twino/blob/v2/docs/Packages.MD)**

**Go to [Documentation Home Page](https://github.com/mhelvacikoylu/twino/tree/v2/docs)**

### Basic WebSocket Server Example

Basic WebSocket Server creation example

    class Program
    {
        static void Main(string[] args)
        {
            TwinoServer server = new TwinoServer();
            server.UseWebSockets((socket, message) => { Console.WriteLine($"Received: {message}"); });
	    
	    //or advanced with IProtocolConnectionHandler<WebSocketMessage> implementation
            //server.UseWebSockets(new ServerWsHandler());
            server.Start(80);
            
            //optional
            _server.Server.BlockWhileRunning();
        }
    }

### Basic MQ Server Example

Basic MQ Server creation example

    class Program
    {
        static void Main(string[] args)
        {
            MqServerOptions options = new MqServerOptions();
            //set your options here
            
            MqServer mq = new MqServer(options);
            mq.SetDefaultDeliveryHandler(new YourCustomDeliveryHandler());
            TwinoServer twinoServer = new TwinoServer(ServerOptions.CreateDefault());
            twinoServer.UseMqServer(mq);
            twinoServer.Start();
            
            //optional
            _server.Server.BlockWhileRunning();
        }
    }


### Basic MVC Example

Twino.Mvc similar to ASP.NET Core. Here is a basic example:

    class Program
    {
        static void Main(string[] args)
        {
            TwinoMvc mvc = new TwinoMvc();
            mvc.Init();
            mvc.Use(app =>
            {
                app.UseMiddleware<CorsMiddleware>();
            });

            TwinoServer server = new TwinoServer();
            server.UseMvc(mvc, HttpOptions.CreateDefault());
            server.Start();
            
            //optional
            server.BlockWhileRunning();
        }
    }

    [Route("[controller]")]
    public class DemoController : TwinoController
    {
        [HttpGet("get/{?id}")]
        public async Task<IActionResult> Get([FromRoute] int? id)
        {
            return await StringAsync("Hello world: " + id);
        }

        [HttpPost("get2")]
        public async Task<IActionResult> Get2([FromBody] CustomModel model)
        {
            return await JsonAsync(new {Message = "Hello World Json"});
        }
    }
