﻿using System.Text.Json.Serialization;

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

    [JsonPropertyName("set_variables")]
    public SetVariable[] SetVariables { get; init;  }
    
    [JsonPropertyName("unset_variables")]
    public UnsetVariable[] UnsetVariables { get; init;  }
}


public class SetVariable
{
    [JsonPropertyName("path")]
    public string Path { get; init;  }

    [JsonPropertyName("encoded_value")]
    public string EncodedValue { get; init;  }

    [JsonPropertyName("value_type")]
    public ValueType ValueType { get; init;  }
}

public class UnsetVariable
{
    [JsonPropertyName("path")] public string Path { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValueType { Int32, Single, String, Boolean, DateTime };