namespace EliteVA.Services.Documentation;

public readonly struct RecordDocumentation
{
    public RecordDocumentation(string name, IEnumerable<string> types, IEnumerable<string> values)
    {
        Name = name;
        Types = types.Distinct().ToArray();
        Values = values.Distinct().ToArray();
    }

    public string Name { get; }
    public string[] Types { get; }
    public string[] Values { get; }
}