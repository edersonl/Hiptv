using IptvStarterApp.Domain.ValueObjects;
using IptvStarterApp.Domain.Enums;

namespace IptvStarterApp.Domain.Entities;

public sealed class Episode : MediaItem
{
    public Episode(
        MediaId id,
        string name,
        Uri source,
        MediaId seriesId,
        SeasonId seasonId,
        int seasonNumber,
        int episodeNumber,
        string group = "Séries")
        : base(id, name, MediaType.Episode, RequirePlaybackSource(source, nameof(source)), group)
    {
        if (seriesId == default) throw new ArgumentException("O episódio deve pertencer a uma série.", nameof(seriesId));
        if (seasonId == default) throw new ArgumentException("O episódio deve pertencer a uma temporada.", nameof(seasonId));
        if (seasonNumber <= 0) throw new ArgumentOutOfRangeException(nameof(seasonNumber));
        if (episodeNumber <= 0) throw new ArgumentOutOfRangeException(nameof(episodeNumber));

        SeriesId = seriesId;
        SeasonId = seasonId;
        SeasonNumber = seasonNumber;
        EpisodeNumber = episodeNumber;
    }

    public Uri Source => PlaybackSource!;
    public ContentType ContentType => ContentType.Serialized;
    public MediaId SeriesId { get; }
    public SeasonId SeasonId { get; }
    public int SeasonNumber { get; }
    public int EpisodeNumber { get; }
    public TimeSpan? Duration { get; init; }
}
