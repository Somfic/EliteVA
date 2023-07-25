using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using EliteAPI.Abstractions;
using EliteAPI.Abstractions.Events;
using EliteAPI.Events;
using EliteVA.Proxy.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WatsonWebsocket;

namespace EliteVA;

public class Documentation
{
    private readonly ILogger<Documentation> _log;
    private readonly IEliteDangerousApi _api;
    private readonly IConfiguration _config;
    private readonly WatsonWsServer _server;
    private KeyValuePair<string, IEnumerable<DocumentationEntry>>[]? _journalRecords;

    public Documentation(ILogger<Documentation> log, IEliteDangerousApi api, IConfiguration config)
    {
        _log = log;
        _api = api;
        _config = config;
        _server = new WatsonWsServer(IPAddress.Loopback.ToString(), 51555);
    }
    
    public void StartServer() {
        _server.Start();
        
        _log.LogDebug($"Starting websocket server");
        
        _server.Logger += (message) => _log.LogTrace(message);

        _server.ClientConnected += async (_, client) =>
        {
            _log.LogDebug("Client connected");

            SendCommands(VoiceAttack.Proxy.Commands.InvokedCommands);
            SendVariables(VoiceAttack.Proxy.Variables.SetVariables);
            
            var records = JsonConvert.SerializeObject(GenerateJournalRecords());
            await _server.SendAsync(client.Client.Guid, "RECORDS");
            await _server.SendAsync(client.Client.Guid, records);
        };

        RecordsGenerated += async (_, _) =>
        {
            WriteRecordsToFile();
            
            var records = JsonConvert.SerializeObject(GenerateJournalRecords());
            var clients = _server.ListClients();
            foreach (var client in clients)
            {
                await _server.SendAsync(client.Guid, "RECORDS");
                await _server.SendAsync(client.Guid, records);
            }
        };
    }
    
    public event EventHandler? RecordsGenerated;

    private void WriteRecordsToFile()
    {
        var variables = GenerateJournalRecords();
        
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
    
    public KeyValuePair<string, IEnumerable<DocumentationEntry>>[] GenerateJournalRecords()
    {
        try
        {
            if (_journalRecords != null)
                return _journalRecords;

            _journalRecords = Array.Empty<KeyValuePair<string, IEnumerable<DocumentationEntry>>>();

            var journalsDirectory = new DirectoryInfo(_api.Config.JournalsPath);
            var journalFiles = journalsDirectory.GetFiles(_api.Config.JournalPattern);

            var latestJournalFile = journalFiles.OrderByDescending(x => x.LastWriteTime).First();
            var targetVersion = GetGameVersionFromFile(latestJournalFile);

            var amountOfJournalsToScrape = _config.GetSection("EliteAPI").GetValue("AmountJournalsToScrape", 1);
            var filteredFiles = journalFiles
                .Where(x => GetGameVersionFromFile(x) == targetVersion).OrderByDescending(x => x.LastWriteTime)
                .Take(amountOfJournalsToScrape)
                .ToArray();

            _log.LogDebug(
                $"Generating journal records for version {targetVersion} by scraping {filteredFiles.Length} journal files");

            var lastCache = DateTime.MinValue;
            
            var generatedPaths = new List<EventPath>();
            
            var filesProcessed = 0;
            foreach (var filteredFile in filteredFiles)
            {
                generatedPaths.AddRange(GetPaths(filteredFile));
                filesProcessed++;

                if (DateTime.Now - lastCache <= TimeSpan.FromSeconds(15)) 
                    continue;
                
                var percentage = (int) Math.Round((double) filesProcessed / filteredFiles.Length * 100);
                _log.LogDebug($"Generated {generatedPaths.Count} journal records ({percentage}%)");
                
                _journalRecords = generatedPaths
                    .GroupBy(x => x.Path)
                    .ToDictionary(x => x.Key, x => x.Select(y => y.Value))
                    .Select(x => new DocumentationEntry(x.Key, x.Value.Select(GetType),
                        x.Value.Select(GetValue).OrderBy(_ => Guid.NewGuid())))
                    .OrderBy(x => x.Name)
                    .GroupBy(x => x.Name.Split('.')[0])
                    .ToDictionary(x => x.Key, x => x.Select(y => y))
                    .ToArray();

                RecordsGenerated?.Invoke(this, EventArgs.Empty);
                lastCache = DateTime.Now;
            }
            
            _journalRecords = generatedPaths
                .GroupBy(x => x.Path)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Value))
                .Select(x => new DocumentationEntry(x.Key, x.Value.Select(GetType),
                    x.Value.Select(GetValue).OrderBy(_ => Guid.NewGuid())))
                .OrderBy(x => x.Name)
                .GroupBy(x => x.Name.Split('.')[0])
                .ToDictionary(x => x.Key, x => x.Select(y => y))
                .ToArray();
            RecordsGenerated?.Invoke(this, EventArgs.Empty);
            
            _log.LogDebug($"Finished generating records. Generated {_journalRecords.Length} events from {generatedPaths.Count} journal events");

            return _journalRecords;
        } catch (Exception ex)
        {
            _log.LogDebug(ex, "Could not generate journal records");
            return Array.Empty<KeyValuePair<string, IEnumerable<DocumentationEntry>>>();
        }
    }

    private string GetGameVersionFromFile(FileInfo file)
    {
        try
        {
            // Open file with read-only access
            using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);

            // Read the first line
            var json = reader.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .First();

            // Parse the fileheader event
            var fileheader = _api.EventParser.FromJson<FileheaderEvent>(json);

            // Get the game version
            var version = fileheader.Gameversion.Split('.');

            // Return the version
            return string.Join(".", version.Take(3));
        } catch (Exception ex)
        {
            ex.Data.Add("file", file.Name);
            _log.LogDebug(ex, "Could not get game version for file");
            return "0.0.0";
        }
    }
    private IReadOnlyCollection<EventPath> GetPaths(FileInfo journalFile)
    {
        // Open file with read-only access
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

    public void SendVariables(IReadOnlyList<(string category, string name, string value)> variables)
    {
        var groupedData = variables.GroupBy(item => item.category);

        // Step 2: Create a new dictionary to hold the result
        var resultDictionary = new Dictionary<string, IDictionary<string, string>>();

        // Step 3: Iterate through the grouped data and create nested dictionaries for each category
        foreach (var group in groupedData)
        {
            var category = group.Key;
            var nestedDictionary = new Dictionary<string, string>();

            // Step 4: Fill the nested dictionary with name-value pairs
            foreach (var item in group)
            {
                nestedDictionary[item.name] = item.value;
            }

            // Step 5: Add the nested dictionary to the result dictionary
            resultDictionary[category] = nestedDictionary;
        }

        var clients = _server.ListClients();

        foreach (var client in clients)
        {
            _server.SendAsync(client.Guid, "VARIABLES");
            _server.SendAsync(client.Guid, JsonConvert.SerializeObject(resultDictionary));
        }
    }

    public void SendCommands(IReadOnlyCollection<VoiceAttackCommands.SetCommand> commands)
    {
        var clients = _server.ListClients();

        foreach (var client in clients)
        {
            _server.SendAsync(client.Guid, "COMMANDS");
            _server.SendAsync(client.Guid, JsonConvert.SerializeObject(commands));
        }
    }

  
}