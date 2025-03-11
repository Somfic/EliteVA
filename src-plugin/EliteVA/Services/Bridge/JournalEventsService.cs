using System.Text.RegularExpressions;
using EliteAPI.Abstractions;
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
    private readonly IEventParser _eventParser;
    private readonly IEvents _events;

    public JournalEventsService(ILogger<JournalEventsService> log, IEventParser eventParser, IEvents events)
    {
        _log = log;
        _eventParser = eventParser;
        _events = events;
    }
    
    public override Task OnStart(IVoiceAttackProxy proxy)
    {
        _events.OnAnyJson(HandleIncomingJournalEvent);
        _events.On<FileheaderEvent>((e, c) => _log.LogInformation("Processing {Journal}", c.SourceFile.Split('\\').Last()));
        return Task.CompletedTask;
    }

    private void HandleIncomingJournalEvent(string json, EventContext context)
    {
        var paths = _eventParser.ToPaths(json).ToArray();
        var eventName = paths.First(x => x.Path.EndsWith(".Event", StringComparison.InvariantCultureIgnoreCase)).Value.Replace("\"", "");
        
        // If this is a status event, remove the status suffix from the paths
        if (eventName == "Status")
            return;

        if (eventName == "NavRoute" && context.SourceFile.Contains("Journal"))
            return;

        _log.LogDebug("Processing {Event}", eventName);
        
        if (context.SourceFile.EndsWith("Status.json") && eventName.Contains("Status"))
            paths = paths
                .Where(x => 
                    !x.Path.EndsWith("timestamp", StringComparison.InvariantCultureIgnoreCase) 
                    && !x.Path.EndsWith("event", StringComparison.InvariantCultureIgnoreCase))
                .Select(x =>
                    new EventPath(Regex.Replace(x.Path, "([a-zA-Z]+)Status\\.Value", "$1"), x.Value))
                .ToArray();
        
        _log.LogDebug("Clearing variables starting with {Variable}", $"EliteAPI.{eventName}");
        VoiceAttackPlugin.Proxy.Variables.ClearStartingWith(context.SourceFile.Split('\\').Last(), $"EliteAPI.{eventName}");
        
        foreach (var path in paths)
        {
            var value = path.Value;
            
            if (string.IsNullOrWhiteSpace(value))
                value = "\"\"";
            
            var name = $"EliteAPI.{path.Path}".Replace("..", ".");
            
            _log.LogDebug("Setting {Variable} to {Value}", name, value);
            VoiceAttackPlugin.Proxy.Variables.Set(context.SourceFile.Split('\\').Last(), name, value, JToken.Parse(value).Type);
        }

        if (context.IsRaisedDuringCatchup)
            return;
        
        var command = $"((EliteAPI.{eventName}))";
        
        if (eventName.EndsWith("Status") && eventName != "Status")
            command = $"((EliteAPI.Status.{eventName.Replace("Status", "")}))";
        
        if (!VoiceAttackPlugin.Proxy.Commands.Exists(command))
            return;
        
        _log.LogDebug("Invoking {Command}", command);
        VoiceAttackPlugin.Proxy.Commands.Invoke(command);
    }
}

