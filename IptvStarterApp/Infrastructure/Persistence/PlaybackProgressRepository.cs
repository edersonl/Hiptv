using System.Text.Json;
using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.Interfaces;
using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Infrastructure.Persistence;

public sealed class PlaybackProgressRepository : IPlaybackProgressRepository
{
    private readonly Dictionary<string, PlaybackProgress> _storage = new();

    public Task<PlaybackProgress?> GetAsync(
        string profileId,
        MediaId mediaId,
        CancellationToken cancellationToken = default)
    {
        _storage.TryGetValue(Key(profileId, mediaId), out var progress);
        return Task.FromResult(progress);
    }

    public Task<IReadOnlyList<PlaybackProgress>> GetHistoryAsync(
        string profileId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PlaybackProgress> result = _storage.Values
            .Where(x => x.ProfileId == profileId)
            .OrderByDescending(x => x.LastPlayedAt)
            .ToList();

        return Task.FromResult(result);
    }

    public Task SaveAsync(
        PlaybackProgress progress,
        CancellationToken cancellationToken = default)
    {
        _storage[Key(progress.ProfileId, progress.MediaId)] = progress;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(
        string profileId,
        MediaId mediaId,
        CancellationToken cancellationToken = default)
    {
        _storage.Remove(Key(profileId, mediaId));
        return Task.CompletedTask;
    }

    private static string Key(
        string profileId,
        MediaId mediaId)
    {
        return $"{profileId}:{mediaId}";
    }
}