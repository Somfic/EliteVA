using System.Text;
using Microsoft.Extensions.Logging;

namespace EliteVA.FileLogger.Formats.Abstractions;

/// <summary>
/// A format used for formatting the file
/// </summary>
public interface IFileFormat
{
    StringBuilder CreateLogEntry(LogLevel logLevel, string category, EventId eventId, string message, Exception ex);
}