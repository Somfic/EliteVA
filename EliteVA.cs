using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EliteVA.Proxy;
using EliteVA.Proxy.Log;
using EliteVA.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EliteVA;

public class EliteVA
{
    private readonly ProxyHolder _proxy;
    private readonly ClientWebSocket _socket;

    public EliteVA(ClientWebSocket socket, ProxyHolder proxy)
    {
        _socket = socket;
        _proxy = proxy;
    }

    public async Task Run()
    {
        await _proxy.Get().Log.Write("Connecting to EliteAPI ... ", VoiceAttackColor.Gray);
        await _socket.ConnectAsync(new Uri("ws://localhost:51555/"), CancellationToken.None);
        await Send(new WebSocketMessage<string>(MessageType.Authentication, "VoiceAttack"));
        await _proxy.Get().Log.Write("Connected to EliteAPI", VoiceAttackColor.Green);
        
        var buffer = new ArraySegment<byte>(new byte[8192]);

        while (true)
        {
            try
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await _socket.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);

                if (result.MessageType != WebSocketMessageType.Text) continue;

                using var reader = new StreamReader(ms, Encoding.UTF8);
                var message = await reader.ReadToEndAsync();
                await Handle(message);
            }
            catch (Exception e)
            {
                await _proxy.Get().Log.Write($"Error: {e.Message}", VoiceAttackColor.Red);
                await _proxy.Get().Log.Write($"Error: {e.StackTrace}", VoiceAttackColor.Yellow);
            }
        }
    }

    private async Task Handle(string json)
    {
        var rawMessage = JsonConvert.DeserializeObject<WebSocketMessage<object>>(json);

        if (rawMessage.Type != MessageType.Paths)
            return;

        var message = JsonConvert.DeserializeObject<WebSocketMessage<PathsPayload>>(json);

        foreach (var (path, jsonValue) in message.Payload.Paths)
        {
            try
            {
                var value = JsonConvert.DeserializeObject(jsonValue);
                var token = JToken.Parse(string.IsNullOrWhiteSpace(jsonValue) ? "''" : jsonValue);

                _proxy.Get().Variables.Set("Events", $"EliteAPI.{path}", value, ToTypeCode(token.Type));
            }
            catch (Exception e)
            {
                await _proxy.Get().Log.Write($"Cannot parse {path} for {jsonValue}: {e.Message}", VoiceAttackColor.Red);
                await _proxy.Get().Log.Write($"Error: {e.StackTrace}", VoiceAttackColor.Yellow);
            }
        }
        
        _proxy.Get().Variables.Set("Context", "EliteAPI.IsImplemented", message.Payload.Context.IsImplemented, TypeCode.Boolean);
        _proxy.Get().Variables.Set("Context", "EliteAPI.SourceFile", message.Payload.Context.SourceFile, TypeCode.String);
        _proxy.Get().Variables.Set("Context", "EliteAPI.IsRaisedDuringCatchup", message.Payload.Context.IsRaisedDuringCatchup, TypeCode.Boolean);

        var eventName = JsonConvert.DeserializeObject<string>(message.Payload.Paths.First(x => x.Path.EndsWith("Event")).Value);
        var commandName = $"((EliteAPI.{eventName}))";

        if (await _proxy.Get().Commands.Exists(commandName))
            await _proxy.Get().Commands.Invoke(commandName);

        await Send(new WebSocketMessage<IReadOnlyDictionary<(string category, string name),string>>(MessageType.Variables, _proxy.Get().Variables.SetVariables));
    }

    private TypeCode ToTypeCode(JTokenType tokenType)
    {
        switch (tokenType)
        {
            case JTokenType.Integer:
                return TypeCode.Int32;
            
            case JTokenType.Float:
                return TypeCode.Decimal;
            
            case JTokenType.String:
            case JTokenType.Guid:
            case JTokenType.Uri:
                return TypeCode.String;
            
            case JTokenType.Boolean:
                return TypeCode.Boolean;

            case JTokenType.Date:
            case JTokenType.TimeSpan:
                return TypeCode.DateTime;
            
            case JTokenType.Bytes:
                return TypeCode.Byte;
            
            default:
                return TypeCode.Empty;
        }
    }

    private Task Send(string data)
    {
        var encoded = Encoding.UTF8.GetBytes(data);
        var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
        return _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }
    
    private Task Send<T>(WebSocketMessage<T> message) => Send(JsonConvert.SerializeObject(message));
}