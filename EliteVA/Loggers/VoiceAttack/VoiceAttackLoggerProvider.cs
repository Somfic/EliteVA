using EliteVA.Proxy.Abstractions;
using Microsoft.Extensions.Logging;

namespace EliteVA.Loggers.VoiceAttack;

internal class VoiceAttackLoggerProvider : ILoggerProvider
{
    private readonly IVoiceAttackProxy _proxy;
    
    public VoiceAttackLoggerProvider(IVoiceAttackProxy proxy)
    {
        _proxy = proxy;
    }

    public void Dispose()
    {
        
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new VoiceAttackLogger(_proxy, categoryName);
    }
}