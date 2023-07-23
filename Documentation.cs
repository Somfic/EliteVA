using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using EliteAPI.Abstractions;
using EliteAPI.Abstractions.Events;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WatsonWebsocket;

namespace EliteVA;

public class Documentation
{
    private readonly ILogger<Documentation> _log;
    private readonly IEliteDangerousApi _api;
    private readonly WatsonWsServer _server;

    public Documentation(ILogger<Documentation> log, IEliteDangerousApi api)
    {
        _log = log;
        _api = api;
        _server = new WatsonWsServer(IPAddress.Loopback.ToString(), 51555);
    }
    
    public void StartServer() {
        _server.Start();
        
        _log.LogDebug($"Starting websocket server");
        
        _server.Logger += (message) => _log.LogTrace(message);

        _server.ClientConnected += (_, client) =>
        {
            _log.LogDebug("Client connected");

            var json = JsonConvert.SerializeObject(Generate());
            _server.SendAsync(client.Client.Guid, json);
        };
    }

    public void WriteToFiles()
    {
        var variables = Generate();
        
        var path = Path.Combine(Plugin.Dir, "Variables", "Journal Records");
        Directory.CreateDirectory(path);
        
        // For each key, write to a file
        foreach (var variable in variables)
        {
            var key = variable.Key;
            var values = variable.Value;

            var content = new StringBuilder(" ### ((EliteAPI.");
            content.Append(key);
            content.Append(")) ###");
            content.AppendLine();
            content.AppendLine();

            foreach (var value in values)
            {
                foreach (var type in value.Types)
                {
                    content.Append("{");
                    content.Append(type);
                    content.Append(":");
                    content.Append("EliteAPI.");
                    content.Append(value.Name);
                    content.AppendLine("}");
                }
            }
            
            File.WriteAllText(Path.Combine(path, key + ".txt"), content.ToString());
        }
    }

    private KeyValuePair<string, IEnumerable<DocumentationEntry>>[] Generate()
    {
        var journalsDirectory = new DirectoryInfo(_api.Config.JournalsPath);
        var journalFiles = journalsDirectory.GetFiles(_api.Config.JournalPattern);

       return journalFiles.SelectMany(GetPaths)
            .GroupBy(x => x.Path)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Value))
            .Select(x => new DocumentationEntry(x.Key, x.Value.Select(GetType),
                x.Value.Select(GetValue).OrderBy(_ => Guid.NewGuid())))
            .OrderBy(x => x.Name)
            .GroupBy(x => x.Name.Split('.')[0])
            .ToDictionary(x => x.Key, x => x.Select(y => y))
            .ToArray();
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

    public readonly struct DocumentationEntry
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