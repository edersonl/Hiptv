using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Domain.Entities;

public sealed class ContinueWatching
{
    public ContinueWatching(MediaItem mediaItem, PlaybackProgress progress, MediaId? nextEpisodeId = null)
    {
        MediaItem = mediaItem ?? throw new ArgumentNullException(nameof(mediaItem));
        Progress = progress ?? throw new ArgumentNullException(nameof(progress));
        if (mediaItem.Id != progress.MediaId) throw new ArgumentException("O progresso deve pertencer ao item de mídia.", nameof(progress));
        if (mediaItem is Channel) throw new ArgumentException("Canais ao vivo não participam de Continue Watching.", nameof(mediaItem));
        if (!progress.IsEligibleForContinueWatching) throw new ArgumentException("O progresso não é elegível para Continue Watching.", nameof(progress));

        NextEpisodeId = nextEpisodeId;
    }

    public MediaId MediaId => MediaItem.Id;
    public MediaItem MediaItem { get; }
    public PlaybackProgress Progress { get; }
    public MediaId? NextEpisodeId { get; }
    public double Percentage => Progress.Percentage;
    public DateTimeOffset LastPlayedAt => Progress.LastPlayedAt;
}