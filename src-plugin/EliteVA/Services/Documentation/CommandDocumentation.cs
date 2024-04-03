namespace EliteVA.Services.Documentation;

public readonly struct CommandDocumentation
{
    public CommandDocumentation(DateTime timestamp, string name)
    {
        Timestamp = timestamp;
        Name = name;
    }

    public DateTime Timestamp { get; }

    public string Name { get; }
}