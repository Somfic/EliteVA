using Microsoft.Extensions.Logging;
using WatsonWebsocket;

namespace EliteVA.Loggers.Socket;

public static class SocketLoggerExtensions
{
    public static ILoggingBuilder AddSocket(this ILoggingBuilder builder)
    {
        builder.AddProvider(new SocketLoggerProvider());
        return builder;
    }
}