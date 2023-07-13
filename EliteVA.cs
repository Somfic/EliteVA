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
    private readonly IConfiguration _config;
    public VoiceAttackProxy Proxy => VoiceAttack.Proxy;

    public Plugin(ILogger<Plugin> log, IEliteDangerousApi api, IConfiguration config)
    {
        _log = log;
        _api = api;
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
        await _api.InitialiseAsync();
        _api.Config.Apply();

        _api.Bindings.OnBindings((bindings, c) =>
        {
            try
            {
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

        var isCurated = _config.GetSection("EliteAPI").GetValue("Mode", "curated") == "curated";
        _log.LogCritical(isCurated ? "Running in curated event mode" : "Running in raw event mode");

        if (isCurated)
        {
            _api.Events.OnAny((e, c) =>
            {
                var paths = _api.EventParser.ToPaths(e).ToList();
                InvokePaths(paths, c);
                WriteVariables();
            });
        }
        else
        {
            _api.Events.OnAnyJson((e, c) =>
            {
                var paths = _api.EventParser.ToPaths(e).ToList();
                InvokePaths(paths, c);
                WriteVariables();
            });
        }

        _api.Events.On<StatusEvent>((e, c) =>
        {
            var paths = _api.EventParser.ToPaths(e).Select(x => new EventPath(x.Path.Replace("Status.", ""), x.Value))
                .ToArray();

            foreach (var path in paths)
            {
                InvokePaths(new[] { path }, c, path.Path);
            }

            WriteVariables();
        });

        _api.Events.Register<ShipEvent>();

        await _api.StartAsync();
    }
    
    private void InvokePaths(ICollection<EventPath> paths, EventContext c, string? eventName = null)
    {
        try
        {
            if (eventName == null)
            {
                eventName = paths.First(x => x.Path.EndsWith(".Event")).Value;

                if (eventName.EndsWith("Status"))
                    return;
            }

            foreach (var path in paths)
            {
                var value = path.Value;
                    
                if (string.IsNullOrWhiteSpace(value))
                    value = "\"\"";

                Proxy.Variables.Set(new FileInfo(c.SourceFile).Name, $"EliteAPI.{path.Path}".Replace("..", "."), JToken.Parse(value));
            }

            if (!c.IsRaisedDuringCatchup)
            {
                var command = $"((EliteAPI.{eventName}))";
                if (Proxy.Commands.Exists(command))
                    Proxy.Commands.Invoke(command);
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
}
readonly struct ShipEvent : IEvent
{
    public DateTime Timestamp { get; }
    public string Event { get; }
}