using System.Text.RegularExpressions;
using EliteAPI.Abstractions;
using EliteAPI.Abstractions.Events;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EliteVA;

public class Documentation
{
    private readonly ILogger<Documentation> _log;
    private readonly IEliteDangerousApi _api;

    public Documentation(ILogger<Documentation> log, IEliteDangerousApi api)
    {
        _log = log;
        _api = api;
    }

    public void Generate()
    {
        var journalsDirectory = new DirectoryInfo(_api.Config.JournalsPath);
        var journalFiles = journalsDirectory.GetFiles(_api.Config.JournalPattern);

        var values = journalFiles.SelectMany(GetPaths)
            .GroupBy(x => x.Path)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Value))
            .Select(x => new DocumentationEntry(x.Key, x.Value.Select(GetType), x.Value.Select(GetValue).OrderBy(y => y)))
            .OrderBy(x => x.Name)
            .ToArray();
        
        var file = Path.Combine(Plugin.Dir, "documentation.txt");
        
        File.WriteAllText(file, JsonConvert.SerializeObject(values, Formatting.Indented));
    }

    private IEnumerable<EventPath> GetPaths(FileInfo journalFile)
    {
        var jsons = File.ReadAllLines(journalFile.FullName);

        foreach (var json in jsons)
        {
            var paths = _api.EventParser.ToPaths(json).ToArray();
            var eventName = paths.First(x => x.Path.EndsWith(".Event", StringComparison.InvariantCultureIgnoreCase)).Value;

            foreach (var path in paths)
            {
                yield return new EventPath(Regex.Replace(path.Path, @"\[\d+?\]", "[n]"), path.Value);
            }
        }
    }

    readonly struct DocumentationEntry
    {
        public DocumentationEntry(string name, IEnumerable<string> types, IEnumerable<string> values)
        {
            Name = name;
            Types = types.Distinct().ToArray();
            Values = values.Distinct().ToArray();
        }

        public string Name { get; }
        public string[] Types { get; }
        public string[] Values { get; }
    }

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

    private string GetValue(string value) => JToken.Parse(value).ToString();
}