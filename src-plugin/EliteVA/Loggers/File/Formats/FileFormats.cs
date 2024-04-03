using EliteVA.Loggers.File.Formats.Abstractions;
using EliteVA.Loggers.File.Formats.Default;

namespace EliteVA.Loggers.File.Formats;

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