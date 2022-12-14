using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using EliteVA.Proxy;
using EliteVA.Proxy.Logging;
using EliteVA.Proxy.Logging.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EliteVA;

public class VoiceAttack
{
    public static VoiceAttackProxy Proxy { get; private set; }
    
    private static IHost Host { get; set; }

    public static Guid VA_Id() => new("189a4e44-caf1-459b-b62e-fabc60a12986");

    public static string VA_DisplayName() => "EliteVA";

    public static string VA_DisplayInfo() => "EliteVA by Somfic";

    public static void VA_Init1(dynamic vaProxy)
    {
        Proxy = new VoiceAttackProxy(vaProxy);
            
        try
        {
            Initialize(vaProxy);
        }
        catch (Exception ex)
        {
            Proxy.Log.Write("Could not initialize EliteVA: " + ex, VoiceAttackColor.Red);
            Proxy.Log.Write((ex.InnerException ?? ex).ToString(), VoiceAttackColor.Yellow);
        }
    }

    public static void VA_Exit1(dynamic vaProxy) { }

    public static void VA_StopCommand() { }

    public static void VA_Invoke1(dynamic vaProxy)
    {
        Proxy = new VoiceAttackProxy(vaProxy);
    }

    private static void Initialize(dynamic vaProxy)
    {
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(s =>
            {
                s.AddSingleton<Plugin>();
                s.AddEliteApi();
            })
            .ConfigureLogging(l =>
            {
                l.SetMinimumLevel(LogLevel.Information);
                l.AddProvider(new VoiceAttackLoggerProvider(vaProxy));
            })
            .Build();
        
        var eliteva = Host.Services.GetRequiredService<Plugin>();
        eliteva.Initialize().GetAwaiter().GetResult();
    }
}