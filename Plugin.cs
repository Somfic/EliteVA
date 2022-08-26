using System;
using System.Net.WebSockets;
using EliteVA.Proxy;
using EliteVA.Proxy.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EliteVA;

public class Plugin
{
    private static IHost Host { get; set; }

    public static Guid VA_Id() => new("189a4e44-caf1-459b-b62e-fabc60a12986");

    public static string VA_DisplayName() => "EliteVA";

    public static string VA_DisplayInfo() => "EliteVA by Somfic";

    public static void VA_Init1(dynamic vaProxy)
    {
        var proxy = new VoiceAttackProxy(vaProxy);
            
        try
        {
            Initialize(vaProxy);
        }
        catch (Exception ex)
        {
            proxy.Log.Write("Could not initialize EliteVA: " + ex.ToString(), VoiceAttackColor.Red);
            proxy.Log.Write(ex.InnerException.ToString(), VoiceAttackColor.Yellow);
        }
    }

    public static void VA_Exit1(dynamic vaProxy) { }

    public static void VA_StopCommand() { }

    public static void VA_Invoke1(dynamic vaProxy)
    {
        Host.Services.GetRequiredService<ProxyHolder>().Set(new VoiceAttackProxy(vaProxy));
    }

    private static void Initialize(dynamic vaProxy)
    {
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(s =>
            {
                s.AddSingleton<EliteVA>();
                s.AddSingleton<ClientWebSocket>();
                s.AddSingleton<ProxyHolder>();
            })
            .ConfigureLogging(l =>
            {
                l.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        Host.Services.GetRequiredService<ProxyHolder>().Set(new VoiceAttackProxy(vaProxy));
        var eliteva = Host.Services.GetRequiredService<EliteVA>();
        eliteva.Run().GetAwaiter().GetResult();
    }
}