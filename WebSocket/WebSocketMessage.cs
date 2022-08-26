using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EliteVA.WebSocket;

public record WebSocketMessage<T>(MessageType Type, T Payload)
{
    [JsonConverter(typeof(StringEnumConverter))]
    public MessageType Type { get; } = Type;

    public T Payload { get; } = Payload;
}