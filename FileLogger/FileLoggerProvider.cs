using EliteVA.FileLogger.Formats.Abstractions;
using Microsoft.Extensions.Logging;

namespace EliteVA.FileLogger;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _name;
    private readonly DirectoryInfo _directory;
    private readonly IFileNamingFormat _namingFormat;
    private readonly IFileFormat _format;

    /// <summary>
    /// Creates a new instance of the <see cref="FileLoggerProvider"/> class
    /// </summary>
    public FileLoggerProvider(string name, DirectoryInfo directory, IFileNamingFormat namingFormat, IFileFormat format)
    {
        _name = name;
        _directory = directory;
        _namingFormat = namingFormat;
        _format = format;
    }

    /// <summary>
    /// Disposes of the logger
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Creates a new ConsoleLogger
    /// </summary>
    /// <param name="categoryName">The category of these logs</param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _name, _directory, _format, _namingFormat);
    }
}