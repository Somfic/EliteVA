using EliteVA.FileLogger.Formats.Abstractions;
using EliteVA.FileLogger.Formats.Default;

namespace EliteVA.FileLogger.Formats;

/// <summary>
/// Default file formats
/// </summary>
public static class FileFormats
{
    /// <summary>
    /// The default file format
    /// </summary>
    public static IFileFormat Default => new DefaultFileFormat();
}