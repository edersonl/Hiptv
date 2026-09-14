namespace IptvStarterApp.Domain.ValueObjects;

public readonly record struct PlaylistId
{
    public PlaylistId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("O identificador da playlist não pode ser vazio.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}