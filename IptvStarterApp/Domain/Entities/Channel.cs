using IptvStarterApp.Domain.ValueObjects;
using IptvStarterApp.Domain.Enums;

namespace IptvStarterApp.Domain.Entities;

public sealed class Channel : MediaItem
{
    public Channel(MediaId id, string name, Uri source, string group = "General")
        : base(id, name, MediaType.Channel, RequirePlaybackSource(source, nameof(source)), group)
    {
    }

    public Uri Source => PlaybackSource!;
    public ContentType ContentType => ContentType.Live;
    public string? TvgId { get; init; }
    public string? TvgName { get; init; }
    public int? Number { get; init; }
}
