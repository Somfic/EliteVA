using System.Reflection;
using System.Text.RegularExpressions;
using EliteAPI.Abstractions;
using EliteAPI.Abstractions.Bindings;
using EliteAPI.Abstractions.Bindings.Models;
using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using Microsoft.Extensions.Logging;

namespace EliteVA.Services.Bridge;

public class BindingsService : VoiceAttackService
{
    private readonly ILogger<BindingsService> _log;
    private readonly IEliteDangerousApi _api;

    public BindingsService(ILogger<BindingsService> log, IEliteDangerousApi api)
    {
        _log = log;
        _api = api;
    }
    
    public override Task OnStart(IVoiceAttackProxy proxy)
    {
        _api.Bindings.OnBindings(HandleBindings);
        return Task.CompletedTask;
    }

    private void HandleBindings(IReadOnlyCollection<Binding> bindings, BindingsContext context)
    {
        var bindsName = new FileInfo(context.SourceFile).Name;
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
    }
    
    private static IDictionary<string, string> ReadYml(string name)
    {
        if(!File.Exists(Path.Combine(VoiceAttackPlugin.Dir, $"{name}.yml")))
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"EliteVA.{name}.yml");
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            
            File.WriteAllText(Path.Combine(VoiceAttackPlugin.Dir, $"{name}.yml"), content);
        }
        
        return File.ReadAllText(Path.Combine(VoiceAttackPlugin.Dir, $"{name}.yml"))
            .Split('\n')
            .Select(x => Regex.Match(x, "^([^#]+?):([^#]+)"))
            .Where(x => x.Success)
            .ToDictionary(x => x.Groups[1].Value.Trim(), x => x.Groups[2].Value.Trim().Replace("\"", ""));
    }
}