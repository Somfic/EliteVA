using System.Net;
using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using EliteVA.Records;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WatsonWebsocket;

namespace EliteVA.Services.Documentation;

public class SocketDocumentationService : VoiceAttackService
{
    private readonly ILogger<SocketDocumentationService> _log;
    private readonly RecordGenerator _record;
    private readonly WatsonWsServer _server;
    private readonly Timer _countdown;
    private CommandDocumentation[] _commands;
    private VariableDocumentation[] _variables;

    public SocketDocumentationService(ILogger<SocketDocumentationService> log, RecordGenerator record)
    {
        _log = log;
        _record = record;
        _server = new WatsonWsServer(IPAddress.Loopback.ToString(), 51555);
        _countdown = new Timer(VariablesHaveBeenSetHandler, null, Timeout.Infinite, Timeout.Infinite);
    }

    public override async Task OnStart(IVoiceAttackProxy proxy)
    {
        _log.LogDebug("Starting socket server");
        _server.Logger += message => _log.LogTrace(message);
        
        _server.ClientConnected += async (_, c) =>
        {
            _log.LogDebug("Client connected");

            await SendToClient(c.Client, "commands", JsonConvert.SerializeObject(_commands));
            await SendToClient(c.Client, "variables", JsonConvert.SerializeObject(_variables));
            await SendToClient(c.Client, "records", JsonConvert.SerializeObject(_record.Records));
        };
        
        proxy.Commands.OnCommandInvoked += (_, _) => CommandsHaveBeenInvokedHandler();
        proxy.Variables.OnVariablesSet += (_, _) => _countdown.Change(500, Timeout.Infinite);
        _record.RecordsGenerated += (_, records) => SendRecordsToClients(records).GetAwaiter().GetResult();

        await _server.StartAsync();
    }

    private void CommandsHaveBeenInvokedHandler()
    {
        _commands = VoiceAttackPlugin.Proxy.Commands.InvokedCommands
            .Select(x => new CommandDocumentation(x.Timestamp, x.Command))
            .OrderByDescending(x => x.Timestamp)
            .ToArray();
        
        SendCommandsToClients(_commands).GetAwaiter().GetResult();
    }

    private void VariablesHaveBeenSetHandler(object state)
    {
        try
        {
            _variables = VoiceAttackPlugin.Proxy.Variables.SetVariables
                .Select(x => new VariableDocumentation(x.category, x.name, GetType($"\"{x.value}\""), x.value))
                .ToArray();

            SendVariablesToClients(_variables).GetAwaiter().GetResult();
        } catch (Exception e)
        {
            _log.LogError(e, "Failed to send variables to clients");
        }
    }
    
    private string GetType(string value)
    {
        try
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
        } catch (Exception e)
        {
            _log.LogError(e, "Failed to get type for value '{Value}'", value);
            return "???";
        }
    }
    
    private async Task SendCommandsToClients(ICollection<CommandDocumentation> commands)
    {
         await SendToClients("commands", JsonConvert.SerializeObject(commands));
    }
    
    private async Task SendVariablesToClients(ICollection<VariableDocumentation> variables)
    {
         await SendToClients("variables", JsonConvert.SerializeObject(variables));
    }

    private async Task SendRecordsToClients(KeyValuePair<string, IEnumerable<RecordDocumentation>>[] records)
    {
        await SendToClients("records", JsonConvert.SerializeObject(records));
    }

    private async Task SendToClients(string type, string json)
    {
        foreach (var client in _server.ListClients())
        {
            await SendToClient(client, type, json);
        }
    }

    private async Task SendToClient(ClientMetadata client, string type, string json)
    {
        await _server.SendAsync(client.Guid, type.ToUpper());
        await _server.SendAsync(client.Guid, json);
    }
}