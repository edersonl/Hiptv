using IptvStarterApp.Domain.Enums;
using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Domain.Entities;

public sealed class Playlist
{
    public Playlist(PlaylistId id, string name, PlaylistType type, Uri? source, IEnumerable<MediaId>? itemIds = null)
    {
        if (id == default) throw new ArgumentException("O identificador da playlist é obrigatório.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome da playlist é obrigatório.", nameof(name));
        if (type == PlaylistType.M3U &&
            (source is null || !source.IsAbsoluteUri ||
             (source.Scheme != Uri.UriSchemeHttp && source.Scheme != Uri.UriSchemeHttps)))
        {
            throw new ArgumentException("Uma playlist M3U remota exige URL HTTP ou HTTPS absoluta.", nameof(source));
        }

        Id = id;
        Name = name.Trim();
        Type = type;
        Source = source;
        ItemIds = (itemIds ?? Array.Empty<MediaId>()).Distinct().ToArray();
    }

    public PlaylistId Id { get; }
    public string Name { get; }
    public PlaylistType Type { get; }
    public Uri? Source { get; }
    public IReadOnlyList<MediaId> ItemIds { get; }
    public DateTimeOffset? LastSynchronizedAt { get; init; }
    public bool IsEnabled { get; init; } = true;
}