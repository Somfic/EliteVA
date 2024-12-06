using VoiceAttack.Proxy;
using VoiceAttack.Proxy.Abstractions;
using VoiceAttack.Proxy.Logging;

namespace VoiceAttack;

public class Plugin : VoiceAttackPlugin
{
    public override void OnInitialise(IVoiceAttackProxy proxy)
    {
        Log(VoiceAttackColor.Green, "Hello from inside the plugin!");
    }

    public override void OnInvoke(IVoiceAttackProxy proxy, string context)
    {
        
    }

    public override void OnCommandStopped(IVoiceAttackProxy proxy)
    {
        
    }

    public override void OnExit(IVoiceAttackProxy proxy)
    {
        
    }
}