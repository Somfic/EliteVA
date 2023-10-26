using System.Text.RegularExpressions;
using EliteAPI.Abstractions;
using EliteAPI.Abstractions.Events;
using EliteAPI.Events;
using EliteVA.Services.Documentation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace EliteVA.Records;

public class RecordGenerator
{
    private readonly ILogger<RecordGenerator> _log;
    private readonly IEliteDangerousApi _api;
    private readonly IConfiguration _config;
    private KeyValuePair<string, IEnumerable<RecordDocumentation>>[]? _journalRecords;
    public IReadOnlyCollection<KeyValuePair<string, IEnumerable<RecordDocumentation>>> Records => _journalRecords ?? Array.Empty<KeyValuePair<string, IEnumerable<RecordDocumentation>>>();
    
    public RecordGenerator(ILogger<RecordGenerator> log, IEliteDangerousApi api, IConfiguration config)
    {
        _log = log;
        _api = api;
        _config = config;
    }
    
    public event EventHandler<KeyValuePair<string, IEnumerable<RecordDocumentation>>[]> RecordsGenerated;
    
    public KeyValuePair<string, IEnumerable<RecordDocumentation>>[] GenerateJournalRecords()
    {
        try
        {
            if (_journalRecords != null)
                return _journalRecords;

            _journalRecords = Array.Empty<KeyValuePair<string, IEnumerable<RecordDocumentation>>>();

            var journalsDirectory = new DirectoryInfo(_api.Config.JournalsPath);
            var journalFiles = journalsDirectory.GetFiles(_api.Config.JournalPattern);

            var latestJournalFile = journalFiles.OrderByDescending(x => x.LastWriteTime).First();
            var targetVersion = GetGameVersionFromFile(latestJournalFile);

            var amountOfJournalsToScrape = _config.GetSection("EliteAPI").GetValue("AmountJournalsToScrape", 50);
            var filteredFiles = journalFiles
                .Where(x => GetGameVersionFromFile(x) == targetVersion).OrderByDescending(x => x.LastWriteTime)
                .Take(amountOfJournalsToScrape)
                .ToArray();

            _log.LogDebug("Generating journal records for version {TargetVersion} by scraping {FilteredFilesLength} journal files", targetVersion, filteredFiles.Length);

            var lastCache = DateTime.MinValue;
            
            var generatedPaths = new List<EventPath>();

            foreach (var filteredFile in filteredFiles)
            {
                generatedPaths.AddRange(GetPaths(filteredFile));

                if (DateTime.Now - lastCache <= TimeSpan.FromSeconds(15)) 
                    continue;
                
                _journalRecords = generatedPaths
                    .GroupBy(x => x.Path)
                    .ToDictionary(x => x.Key, x => x.Select(y => y.Value))
                    .Select(x => new RecordDocumentation(x.Key, x.Value.Select(GetType),
                        x.Value.Select(GetValue).OrderBy(_ => Guid.NewGuid())))
                    .OrderBy(x => x.Name)
                    .GroupBy(x => x.Name.Split('.')[0])
                    .ToDictionary(x => x.Key, x => x.Select(y => y))
                    .ToArray();

                RecordsGenerated?.Invoke(this, _journalRecords);
                lastCache = DateTime.Now;
            }
            
            _journalRecords = generatedPaths
                .GroupBy(x => x.Path)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Value))
                .Select(x => new RecordDocumentation(x.Key, x.Value.Select(GetType),
                    x.Value.Select(GetValue).OrderBy(_ => Guid.NewGuid())))
                .OrderBy(x => x.Name)
                .GroupBy(x => x.Name.Split('.')[0])
                .ToDictionary(x => x.Key, x => x.Select(y => y).Reverse())
                .ToArray();
            RecordsGenerated?.Invoke(this, _journalRecords);

            return _journalRecords;
        } catch (Exception ex)
        {
            _log.LogWarning(ex, "Could not generate journal records");
            return Array.Empty<KeyValuePair<string, IEnumerable<RecordDocumentation>>>();
        }
    }
    
    private IReadOnlyCollection<EventPath> GetPaths(FileInfo journalFile)
    {
        using var stream = journalFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        
        var jsons = reader.ReadToEnd().Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

        List<EventPath> allPaths = new();
        
        foreach (var json in jsons)
        {
            try
            {
                var paths = _api.EventParser.ToPaths(json).ToArray();
                allPaths.AddRange(paths.Select(path => new EventPath(Regex.Replace(path.Path, @"\[\d+?\]", "[n]"), path.Value)));
            } catch (Exception ex)
            {
                ex.Data.Add("json", json);
                ex.Data.Add("file", journalFile.Name);
                _log.LogDebug(ex, "Could not parse journal entry");
            }
        }

        return allPaths;
    }
    
    private string GetGameVersionFromFile(FileInfo file)
    {
        try
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);

            var json = reader.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .First();

            var fileHeader = _api.EventParser.FromJson<FileheaderEvent>(json);

            var version = fileHeader.Gameversion.Split('.');

            return string.Join(".", version.Take(3));
        } catch (Exception ex)
        {
            ex.Data.Add("file", file.Name);
            _log.LogDebug(ex, "Could not get game version for file");
            return "0.0.0";
        }
    }
    
    private string GetValue(string value) => JToken.Parse(value).ToString();
    
    private string GetType(string value)
    {
        return JToken.Parse(value).Type switch
        {
            JTokenType.Boolean => "BOOL",
            JTokenType.Date => "DATE",
            JTokenType.TimeSpan => "DATE",
            JTokenType.Float => "DEC",
            JTokenType.String => "TXT",
            JTokenType.Integer => int.TryParse(value, out _) ? "INT" : "DEC",
            _ => "???"
        };
    }
}