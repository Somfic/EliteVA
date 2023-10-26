using EliteVA.Loggers.File.Formats.Abstractions;
using Microsoft.Extensions.Logging;

namespace EliteVA.Loggers.File;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly IFileNamingFormat _namingFormat;
    private readonly IFileFormat _format;
    private readonly string _path;

    /// <summary>
    /// Creates a new instance of the <see cref="FileLoggerProvider"/> class
    /// </summary>
    public FileLoggerProvider(string name, DirectoryInfo directory, IFileNamingFormat namingFormat, IFileFormat format)
    {
        Directory.CreateDirectory(directory.FullName);
        
        _namingFormat = namingFormat;
        _format = format;

        _path = Path.Combine(directory.FullName, namingFormat.NameFile(directory, name));
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
        return new FileLogger(categoryName, _path, _format, _namingFormat);
    }
}