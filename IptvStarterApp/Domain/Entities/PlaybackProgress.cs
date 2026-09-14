using IptvStarterApp.Domain.Enums;
using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Domain.Entities;

public sealed class PlaybackProgress
{
    public PlaybackProgress(
        MediaId mediaId,
        TimeSpan position,
        TimeSpan? duration,
        DateTimeOffset lastPlayedAt,
        int playCount = 1,
        string profileId = "local")
    {
        if (mediaId == default) throw new ArgumentException("O identificador da mídia é obrigatório.", nameof(mediaId));
        if (position < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(position));
        if (duration is { } suppliedDuration && suppliedDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }
        if (duration is { } knownDuration && position > knownDuration) throw new ArgumentException("A posição não pode exceder a duração.", nameof(position));
        if (lastPlayedAt == default) throw new ArgumentException("A data de reprodução é obrigatória.", nameof(lastPlayedAt));
        if (playCount < 0) throw new ArgumentOutOfRangeException(nameof(playCount));
        if (string.IsNullOrWhiteSpace(profileId)) throw new ArgumentException("O perfil é obrigatório.", nameof(profileId));

        MediaId = mediaId;
        Position = position;
        Duration = duration;
        LastPlayedAt = lastPlayedAt.ToUniversalTime();
        PlayCount = playCount;
        ProfileId = profileId.Trim();
        Status = ResolveStatus(position, duration);
        CompletedAt = Status == PlaybackStatus.Completed ? LastPlayedAt : null;
    }

    public MediaId MediaId { get; }
    public TimeSpan Position { get; }
    public TimeSpan? Duration { get; }
    public DateTimeOffset LastPlayedAt { get; }
    public DateTimeOffset? CompletedAt { get; }
    public int PlayCount { get; }
    public string ProfileId { get; }
    public PlaybackStatus Status { get; }
    public double Percentage => Duration is { Ticks: > 0 } duration
        ? Math.Min(1d, Position.TotalMilliseconds / duration.TotalMilliseconds)
        : 0d;
    public bool IsEligibleForContinueWatching =>
        Status == PlaybackStatus.InProgress && Position >= TimeSpan.FromSeconds(30);

    private static PlaybackStatus ResolveStatus(TimeSpan position, TimeSpan? duration)
    {
        if (position == TimeSpan.Zero) return PlaybackStatus.NotStarted;
        if (duration is not { Ticks: > 0 } knownDuration) return PlaybackStatus.InProgress;

        var remaining = knownDuration - position;
        return position.TotalMilliseconds / knownDuration.TotalMilliseconds >= 0.9 ||
               remaining <= TimeSpan.FromMinutes(2)
            ? PlaybackStatus.Completed
            : PlaybackStatus.InProgress;
    }
}