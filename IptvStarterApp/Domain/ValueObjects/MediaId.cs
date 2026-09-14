namespace IptvStarterApp.Domain.ValueObjects;

public readonly record struct MediaId
{
    public MediaId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("O identificador de mídia não pode ser vazio.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
