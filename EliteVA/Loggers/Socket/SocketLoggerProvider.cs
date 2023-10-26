using System.Net;
using Microsoft.Extensions.Logging;
using WatsonWebsocket;

namespace EliteVA.Loggers.Socket;

public class SocketLoggerProvider : ILoggerProvider
{
    private readonly WatsonWsServer _server;

    public SocketLoggerProvider()
    {
        _server = new WatsonWsServer(IPAddress.Loopback.ToString(), 51556);
        _server.StartAsync().GetAwaiter().GetResult();
    }
    
    public void Dispose()
    {
        
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new SocketLogger(_server);
    }
}