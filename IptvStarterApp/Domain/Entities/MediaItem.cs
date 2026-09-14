using IptvStarterApp.Domain.ValueObjects;
using IptvStarterApp.Domain.Enums;

namespace IptvStarterApp.Domain.Entities;

public abstract class MediaItem
{
    protected MediaItem(
        MediaId id,
        string name,
        MediaType mediaType,
        Uri? playbackSource = null,
        string group = "General")
    {
        if (id == default)
        {
            throw new ArgumentException("O identificador da mídia é obrigatório.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("O nome da mídia é obrigatório.", nameof(name));
        }

        Id = id;
        Name = name.Trim();
        MediaType = mediaType;
        PlaybackSource = playbackSource;
        Group = string.IsNullOrWhiteSpace(group) ? "General" : group.Trim();
    }

    public MediaId Id { get; }
    public string Name { get; }
    public string Title => Name;
    public MediaType MediaType { get; }
    public Uri? PlaybackSource { get; }
    public string Group { get; }
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public Uri? ArtworkUri { get; init; }
    public IReadOnlySet<CategoryId> CategoryIds { get; init; } = new HashSet<CategoryId>();
    public IReadOnlyDictionary<string, string> Attributes { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    protected static Uri RequirePlaybackSource(Uri? source, string parameterName)
    {
        if (source is null || !source.IsAbsoluteUri ||
            (source.Scheme != Uri.UriSchemeHttp && source.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("A URL de reprodução deve ser HTTP ou HTTPS e absoluta.", parameterName);
        }

        return source;
    }
}
