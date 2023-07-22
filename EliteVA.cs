using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EliteAPI.Abstractions;
using EliteAPI.Abstractions.Bindings.Models;
using EliteAPI.Abstractions.Events;
using EliteAPI.Abstractions.Status;
using EliteAPI.Bindings;
using EliteAPI.Events;
using EliteAPI.Events.Status.Ship;
using EliteAPI.Events.Status.Ship.Events;
using EliteVA.Proxy;
using EliteVA.Proxy.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EliteVA;

public class Plugin
{
    public static string Dir => new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName ??
                                 Directory.GetCurrentDirectory();

    private readonly ILogger<Plugin> _log;
    private readonly IEliteDangerousApi _api;
    private readonly Documentation _docs;
    private readonly IConfiguration _config;
    public VoiceAttackProxy Proxy => VoiceAttack.Proxy;

    public Plugin(ILogger<Plugin> log, IEliteDangerousApi api, Documentation docs, IConfiguration config)
    {
        _log = log;
        _api = api;
        _docs = docs;
        _config = config;
    }

    private IDictionary<string, string> ReadYml(string name)
    {
        if(!File.Exists(Path.Combine(Dir, $"{name}.yml")))
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"EliteVA.{name}.yml");
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            
            File.WriteAllText(Path.Combine(Dir, $"{name}.yml"), content);
        }
        
        return File.ReadAllText(Path.Combine(Dir, $"{name}.yml"))
            .Split('\n')
            .Select(x => Regex.Match(x, "^([^#]+?):([^#]+)"))
            .Where(x => x.Success)
            .ToDictionary(x => x.Groups[1].Value.Trim(), x => x.Groups[2].Value.Trim().Replace("\"", ""));
    }
    
    public async Task Initialize()
    {
        ClearVariables();
        
        await _api.InitialiseAsync();
        _api.Config.Apply();
        
        _log.LogInformation("Generating documentation");
        _docs.Generate();
        _log.LogInformation("Documentation generated");

        _api.Bindings.OnBindings((bindings, c) =>
        {
            try
            {
                var bindsName = new FileInfo(c.SourceFile).Name;
                if (!bindsName.EndsWith(".binds"))
                    bindsName = "standard";

                _log.LogInformation("Applying {BindingsFile} keybindings", bindsName);
                
                var layout = ReadYml("layout");
                
                // Set keyboard keys
                foreach (var b in bindings)
                {
                    if (b.Primary?.Device != "Keyboard" && b.Secondary?.Device != "Keyboard")
                        continue;

                    IPrimarySecondaryBinding binding = b.Primary?.Device == "Keyboard" ? b.Primary! : b.Secondary!;

                    var keycode = $"[{GetKeyCode(binding.Key, layout)}]";

                    foreach (var bindingModifier in binding.Modifiers.Reverse())
                    {
                        if (bindingModifier.Device != "Keyboard")
                        {
                            _log.LogWarning("Modifier for binding '{Binding}' is not a keyboard modifier and cannot be added", b.Name);
                            continue;
                        }
                        
                        keycode = $"[{GetKeyCode(bindingModifier.Key, layout)}]{keycode}";
                    }

                    Proxy.Variables.Set("Bindings", $"EliteAPI.{b.Name}", keycode, TypeCode.String);
                }
                
                WriteVariables();
                
            } catch (Exception ex)
            {
                _log.LogError(ex, "Failed to set bindings");
            }

        });

        _api.Events.On<FileheaderEvent>((e, c) =>
        {
            _log.LogInformation("Processing {JournalFile}", new FileInfo(c.SourceFile).Name);
        });
        
        var isCurated = _config.GetSection("EliteAPI").GetValue("Mode", "raw") == "curated";
        _log.LogCritical(isCurated ? "Running in curated event mode" : "Running in raw event mode");

        // _api.Events.On<StatusEvent>((e, c) =>
        // {
        //     var paths = _api.EventParser.ToPaths(e).Select(x => new EventPath(x.Path.Replace("Status.", ""), x.Value))
        //         .ToArray();
        //
        //     foreach (var path in paths)
        //     {
        //         InvokePaths(new[] { path }, c, path.Path);
        //     }
        //
        //     WriteVariables();
        // });
        
        if (isCurated)
        {
            _api.Events.OnAny((e, c) =>
            {
                var paths = _api.EventParser.ToPaths(e).ToList();
                InvokePaths(paths, c);
                WriteVariables();

                var eventName = e.Event;

                if (e is IStatusEvent)
                    eventName = $"Status.{eventName}";
                
                TriggerEvent(eventName, c);
            });
        }
        else
        {
            _api.Events.OnAnyJson((e, c) =>
            {
                var paths = _api.EventParser.ToPaths(e).ToList();
                InvokePaths(paths, c);
                WriteVariables();
                var eventName = paths.First(x => x.Path.EndsWith(".Event", StringComparison.InvariantCultureIgnoreCase)).Value;
                TriggerEvent(eventName, c);
            });
        }

        _api.Events.Register<ShipEvent>();

        await _api.StartAsync();
    }
    
    private void TriggerEvent(string eventName, EventContext c)
    {
        // Transform EliteAPI.FuelStatus into EliteAPI.Status.Fuel
        eventName = eventName.Replace("\"", "");
        eventName = Regex.Replace(eventName, @"([A-Za-z]+)Status", "Status.$1");

        if (c.IsRaisedDuringCatchup) 
            return;
        
        var command = $"((EliteAPI.{eventName}))";
        
        _log.LogDebug("Invoking {Command}", command);

        if (!Proxy.Commands.Exists(command)) 
            return;
        
        Proxy.Commands.Invoke(command);
        WriteCommands();
    }
    
    private void InvokePaths(ICollection<EventPath> paths, EventContext c, string? eventName = null)
    {
        try
        {
            eventName ??= paths.First(x => x.Path.EndsWith(".Event", StringComparison.InvariantCultureIgnoreCase)).Value;

            if (c.SourceFile.EndsWith("Status.json") && eventName.Contains("Status"))
            {
                paths = paths
                    .Where(x => 
                        !x.Path.EndsWith("timestamp", StringComparison.InvariantCultureIgnoreCase) &&
                        !x.Path.EndsWith("event", StringComparison.InvariantCultureIgnoreCase))
                    .Select(x =>
                        new EventPath(Regex.Replace(x.Path, "([a-zA-Z]+)Status\\.Value", "$1"),
                            x.Value))
                    .ToList();
            }

            // Clear arrays
            if (paths.Any(x => x.Path.Contains("[0]")))
            {
                var array = $"EliteAPI.{paths.First(x => x.Path.Contains("[0]")).Path.Split(new[] {"[0]"}, StringSplitOptions.None)[0]}";
                Proxy.Variables.ClearStartingWith(array);
            }


            foreach (var path in paths)
            {
                var value = path.Value;
                    
                if (string.IsNullOrWhiteSpace(value))
                    value = "\"\"";

                var name = $"EliteAPI.{path.Path}".Replace("..", ".");
                
                _log.LogDebug("Setting {Variable} to {Value}", name, value);
                
                Proxy.Variables.Set(new FileInfo(c.SourceFile).Name, name, value, JToken.Parse(value).Type);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, ex.StackTrace);
        }
    }
    
    public string GetKeyCode(string key, IDictionary<string, string> layout)
    {
        key = key.Replace("Key_", "");
        var keycode = layout.FirstOrDefault(x => x.Key == key).Value ?? $"NOT_SET({key})";

        if (keycode == "NOT_SET")
            _log.LogWarning("Key '{Key}' is not set in the layout.yml file and cannot be added", key);

        return keycode;
    }

    public void ClearVariables()
    {
        if (!Directory.Exists(Path.Combine(Dir, "Variables")))
            return;
        
        foreach (var file in Directory.GetFiles(Path.Combine(Dir, "Variables")))
            File.Delete(file);
    }

    public void WriteVariables()
    {
        var groups = Proxy.Variables.SetVariables.GroupBy(x => x.category);

        if(!Directory.Exists(Path.Combine(Dir, "Variables")))
            Directory.CreateDirectory(Path.Combine(Dir, "Variables"));
        
        foreach (var group in groups)
        {
            var source = new FileInfo(group.Key).Name.Split('.').First();
            
            if (source == "Journal")
            {
                var rawVariables = Proxy.Variables.SetVariables.Where(x => x.category.StartsWith("Journal")).Select(x => $"{x.name}: {x.value}");
                var lastName = "";
                var variables = new List<string>();
                foreach (var variable in rawVariables)
                {
                    var name = variable.Split('.')[1];
                    if (lastName != name)
                    {
                        lastName = name;
                        variables.Add("");
                        variables.Add($" ###  {lastName}  ### ");
                    }
                    
                    variables.Add(variable);
                }

                File.WriteAllLines(Path.Combine(Dir, "Variables", source) + ".txt", variables);
            }
            else
            {
                var variables = group.Select(x => $"{x.name}: {x.value}").ToList();
                variables.Insert(0, $" ###  {group.First().category}  ### ");
                File.WriteAllLines(Path.Combine(Dir, "Variables",  source) + ".txt", variables);
            }
        }
    }
    
    public void WriteCommands()
    {
        var commands = Proxy.Commands.InvokedCommands.Select(x => $"{x.timestamp.ToLongTimeString()}: {x.command}").ToList();
        commands.Reverse();
        commands.Insert(0, " ###  Commands  ### ");
        File.WriteAllLines(Path.Combine(Dir, "Variables", "Commands.txt"), commands);
    }
}
readonly struct ShipEvent : IEvent
{
    public DateTime Timestamp { get; }
    public string Event { get; }
}