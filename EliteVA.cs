using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EliteAPI.Abstractions;
using EliteVA.Proxy;
using EliteVA.Proxy.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EliteVA;

public class Plugin
{
    private string Dir => new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName ??
                          Directory.GetCurrentDirectory();
    
    private readonly IEliteDangerousApi _api;
    public VoiceAttackProxy Proxy => VoiceAttack.Proxy;

    public Plugin(IEliteDangerousApi api)
    {
        _api = api;
    }

    public async Task Initialize()
    {
        if(!File.Exists(Path.Combine(Dir, "config.yml")))
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"EliteVA.config.yml");
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            
            File.WriteAllText(Path.Combine(Dir, "config.yml"), content);
        }
        
        var config = File.ReadAllText(Path.Combine(Dir, "config.yml"))
            .Split('\n')
            .Where(x => x.Contains(":") && !x.Trim().StartsWith("#"))
            .Select(x => x.Split(':'))
            .ToDictionary(x => x[0].Trim(), x => x[1].Contains("#") ? x[1].Substring(x[1].IndexOf('#')).Trim() : x[1].Trim());

        if (config.ContainsKey("journalsPath"))
            _api.Config.JournalsPath = config["journalsPath"];
        
        if (config.ContainsKey("optionsPath"))
            _api.Config.OptionsPath = config["optionsPath"];
        
        if (config.ContainsKey("journalPattern"))
            _api.Config.JournalPattern = config["journalPattern"];
        
        _api.Config.Apply();

        _api.Bindings.OnBindings(bindings =>
        {
            try
            {
                if (!File.Exists(Path.Combine(Dir, "layout.yml")))
                {
                    using var stream = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream($"EliteVA.layout.yml");
                    using var reader = new StreamReader(stream);
                    var content = reader.ReadToEnd();

                    // Write the resource to disk
                    File.WriteAllText(Path.Combine(Dir, "layout.yml"), content);
                }

                var layout = File.ReadAllText(Path.Combine(Dir, "layout.yml"))
                    .Split('\n')
                    .Where(x => x.Contains(":") && !x.Trim().StartsWith("#"))
                    .Select(x => x.Split(':'))
                    .ToDictionary(x => x[0].Trim(), x => x[1].Contains("#") ? x[1].Substring(x[1].IndexOf('#')).Trim() : x[1].Trim());
                
                // Set keyboard keys
                foreach (var binding in bindings)
                {
                    string key;

                    if (binding.Primary is { Device: "Keyboard" })
                        key = binding.Primary.Value.Key;

                    else if (binding.Secondary is { Device: "Keyboard" })
                        key = binding.Secondary.Value.Key;

                    else
                        continue;

                    key = key.Replace("Key_", "");
                    var keycode = layout.FirstOrDefault(x => x.Key == key).Value ?? "NOT_SET";

                    if (keycode == "NOT_SET")
                    {
                        Proxy.Log.Write($"Key '{key}' is not set in the layout.yml file and cannot be added", VoiceAttackColor.Yellow);
                        continue;
                    }

                    Proxy.Variables.Set("Keybindings", $"EliteAPI.{binding.Name}", $"[{keycode}]", TypeCode.String);
                }
                
                var variables = Proxy.Variables.SetVariables.Where(x => x.category == "Keybindings").Select(x => $"{x.name}: {x.value}");
                File.WriteAllLines(Path.Combine(Dir, "keybindings variables.txt"), variables);
                
            } catch (Exception ex)
            {
                Proxy.Log.Write($"Error: {ex}", VoiceAttackColor.Red);
            }

        });
        
        _api.Events.OnAny(e =>
        {
            try
            { 
                var paths = _api.EventParser.ToPaths(e);
                
                foreach (var path in paths)
                {
                    var value = path.Value;
                    
                    if (string.IsNullOrWhiteSpace(value))
                        value = "\"\"";

                    Proxy.Variables.Set("Events", $"EliteAPI.{path.Path}", JToken.Parse(value));
                }

                var rawVariables = Proxy.Variables.SetVariables.Where(x => x.category == "Events").Select(x => $"{x.name}: {x.value}");
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

                // Get the path to the plugin's folder
                File.WriteAllLines(Path.Combine(Dir, "event variables.txt"), variables);
                
                var command = $"((EliteAPI.{e.Event}))";
                if (Proxy.Commands.Exists(command))
                    Proxy.Commands.Invoke(command);
            }
            catch (Exception ex)
            {
                Proxy.Log.Write($"Error while trying to process {e.Event} event: {ex}", VoiceAttackColor.Red);
            }
        });
        
        await _api.StartAsync();
    }
}

public static class Helper
{
    public static TypeCode FromJTokenType(this JTokenType jToken){
        switch (jToken)
        {   
            case JTokenType.Undefined:
            case JTokenType.Raw:
            case JTokenType.Null:
            case JTokenType.String:
            case JTokenType.None:
            case JTokenType.Object:
            case JTokenType.Array:
            case JTokenType.Constructor:
            case JTokenType.Property:
            case JTokenType.Comment:
            case JTokenType.Guid:
            case JTokenType.Bytes:
            case JTokenType.Uri:
                return TypeCode.String;

            case JTokenType.Integer:
                return TypeCode.Int64;

            case JTokenType.Float:
                return TypeCode.Decimal;

            case JTokenType.Boolean:
                return TypeCode.Boolean;

            case JTokenType.Date:
            case JTokenType.TimeSpan:
                return TypeCode.DateTime;

            default:
                throw new ArgumentOutOfRangeException(nameof(jToken), jToken, null);
        }
        
    }
}