using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using EliteVA.Abstractions;
using EliteVA.Logging;

namespace EliteVA;

public class Plugin : VoiceAttackPlugin
{
    public override Task OnStart(IVoiceAttackProxy proxy)
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                Log(VoiceAttackColor.Gray, "Connecting to EliteAPI...");
                
                try
                {
                    await Connect();
                }
                catch (Exception ex)
                {
                    Log(VoiceAttackColor.Red, "Could not connect to EliteAPI", ex);
                }

                await Task.Delay(1000);
            }
        });

        return Task.CompletedTask;
    }

    private async Task Connect()
    {
        await using var pipeClient = new NamedPipeClientStream(".",  "eliteapi.sock", PipeDirection.InOut);
        await pipeClient.ConnectAsync();
        
        Log(VoiceAttackColor.Green, "Connected to EliteAPI");
        
        using var sr = new StreamReader(pipeClient, Encoding.UTF8);
        await using var sw = new StreamWriter(pipeClient, Encoding.UTF8);
        sw.AutoFlush = true;
        
        while (await sr.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            try
            {
                Log(VoiceAttackColor.Black, line);
            }
            catch (JsonException ex)
            {
                Log(VoiceAttackColor.Red, "Failed to parse JSON", ex);
            }
        }
    }
}