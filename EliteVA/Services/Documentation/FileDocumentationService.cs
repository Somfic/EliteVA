using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using Microsoft.Extensions.Logging;

namespace EliteVA.Services.Documentation;

public class FileDocumentationService : VoiceAttackService
{
    private readonly ILogger<FileDocumentationService> _log;
    private readonly Timer _countdown;

    public FileDocumentationService(ILogger<FileDocumentationService> log)
    {
        _countdown = new Timer(VariablesHaveBeenSetHandler, null, Timeout.Infinite, Timeout.Infinite);
        _log = log;
    }
    
    public override Task OnStart(IVoiceAttackProxy proxy)
    {
        proxy.Variables.OnVariablesSet += (_, _) =>
        {
            _countdown.Change(500, Timeout.Infinite);
        };
        
        proxy.Commands.OnCommandInvoked += (_, _) => CommandsHaveBeenInvokedHandler();
        
        return Task.CompletedTask;
    }

    private void CommandsHaveBeenInvokedHandler()
    {
        _log.LogDebug("Writing commands to file");

        var commands = VoiceAttackPlugin.Proxy.Commands.InvokedCommands;

        if(!Directory.Exists(Path.Combine(VoiceAttackPlugin.Dir, "Commands")))
            Directory.CreateDirectory(Path.Combine(VoiceAttackPlugin.Dir, "Commands"));
        
        File.WriteAllLines(Path.Combine(VoiceAttackPlugin.Dir, "Commands", "Commands.txt"), commands.OrderByDescending(x => x.Timestamp).Select(x => $"{x.Timestamp.ToLongTimeString()}: {x.Command}"));
    }

    private void VariablesHaveBeenSetHandler(object state)
    {
        try
        {
            _log.LogDebug("Writing variables to file");

            var groups = VoiceAttackPlugin.Proxy.Variables.SetVariables.GroupBy(x => x.category);

            if (!Directory.Exists(Path.Combine(VoiceAttackPlugin.Dir, "Variables")))
                Directory.CreateDirectory(Path.Combine(VoiceAttackPlugin.Dir, "Variables"));

            foreach (var group in groups)
            {
                var source = new FileInfo(group.Key).Name.Split('.').First();

                if (source == "Journal")
                {
                    var variables = new List<string>();
                    
                    // Group by event
                   var events = group.GroupBy(x => x.name.Split('.')[1]).OrderByDescending(
                       x => x.First(x => x.name.Contains("timestamp")).value).ToList();
                   foreach (var eventVariables in events)
                   {
                       variables.Add($" ###  {eventVariables.Key}  ### ");
                       variables.AddRange(eventVariables
                           .Select(x => x with{ name = x.name.Split(':')[0].Length == 4 ? $" {x.name}" : x.name})
                           .Select(x => $"{x.name}: {x.value}")
                           .Reverse());
                       variables.Add("");
                   }
                   File.WriteAllLines(Path.Combine(VoiceAttackPlugin.Dir, "Variables", source) + ".txt", variables);
                }
                else
                {
                    var variables = group
                        .Select(x => x with{ name = x.name.Split(':')[0].Length == 4 ? $" {x.name}" : x.name})
                        .Select(x => $"{x.name}: {x.value}")
                        .ToList();
                    variables.Insert(0, $" ###  {group.First().category}  ### ");
                    File.WriteAllText(Path.Combine(VoiceAttackPlugin.Dir, "Variables", source) + ".txt", string.Join(Environment.NewLine, variables));
                }
            }
        } catch (Exception ex)
        {
            _log.LogError(ex, "Could not write variables to file");
        }
    }
}