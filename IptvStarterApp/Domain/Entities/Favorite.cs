using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Domain.Entities;

public sealed class Favorite
{
    public Favorite(MediaItem mediaItem, DateTimeOffset addedAt, string profileId = "local")
    {
        MediaItem = mediaItem ?? throw new ArgumentNullException(nameof(mediaItem));
        if (string.IsNullOrWhiteSpace(profileId)) throw new ArgumentException("O perfil é obrigatório.", nameof(profileId));
        if (addedAt == default) throw new ArgumentException("A data de inclusão é obrigatória.", nameof(addedAt));

        MediaId = mediaItem.Id;
        ProfileId = profileId.Trim();
        AddedAt = addedAt.ToUniversalTime();
    }

    public MediaId MediaId { get; }
    public MediaItem MediaItem { get; }
    public string ProfileId { get; }
    public DateTimeOffset AddedAt { get; }
}