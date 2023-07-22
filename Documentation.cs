using System.Net;
using System.Net.Sockets;
using System.Text;
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
    private TcpListener _listener;

    public Documentation(ILogger<Documentation> log, IEliteDangerousApi api)
    {
        _log = log;
        _api = api;
        _listener = new TcpListener(IPAddress.Loopback, 51555);
    }
    
    public void StartServer() {
        _listener.Start();
        _log.LogDebug($"Starting socket server on {_listener.LocalEndpoint}");

        while (true)
        { 
            _log.LogDebug("Waiting for a new connection");
            var client = _listener.AcceptTcpClient();
            Task.Run(() => HandleClient(client));
        }
        
       
    }

    private async Task HandleClient(TcpClient client)
    {
        _log.LogDebug("Client connected, waiting on handshake");
        var stream = client.GetStream();

        try
        {

            // enter to an infinite cycle to be able to handle every change in stream
            while (client.Connected)
            {
                while (!stream.DataAvailable) ; // hang here until we receive some data
                while (client.Available < 3) ; // match against "get"

                var bytes = new byte[client.Available];
                stream.Read(bytes, 0, bytes.Length);
                var s = Encoding.UTF8.GetString(bytes);

                if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase))
                {
                    _log.LogDebug("Starting handshake");

                    // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
                    // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
                    // 3. Compute SHA-1 and Base64 hash of the new value
                    // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
                    var swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                    var swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                    var swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                    var swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                    // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                    var response = Encoding.UTF8.GetBytes(
                        "HTTP/1.1 101 Switching Protocols\r\n" +
                        "Connection: Upgrade\r\n" +
                        "Upgrade: websocket\r\n" +
                        "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

                    stream.Write(response, 0, response.Length);

                    _log.LogDebug("Handshake complete");

                    var jsons = Generate();
                    await SendWebSocketMessage(stream, "RESET");
                    foreach (var json in jsons)
                    {
                        try
                        {
                            await SendWebSocketMessage(stream, json);
                        }
                        catch (Exception ex)
                        {
                            _log.LogDebug(ex, "Could not send message");
                        }
                    }
                }
            }
        } catch (Exception ex)
        {
            _log.LogDebug(ex, "Could not handle client");
        }

        _log.LogDebug("Client disconnected");
        
        stream.Close();
        client.Close();
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

    public string[] Generate()
    {
        var journalsDirectory = new DirectoryInfo(_api.Config.JournalsPath);
        var journalFiles = journalsDirectory.GetFiles(_api.Config.JournalPattern);

        var values = journalFiles.SelectMany(GetPaths)
            .GroupBy(x => x.Path)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Value))
            .Select(x => new DocumentationEntry(x.Key, x.Value.Select(GetType), x.Value.Select(GetValue).OrderBy(y => y)))
            .OrderBy(x => x.Name)
            .GroupBy(x => x.Name.Split('.')[0])
            .ToDictionary(x => x.Key, x => x.Select(y => y))
            .ToArray();


        return values.Select(x => JsonConvert.SerializeObject(x)).ToArray();
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