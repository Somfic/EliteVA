using EliteAPI.Abstractions;
using EliteAPI.Web.Spansh;
using EliteVA.Loggers.File;
using EliteVA.Loggers.VoiceAttack;
using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using EliteVA.Services.Bridge;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace EliteVA;

public class VoiceAttack : VoiceAttackPlugin
{
    private IHost _host;
    private ILogger<VoiceAttack> _log;
    private ICollection<VoiceAttackService> _services = new List<VoiceAttackService>();

    public override async Task OnStart(IVoiceAttackProxy proxy)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(s =>
            {
                s.AddSingleton<JournalEventsService>();
                s.AddSingleton<BindingsService>();

                s.AddEliteApi();
                s.AddHttpClient();
                s.AddWebApi<SpanshApi>();
                s.RemoveAll<IHttpMessageHandlerBuilderFilter>();
            })
            .ConfigureLogging((c, l) =>
            {
                l.SetMinimumLevel(LogLevel.Trace);
                l.AddFilter("Microsoft", LogLevel.Warning);
                l.AddFilter("System", LogLevel.Warning);

                l.AddVoiceAttack(proxy);

                //if (c.Configuration.GetSection("EliteAPI").GetValue("FileLogging", true))
                l.AddFile("EliteVA", Path.Combine(Dir, "Logs"));
            })
            .ConfigureAppConfiguration(config => { config.AddIniFile(Path.Combine(Dir, "EliteVA.ini"), true); })
            .Build();

        _log = _host.Services.GetRequiredService<ILogger<VoiceAttack>>();

        _log.LogInformation("Initialising EliteVA v{Version}", GetType().Assembly.GetName().Version);

        _services = new List<VoiceAttackService>()
        {
            _host.Services.GetRequiredService<JournalEventsService>(),
            _host.Services.GetRequiredService<BindingsService>()
        };
        
        var api = _host.Services.GetRequiredService<IEliteDangerousApi>();

        foreach (var service in _services)
        {
            try
            {
                _log.LogDebug("Starting {Name}", service.GetType().Name);
                await service.OnStart(proxy);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to start for {Name}", service.GetType().Name);
            }
        }

        await api.StartAsync();
    }

    public override async Task OnInvoke(IVoiceAttackProxy proxy, string context)
    {
        foreach (var service in _services)
        {
            try
            {
                await service.OnInvoke(proxy, context);
            }
            catch (Exception e)
            {
                _log.LogWarning(e, "Failed to invoke context '{Context}' for {Name}", context, service.GetType().Name);
            }
        }
    }

    public override async Task OnCommandStopped(IVoiceAttackProxy proxy)
    {
        foreach (var service in _services)
        {

            try
            {
                await service.OnCommandStopped(proxy);
            }
            catch (Exception e)
            {
                _log.LogWarning(e, "Failed to stop on command for {Name}", service.GetType().Name);
            }
        }
    }

    public override async Task OnStop(IVoiceAttackProxy proxy)
    {
        foreach (var service in _services)
        {

            try
            {
                await service.OnStop(proxy);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to stop for {Name}", service.GetType().Name);
            }
        }

        _host.Dispose();
    }
}