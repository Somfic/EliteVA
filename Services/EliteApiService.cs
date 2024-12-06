using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using System.IO.Pipes;
using System.Text;
using EliteVA.Proxy.Logging;
using System.Text.Json;

namespace VoiceAttack.Services;

public class EliteApiService : VoiceAttackService
{
    private const string PipeName = "eliteapi";
    
    public override Task OnStart(IVoiceAttackProxy proxy)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await ConnectToPipe(PipeName, proxy);
            }
        });
        
        return Task.CompletedTask;
    }

    private static async Task ConnectToPipe(string pipeName, IVoiceAttackProxy proxy)
    {
        using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync();
        proxy.Log.Write("Connected to EliteAPI", VoiceAttackColor.Green);
        
        using var reader = new StreamReader(pipe, Encoding.UTF8);
        
        while (pipe.IsConnected)
        {
            try
            {
                var message = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(message))
                    continue;

                try
                {
                    await HandleMessage(message, proxy);
                } catch (Exception e)
                {
                    proxy.Log.Write($"Error handling message from EliteAPI: {e.Message}", VoiceAttackColor.Yellow);
                }
            } catch (Exception e)
            {
                proxy.Log.Write($"Error reading message from EliteAPI: {e.Message}", VoiceAttackColor.Yellow);
            }
        }
        
        proxy.Log.Write("Disconnected from EliteAPI", VoiceAttackColor.Red);
    }
    
    private static async Task HandleMessage(string messageJson, IVoiceAttackProxy proxy)
    {
        var message = JsonSerializer.Deserialize<EliteApiMessage>(messageJson);
        
        proxy.Log.Write($"Received {message.Commands.Count} commands from EliteAPI", VoiceAttackColor.Blue);

        foreach (var command in message.Commands)
        {
            try
            {
                await HandleCommand(command, proxy);
            } catch (Exception e)
            {
                proxy.Log.Write($"Error handling command {command.Type}: {e.Message}", VoiceAttackColor.Yellow);
            }
        }
    }
    
    private static Task HandleCommand(EliteApiCommand command, IVoiceAttackProxy proxy)
    {
        return command.Type switch
        {
            EliteApiCommandType.SetVariable => HandleSetVariable(command, proxy),
            EliteApiCommandType.ClearVariable => HandleClearVariable(command, proxy),
            EliteApiCommandType.ClearVariablesStartingWith => HandleClearVariablesStartingWith(command, proxy),
            _ => throw new ArgumentOutOfRangeException(nameof(command.Type), command.Type, $"Unimplemented command type: {command.Type}''")
        };
    }
    
    private static Task HandleSetVariable(EliteApiCommand command, IVoiceAttackProxy proxy)
    {
        var category = command.Arguments.Expect(0, "category");
        var name = command.Arguments.Expect(1, "name");
        var type = command.Arguments.ExpectTypeCode(2, "type");
        var value = command.Arguments.ExpectObject(3, "value");
        
        proxy.Variables.Set(category, name, value, type);
       
        return Task.CompletedTask;     
    }
    
    private static Task HandleClearVariable(EliteApiCommand command, IVoiceAttackProxy proxy)
    {
        var category = command.Arguments.Expect(0, "category");
        var name = command.Arguments.Expect(1, "name");
        var type = command.Arguments.ExpectTypeCode(2, "type");
        
        proxy.Variables.Clear(category, name, type);
        
        return Task.CompletedTask;
    }
    
    private static Task HandleClearVariablesStartingWith(EliteApiCommand command, IVoiceAttackProxy proxy)
    {
        var category = command.Arguments.Expect(0, "category");
        var prefix = command.Arguments.Expect(1, "prefix");
        
        proxy.Variables.ClearStartingWith(category, prefix);
        
        return Task.CompletedTask;
    }

    private struct EliteApiMessage
    {
        public IReadOnlyCollection<EliteApiCommand> Commands { get; }
    }
    
    private  struct EliteApiCommand
    {
        public EliteApiCommandType Type { get; }
            
        public string[] Arguments { get; }
    }
    
    private enum EliteApiCommandType
    {
        SetVariable,
        ClearVariable,
        ClearVariablesStartingWith
    }
}

internal static class EliteApiCommandExtensions
{
    public static string Expect(this string[] array, int index, string expected)
    {
        if (index >= array.Length)
            throw new IndexOutOfRangeException($"Expected '{expected}' at index {index}");
        
        return array[index];
    }
    
    public static object ExpectObject(this string[] array, int index, string expected)
    {
        var json = array.Expect(index, expected);
        
        try
        {
            var obj = JsonSerializer.Deserialize<object>(json);
            
            if (obj is null)
                throw new JsonException($"Expected JSON object at index {index}");
            
            return obj;
        }
        catch (JsonException e)
        {
            throw new JsonException($"Expected JSON object at index {index}: {e.Message}", e);
        }
    }
    
    public static TypeCode ExpectTypeCode(this string[] array, int index, string expected)
    {
        var type = array.Expect(index, expected);
        
        if (!Enum.TryParse<TypeCode>(type, out var typeCode))
            throw new ArgumentException($"Expected TypeCode at index {index}: '{type}' is not a valid TypeCode");
        
        return typeCode;
    }
}