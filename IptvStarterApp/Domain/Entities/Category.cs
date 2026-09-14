using IptvStarterApp.Domain.Enums;
using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Domain.Entities;

public sealed class Category
{
    public Category(CategoryId id, string name, ContentType contentType, CategoryId? parentId = null, int sortOrder = 0)
    {
        if (id == default) throw new ArgumentException("O identificador da categoria é obrigatório.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome da categoria é obrigatório.", nameof(name));
        if (parentId == id) throw new ArgumentException("Uma categoria não pode ser pai de si mesma.", nameof(parentId));

        Id = id;
        Name = name.Trim();
        ContentType = contentType;
        ParentId = parentId;
        SortOrder = sortOrder;
    }

    public CategoryId Id { get; }
    public string Name { get; }
    public ContentType ContentType { get; }
    public CategoryId? ParentId { get; }
    public int SortOrder { get; }
}