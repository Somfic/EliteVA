using EliteVA.Proxy.Logging;

namespace EliteVA.Proxy;

public class VoiceAttackPluginWrapper
{
    public static Guid VA_Id() => PluginConfiguration.Id;

    public static string VA_DisplayName() => PluginConfiguration.Name;

    public static string VA_DisplayInfo() => PluginConfiguration.Description;

    public static void VA_Init1(dynamic vaProxy)
    {
        // Find type that implements VoiceAttackPlugin
        var pluginType = typeof(VoiceAttackPluginWrapper).Assembly
            .GetTypes()
            .FirstOrDefault(t => t.IsSubclassOf(typeof(VoiceAttackPlugin)));
        
        if(pluginType == null)
            throw new InvalidOperationException("No class found that inherits VoiceAttackPlugin.");
        
        VoiceAttackPlugin.Instance = (VoiceAttackPlugin) Activator.CreateInstance(pluginType);

        if(VoiceAttackPlugin.Instance == null) 
            throw new InvalidOperationException("No VoiceAttackPlugin instance found.");
        
        VoiceAttackPlugin.Proxy = new VoiceAttackProxy(vaProxy);

        try
        {
            VoiceAttackPlugin.Instance.OnStart(VoiceAttackPlugin.Proxy).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            VoiceAttackPlugin.Instance.Log(VoiceAttackColor.Red, "Error during plugin initialisation", e);
        }
    }
    
    public static void VA_Invoke1(dynamic vaProxy)
    {
        if(VoiceAttackPlugin.Instance == null) 
            throw new InvalidOperationException("No VoiceAttackPlugin instance found.");
        
        VoiceAttackPlugin.Proxy = new VoiceAttackProxy(vaProxy);
        var context = VoiceAttackPlugin.Proxy.Context;

        try
        {
            VoiceAttackPlugin.Instance.OnInvoke(VoiceAttackPlugin.Proxy, context).GetAwaiter().GetResult();
        } 
        catch (Exception e)
        {
            VoiceAttackPlugin.Instance.Log(VoiceAttackColor.Red, "Error during plugin invocation", e);
        }
    }

    public static void VA_StopCommand()
    {
        if(VoiceAttackPlugin.Instance == null) 
            throw new InvalidOperationException("No VoiceAttackPlugin instance found.");
        
        if(VoiceAttackPlugin.Proxy == null)
            throw new InvalidOperationException("No VoiceAttackProxy instance found.");
        
        try 
        {
            VoiceAttackPlugin.Instance.OnCommandStopped(VoiceAttackPlugin.Proxy).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            VoiceAttackPlugin.Instance.Log(VoiceAttackColor.Red, "Error during command stop", e);
        }
    }
    
    public static void VA_Exit1(dynamic vaProxy)
    {
        if(VoiceAttackPlugin.Instance == null) 
            throw new InvalidOperationException("No VoiceAttackPlugin instance found.");
        
        VoiceAttackPlugin.Proxy = new VoiceAttackProxy(vaProxy);
        
        try
        {
            VoiceAttackPlugin.Instance.OnStop(VoiceAttackPlugin.Proxy).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            VoiceAttackPlugin.Instance.Log(VoiceAttackColor.Red, "Error during plugin exit", e);
        }
    }
}