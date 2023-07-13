using EliteVA.FileLogger.Formats.Abstractions;
using EliteVA.FileLogger.Formats.Default;

namespace EliteVA.FileLogger.Formats;

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