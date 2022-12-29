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
    private readonly IEliteDangerousApi _api;
    public VoiceAttackProxy Proxy => VoiceAttack.Proxy;

    public Plugin(IEliteDangerousApi api)
    {
        _api = api;
    }

    public async Task Initialize()
    {
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

                var rawVariables = Proxy.Variables.SetVariables.Select(x => $"{x.name}: {x.value}");
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
                File.WriteAllLines(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName ?? Directory.GetCurrentDirectory(), "variables.txt"), variables);
                
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