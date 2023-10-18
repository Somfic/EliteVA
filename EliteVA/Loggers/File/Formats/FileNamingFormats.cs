using EliteVA.Loggers.File.Formats.Abstractions;
using EliteVA.Loggers.File.Formats.Default;

namespace EliteVA.Loggers.File.Formats;

/// <summary>
/// Default file naming formats
/// </summary>
public static class FileNamingFormats
{
    /// <summary>
    /// The default file naming format
    /// </summary>
    public static IFileNamingFormat Default => new DefaultFileNamingFormat();
}