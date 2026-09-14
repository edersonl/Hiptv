using IptvStarterApp.Domain.Enums;
using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Domain.Entities;

public sealed class Movie : MediaItem
{
    public Movie(MediaId id, string name, Uri source, string group = "Filmes")
        : base(id, name, MediaType.Movie, RequirePlaybackSource(source, nameof(source)), group)
    {
    }

    public Uri Source => PlaybackSource!;
    public ContentType ContentType => ContentType.VideoOnDemand;
    public TimeSpan? Duration { get; init; }
    public int? ReleaseYear { get; init; }
    public decimal? Rating { get; init; }
}