using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Domain.Interfaces;

public interface IPlaybackProgressRepository
{
    Task<PlaybackProgress?> GetAsync(string profileId, MediaId mediaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlaybackProgress>> GetHistoryAsync(string profileId, CancellationToken cancellationToken = default);
    Task SaveAsync(PlaybackProgress progress, CancellationToken cancellationToken = default);
    Task RemoveAsync(string profileId, MediaId mediaId, CancellationToken cancellationToken = default);
}