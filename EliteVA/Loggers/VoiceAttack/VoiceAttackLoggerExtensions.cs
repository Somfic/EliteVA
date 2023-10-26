using EliteVA.Loggers.File;
using EliteVA.Proxy.Abstractions;
using Microsoft.Extensions.Logging;

namespace EliteVA.Loggers.VoiceAttack;

public static class VoiceAttackLoggerExtensions
{
    /// <summary>
    /// Adds a <see cref="FileLogger"/> instance to the <seealso cref="ILoggingBuilder"/>
    /// </summary>
    /// <param name="name">The name of this log file</param>
    /// <param name="directory">The directory of the log file</param>
    /// <param name="namingFormat">The naming format applied</param>
    /// <param name="format">The format applied</param>
    public static ILoggingBuilder AddVoiceAttack(this ILoggingBuilder builder, IVoiceAttackProxy proxy)
    {
        builder.AddProvider(new VoiceAttackLoggerProvider(proxy));

        return builder;
    }
}