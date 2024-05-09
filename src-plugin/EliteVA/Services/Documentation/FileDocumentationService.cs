using System.Text;
using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using EliteVA.Records;
using Microsoft.Extensions.Logging;

namespace EliteVA.Services.Documentation;

public class FileDocumentationService : VoiceAttackService
{
    private readonly ILogger<FileDocumentationService> _log;
    private readonly Timer _countdown;

    public FileDocumentationService(ILogger<FileDocumentationService> log, RecordGenerator record)
    {
        _countdown = new Timer(VariablesHaveBeenSetHandler, null, Timeout.Infinite, Timeout.Infinite);
        record.RecordsGenerated += (_, records) => WriteRecordsToFile(records);
        _log = log;
    }

    private void WriteRecordsToFile(KeyValuePair<string, IEnumerable<RecordDocumentation>>[] records)
    {
        var path = Path.Combine(VoiceAttackPlugin.Dir, "Variables", "Journal Records");
        Directory.CreateDirectory(path);
        
        // For each key, write to a file
        foreach (var record in records)
        {
            var key = record.Key;
            var values = record.Value;

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
        var commands = VoiceAttackPlugin.Proxy.Commands.InvokedCommands;

        if(!Directory.Exists(Path.Combine(VoiceAttackPlugin.Dir, "Commands")))
            Directory.CreateDirectory(Path.Combine(VoiceAttackPlugin.Dir, "Commands"));
        
        File.WriteAllLines(Path.Combine(VoiceAttackPlugin.Dir, "Commands", "Commands.txt"), commands.OrderByDescending(x => x.Timestamp).Select(x => $"{x.Timestamp.ToLongTimeString()}: {x.Command}"));
    }

    private void VariablesHaveBeenSetHandler(object state)
    {
        try
        {
            var groups = VoiceAttackPlugin.Proxy.Variables.SetVariables.GroupBy(x => x.category);

            if (!Directory.Exists(Path.Combine(VoiceAttackPlugin.Dir, "Variables")))
                Directory.CreateDirectory(Path.Combine(VoiceAttackPlugin.Dir, "Variables"));

            foreach (var group in groups)
            {
                var source = new FileInfo(group.Key).Name.Split('.').First();

                if (source == "Journal")
                {
                    var variables = new List<string>();

                    try
                    {
                        // Group by event
                        var events = group
                            .GroupBy(x => x.name.Split('.')[1])
                            .OrderByDescending(x => x.FirstOrDefault(x => x.name.Contains("timestamp")).value ?? "")
                            .ToList();
                        
                        foreach (var eventVariables in events)
                        {
                            variables.Add($" ###  {eventVariables.Key}  ### ");
                            variables.AddRange(eventVariables
                                .Select(x => x with { name = x.name.Split(':')[0].Length == 4 ? $" {x.name}" : x.name })
                                .Select(x => $"{x.name}: {x.value}")
                                .Reverse());
                            variables.Add("");
                        }

                        File.WriteAllLines(Path.Combine(VoiceAttackPlugin.Dir, "Variables", source) + ".txt",
                            variables);
                    }
                    catch (Exception ex)
                    {
                        _log.LogDebug(ex, "Could not write variables to file");
                    }
                }
                else
                {
                    var variables = group
                        .Select(x => x with{ name = x.name.Split(':')[0].Length == 4 ? $" {x.name}" : x.name})
                        .Select(x => $"{x.name}: {x.value}")
                        .Reverse()
                        .ToList();
                    variables.Insert(0, $" ###  {group.First().category}  ### ");
                    File.WriteAllText(Path.Combine(VoiceAttackPlugin.Dir, "Variables", source) + ".txt", string.Join(Environment.NewLine, variables));
                }
            }
        } catch (Exception ex)
        {
            _log.LogDebug(ex, "Could not write variables to file");
        }
    }
}