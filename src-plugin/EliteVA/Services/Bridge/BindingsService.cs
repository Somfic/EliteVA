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
    private IReadOnlyCollection<Binding> _oldBindings = new List<Binding>();

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
        foreach (var oldBinding in _oldBindings)
        {
            _log.LogDebug("Clearing {Variable} from VoiceAttack", $"EliteAPI.{oldBinding.Name}");
            VoiceAttackPlugin.Proxy.Variables.Clear("Bindings", $"EliteAPI.{oldBinding.Name}", TypeCode.String);
        }
        
        _oldBindings = bindings;

        var bindsName = new FileInfo(context.SourceFile).Name;
        if (!bindsName.EndsWith(".binds"))
            bindsName = "standard";

        _log.LogInformation("Applying {BindingsFile} keybindings", bindsName);
                
        var layout = ReadYml("layout");
                
        // var nonKeyboardBinidings = bindings.Where(x => x.Primary?.Device != "Keyboard" && x.Secondary?.Device != "Keyboard").ToList();
        
        // Set keyboard keys
        foreach (var b in bindings.Where(x => x.Primary?.Device == "Keyboard" || x.Secondary?.Device == "Keyboard"))
        {
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

            _log.LogDebug("Setting {Variable} to {Value}", $"EliteAPI.{b.Name}", keycode);
            VoiceAttackPlugin.Proxy.Variables.Set("Bindings", $"EliteAPI.{b.Name}", keycode, TypeCode.String);
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