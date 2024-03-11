using Microsoft.Extensions.Logging;

namespace EliteVA.Loggers.Socket;

public static class SocketLoggerExtensions
{
    public static ILoggingBuilder AddSocket(this ILoggingBuilder builder)
    {
        builder.AddProvider(new SocketLoggerProvider());
        return builder;
    }
}