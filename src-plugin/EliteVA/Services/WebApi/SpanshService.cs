using EliteAPI.Abstractions;
using EliteAPI.Abstractions.Events;
using EliteAPI.Abstractions.Events.Converters;
using EliteAPI.Web.Spansh;
using EliteAPI.Web.Spansh.RoutePlanner.Requests;
using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Somfic.Common;

namespace EliteVA.Services.WebApi;

public class SpanshService : VoiceAttackService
{
    private readonly ILogger<SpanshService> _log;
    private readonly SpanshApi _spansh;
    private readonly IEliteDangerousApi _api;

    public SpanshService(ILogger<SpanshService> log, SpanshApi spansh, IEliteDangerousApi api)
    {
        _log = log;
        _spansh = spansh;
        _api = api;
    }

    public override Task OnStart(IVoiceAttackProxy proxy)
    {
        return Task.CompletedTask;
    }

    public override async Task OnInvoke(IVoiceAttackProxy proxy, string context)
    {
        context = context.Trim().ToUpper();

        switch (context)
        {
            case "SPANSH.NEUTRON-PLOTTER":
                await NeutronPlotter();
                break;
        }
    }

    private async Task NeutronPlotter()
    {
        _log.LogDebug("Invoking Spansh.NeutronPlotter");
        
        var from = VoiceAttackPlugin.Proxy.Variables.Get<string>("EliteAPI.Spansh.NeutronPlotter.From", "Fusang");
        var to = VoiceAttackPlugin.Proxy.Variables.Get<string>("EliteAPI.Spansh.NeutronPlotter.To", "Sol");
        var range = VoiceAttackPlugin.Proxy.Variables.Get("EliteAPI.Spansh.NeutronPlotter.Range", 15);
        var result = await _spansh.Routes.Neutron(new NeutronRequest(from, to, range) { Via = from });
        
        result.On(
            value: x => HandleResponse("Spansh.NeutronPlotter", x),
            error: x => _log.LogWarning(x, "Could not get neutron route"));
    }
    
    private void HandleResponse<T>(string command, T response)
    {
        var json = JsonConvert.SerializeObject(response, new JsonSerializerSettings {ContractResolver = new EventContractResolver()});

        var jobject = JObject.Parse(json);
        jobject.Add("event", command);
        json = jobject.ToString();
        
        var paths = _api.EventParser.ToPaths(json);
        paths = paths.Select(x => new EventPath(x.Path, x.Value)).ToList();
        SetPaths(paths, command);
        
        command = $"((EliteAPI.{command}))";
        if (VoiceAttackPlugin.Proxy.Commands.Exists(command))
            VoiceAttackPlugin.Proxy.Commands.Invoke(command);   
    }
    
    private void SetPaths(IEnumerable<EventPath> paths, string category)
    {
        // Clear arrays
        if (paths.Any(x => x.Path.Contains("[0]")))
        {
            var array = $"EliteAPI.{paths.First(x => x.Path.Contains("[0]")).Path.Split(new[] {"[0]"}, StringSplitOptions.None)[0]}";
            _log.LogDebug("Clearing variables starting with {Variable}", array);
            VoiceAttackPlugin.Proxy.Variables.ClearStartingWith(category, array);
        }

        foreach (var path in paths)
        {
            try
            {
                var value = path.Value;

                if (string.IsNullOrWhiteSpace(value))
                    value = "\"\"";

                var name = $"EliteAPI.{path.Path}".Replace("..", ".");

                _log.LogDebug("Setting {Variable} to {Value}", name, value);

                VoiceAttackPlugin.Proxy.Variables.Set(category, name, value, JToken.Parse(value).Type);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Could not set variable {Variable} to {Value}", path.Path, path.Value);
            }
        }
    }

}