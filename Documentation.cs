using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using EliteAPI.Abstractions;
using EliteAPI.Abstractions.Events;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperSimpleTcp;
using WatsonWebsocket;

namespace EliteVA;

public class Documentation
{
    private readonly ILogger<Documentation> _log;
    private readonly IEliteDangerousApi _api;
    private WatsonWsServer _server;

    public Documentation(ILogger<Documentation> log, IEliteDangerousApi api)
    {
        _log = log;
        _api = api;
        _server = new WatsonWsServer(IPAddress.Loopback.ToString(), 51555);
    }
    
    public void StartServer() {
        _server.Start();
        
        _log.LogDebug($"Starting websocket server");
        
        _server.Logger += (message) => _log.LogTrace($"WS: {message}");

        _server.ClientConnected += (sender, client) =>
        {
            _log.LogDebug("Client connected");

            var json = Generate();
            _server.SendAsync(client.Client.Guid, json);
        };
    }
    
    private async Task SendWebSocketMessage(NetworkStream stream, string message)
    {
        var data = Encoding.UTF8.GetBytes(message);
        int headerSize;

        if (data.Length <= 125)
        {
            headerSize = 2;
        }
        else if (data.Length <= 65535)
        {
            headerSize = 4;
        }
        else
        {
            headerSize = 10;
        }

        byte[] header;
        if (headerSize == 2)
        {
            header = new byte[2];
            header[0] = 0x81; // FIN = 1, Text frame
            header[1] = (byte)data.Length;
        }
        else if (headerSize == 4)
        {
            header = new byte[4];
            header[0] = 0x81; // FIN = 1, Text frame
            header[1] = 126;
            header[2] = (byte)((data.Length >> 8) & 255);
            header[3] = (byte)(data.Length & 255);
        }
        else
        {
            header = new byte[10];
            header[0] = 0x81; // FIN = 1, Text frame
            header[1] = 127;
            Array.Copy(BitConverter.GetBytes((ulong)data.Length), 0, header, 2, 8);
        }

        await stream.WriteAsync(header, 0, header.Length); // Write the WebSocket frame header
        await stream.WriteAsync(data, 0, data.Length);     // Write the payload
        await stream.FlushAsync();
    }

    public string Generate()
    {
        var journalsDirectory = new DirectoryInfo(_api.Config.JournalsPath);
        var journalFiles = journalsDirectory.GetFiles(_api.Config.JournalPattern);

        var values = journalFiles.SelectMany(GetPaths)
            .GroupBy(x => x.Path)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Value))
            .Select(x => new DocumentationEntry(x.Key, x.Value.Select(GetType), x.Value.Select(GetValue).OrderBy(x => Guid.NewGuid())))
            .OrderBy(x => x.Name)
            .GroupBy(x => x.Name.Split('.')[0])
            .ToDictionary(x => x.Key, x => x.Select(y => y))
            .ToArray();


        return JsonConvert.SerializeObject(values);
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