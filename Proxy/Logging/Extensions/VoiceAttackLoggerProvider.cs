using Microsoft.Extensions.Logging;

namespace EliteVA.Proxy.Logging.Extensions;

internal class VoiceAttackLoggerProvider : ILoggerProvider
{
    private VoiceAttackProxy _proxy;
    
    public VoiceAttackLoggerProvider(object vaProxy)
    {
        _proxy = new VoiceAttackProxy(vaProxy);
    }

    public void Dispose()
    {
        
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new VoiceAttackLogger(_proxy, categoryName);
    }
}