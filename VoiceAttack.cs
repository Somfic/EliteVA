using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using EliteAPI.Web.Spansh;
using EliteVA.FileLogger;
using EliteVA.Proxy;
using EliteVA.Proxy.Logging;
using EliteVA.Proxy.Logging.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EliteVA;

public class VoiceAttack
{
    private static Plugin _plugin;
    public static VoiceAttackProxy Proxy { get; private set; }
    
    private static IHost Host { get; set; }

    public static Guid VA_Id() => new("189a4e44-caf1-459b-b62e-fabc60a12986");

    public static string VA_DisplayName() => "EliteVA";

    public static string VA_DisplayInfo() => "EliteVA by Somfic";

    public static void VA_Init1(dynamic vaProxy)
    {
        Proxy = new VoiceAttackProxy(vaProxy);
        var version = typeof(Plugin).Assembly.GetName().Version;
        Proxy.Log.Write($"Starting EliteVA v{version}", VoiceAttackColor.Green);

        try
        {
            Initialize(vaProxy);
        }
        catch (Exception ex)
        {
            Proxy.Log.Write("Could not start EliteVA", VoiceAttackColor.Red);
            Proxy.Log.Write(ex.Message, VoiceAttackColor.Red);
            Proxy.Log.Write(ex.StackTrace, VoiceAttackColor.Red);
            
            File.WriteAllText(Path.Combine(Plugin.Dir, "Logs", "EliteVA.startup.log"), JsonConvert.SerializeObject(ex, Formatting.Indented));
            throw;
        }
    }

    public static void VA_Exit1(dynamic vaProxy) { }

    public static void VA_StopCommand() { }

    public static void VA_Invoke1(dynamic vaProxy)
    {
        Proxy = new VoiceAttackProxy(vaProxy);

        try
        {
            _plugin.Invoke(Proxy.Context).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Proxy.Log.Write("Error while invoking EliteVA", VoiceAttackColor.Red);
            Proxy.Log.Write(ex.Message, VoiceAttackColor.Red);
            Proxy.Log.Write(ex.StackTrace, VoiceAttackColor.Red);
        }
    }

    private static void Initialize(dynamic vaProxy)
    {
        var loggingPath = Path.Combine(Plugin.Dir, "Logs");
        if (!Directory.Exists(loggingPath))
            Directory.CreateDirectory(loggingPath);
            
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(s =>
            {
                s.AddSingleton<Plugin>();
                s.AddSingleton<Documentation>();
                s.AddEliteApi();
                s.AddHttpClient();
                s.AddWebApi<SpanshApi>();
                s.RemoveAll<IHttpMessageHandlerBuilderFilter>();
            })
            .ConfigureLogging((c, l) =>
            {
                l.SetMinimumLevel(LogLevel.Trace);
                l.AddProvider(new VoiceAttackLoggerProvider(vaProxy));
                l.AddFile("EliteVA", loggingPath, c.Configuration.GetSection("EliteAPI").GetValue("FileLogging", true));
                l.AddFilter("Microsoft", LogLevel.Warning);
                l.AddFilter("System", LogLevel.Warning);
            }).ConfigureAppConfiguration(config =>
            {
                config.AddIniFile(Path.Combine(Plugin.Dir, "EliteVA.ini"), false);
            })
            .Build();
        
        _plugin = Host.Services.GetRequiredService<Plugin>();
        _plugin.Initialize().GetAwaiter().GetResult();

        var documentation = Host.Services.GetRequiredService<Documentation>();
        Task.Run(() => documentation.StartServer());
    }
}