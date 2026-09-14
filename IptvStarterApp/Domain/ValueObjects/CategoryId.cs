namespace IptvStarterApp.Domain.ValueObjects;

public readonly record struct CategoryId
{
    public CategoryId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("O identificador da categoria não pode ser vazio.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}