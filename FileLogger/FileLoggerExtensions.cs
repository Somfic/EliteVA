using EliteVA.FileLogger.Formats;
using EliteVA.FileLogger.Formats.Abstractions;
using Microsoft.Extensions.Logging;

namespace EliteVA.FileLogger;

public static class FileLoggerExtensions
{
    /// <summary>
    /// Adds a <see cref="FileLogger"/> instance to the <seealso cref="ILoggingBuilder"/>
    /// </summary>
    /// <param name="name">The name of this log file</param>
    /// <param name="directory">The directory of the log file</param>
    /// <param name="namingFormat">The naming format applied</param>
    /// <param name="format">The format applied</param>
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string name, string directory, IFileNamingFormat? namingFormat = null, IFileFormat? format = null)
    {
        if (namingFormat == null) { namingFormat = FileNamingFormats.Default; }
        if (format == null) { format = FileFormats.Default; }

        builder.AddProvider(new FileLoggerProvider(name, new DirectoryInfo(directory), namingFormat, format));

        return builder;
    }
}