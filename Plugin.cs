using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using EliteVA.Proxy.Logging;
using VoiceAttack.Services;

namespace VoiceAttack;

public class Plugin : VoiceAttackPlugin
{
    private readonly VoiceAttackService[] _services = [new EliteApiService()];
    
    public override async Task OnStart(IVoiceAttackProxy proxy)
    {
        var version = GetType().Assembly.GetName().Version;
        proxy.Log.Write($"Starting EliteVA v{version.Major}.{version.Minor}.{version.Build}", VoiceAttackColor.Orange);

        foreach (var service in _services)
        {
            try
            {
                await service.OnStart(proxy);
            } 
            catch (Exception e) 
            {
                proxy.Log.Write($"Error during startup in {service.GetType().Name}: {e.Message}", VoiceAttackColor.Red);
            }
        }
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
                proxy.Log.Write($"Error during invocation in {service.GetType().Name}: {e.Message}", VoiceAttackColor.Red);
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
                proxy.Log.Write($"Error during stopped command in {service.GetType().Name}: {e.Message}", VoiceAttackColor.Red);
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
                proxy.Log.Write($"Error during exiting in {service.GetType().Name}: {e.Message}", VoiceAttackColor.Red);
            }
        }
    }
}