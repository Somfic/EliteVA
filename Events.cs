using System.Text.Json.Serialization;

namespace EliteVA;

public class ServerEvent
{
    [JsonPropertyName("VariablesEvent")]
    public VariablesEvent? VariablesEvent { get; init; } 
    
    [JsonPropertyName("JournalEvent")]
    public string? JournalEvent { get; init; }
}


public class VariablesEvent
{
    [JsonPropertyName("event")]
    public string Event { get; init;  }

    [JsonPropertyName("variables")]
    public Variable[] Variables { get; init;  }
}


public class Variable
{
    [JsonPropertyName("path")]
    public string Path { get; init;  }

    [JsonPropertyName("encoded_value")]
    public string EncodedValue { get; init;  }

    [JsonPropertyName("value_type")]
    public System.ValueType ValueType { get; init;  }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValueType { Int32, Single, String, Boolean, Date };