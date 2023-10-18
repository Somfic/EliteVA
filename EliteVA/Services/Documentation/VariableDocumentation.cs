namespace EliteVA.Services.Documentation;

public readonly struct VariableDocumentation
{
    public VariableDocumentation(string category, string name, string type, string value)
    {
        Category = category;
        Name = name;
        Value = value;
    }

    public string Category { get; }
    public string Name { get; }
    public string Value { get; }
}