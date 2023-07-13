using System;
using Microsoft.Extensions.Logging;

namespace EliteVA.Proxy.Logging.Extensions;

internal class VoiceAttackLogger : ILogger
{
    private readonly VoiceAttackProxy _proxy;

    public VoiceAttackLogger(VoiceAttackProxy proxy, string categoryName)
    {
        _proxy = proxy; 
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {
        if(logLevel <= LogLevel.Information)
            return;
        
        VoiceAttackColor color;
        
        switch (logLevel)
        {
            case LogLevel.Critical:
                color = VoiceAttackColor.Purple;
                break;
            
            case LogLevel.Error:
                color = VoiceAttackColor.Red;
                break;
            
            case LogLevel.Warning:
                color = VoiceAttackColor.Yellow;
                break;

            case LogLevel.None:
            case LogLevel.Information:
                color = VoiceAttackColor.Blue;
                break;
            
            case LogLevel.Debug:
                color = VoiceAttackColor.Gray;
                break;
            
            case LogLevel.Trace:
                color = VoiceAttackColor.Black;
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
        
        var exceptionMessage = exception != null ? $"\n{exception?.Message}" : string.Empty;
        
        _proxy.Log.Write($"EliteVA: {formatter(state, exception)}{exceptionMessage}", color);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}