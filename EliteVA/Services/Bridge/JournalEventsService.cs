using System.Text.RegularExpressions;
using EliteAPI.Abstractions;
using EliteAPI.Abstractions.Bindings;
using EliteAPI.Abstractions.Bindings.Models;
using EliteAPI.Abstractions.Events;
using EliteAPI.Events;
using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace EliteVA.Services.Bridge;

public class JournalEventsService : VoiceAttackService
{
    private readonly ILogger<JournalEventsService> _log;
    private readonly IEliteDangerousApi _api;

    public JournalEventsService(ILogger<JournalEventsService> log, IEliteDangerousApi api)
    {
        _log = log;
        _api = api;
    }
    
    public override Task OnStart(IVoiceAttackProxy proxy)
    {
        _api.Events.OnAnyJson(HandleIncomingJournalEvent);
        return Task.CompletedTask;
    }
    private void HandleIncomingJournalEvent(string json, EventContext context)
    {
        var paths = _api.EventParser.ToPaths(json).ToArray();
        var eventName = paths.First(x => x.Path.EndsWith(".Event", StringComparison.InvariantCultureIgnoreCase)).Value;
        
        _log.LogDebug("Processing {Event}", eventName);
        
        // If this is a status event, remove the status suffix from the paths
        if (context.SourceFile.EndsWith("Status.json") && eventName.Contains("Status"))
            paths = paths
                .Where(x => 
                    !x.Path.EndsWith("timestamp", StringComparison.InvariantCultureIgnoreCase) 
                    && !x.Path.EndsWith("event", StringComparison.InvariantCultureIgnoreCase))
                .Select(x =>
                    new EventPath(Regex.Replace(x.Path, "([a-zA-Z]+)Status\\.Value", "$1"), x.Value))
                .ToArray();
        
        // Todo: clear arrays
        
        foreach (var path in paths)
        {
            var value = path.Value;
            
            if (string.IsNullOrWhiteSpace(value))
                value = "\"\"";
            
            var name = $"EliteAPI.{path.Path}".Replace("..", ".");
            
            _log.LogDebug("Setting {Variable} to {Value}", name, value);
            VoiceAttackPlugin.Proxy.Variables.Set(context.SourceFile, name, value, JToken.Parse(value).Type);
        }
    }
}

