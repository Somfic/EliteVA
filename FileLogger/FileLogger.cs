using EliteVA.FileLogger.Formats.Abstractions;
using Microsoft.Extensions.Logging;

namespace EliteVA.FileLogger;

/// <summary>   
/// A colorful console implementation for <seealso cref="ILogger"/>
/// </summary>
public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly IFileFormat _format;
    private readonly string _path;

    internal FileLogger(string categoryName, string name, DirectoryInfo directory, IFileFormat format, IFileNamingFormat namingFormat)
    {
        _categoryName = categoryName;
        _format = format;
        _path = Path.Combine(directory.FullName, namingFormat.NameFile(directory, name));
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        var entryBuilder = _format.CreateLogEntry(logLevel, _categoryName, eventId, formatter(state, exception), exception);
        var entry = entryBuilder.ToString();
        entry = entry.Trim('\r', '\n');
        entry += Environment.NewLine;

        System.IO.File.AppendAllText(_path, entry);
    }
}