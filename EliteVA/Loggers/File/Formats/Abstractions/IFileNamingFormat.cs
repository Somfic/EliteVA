namespace EliteVA.Loggers.File.Formats.Abstractions;

/// <summary>
/// A format used for naming the file
/// </summary>
public interface IFileNamingFormat
{
    string NameFile(DirectoryInfo directory, string name);
}