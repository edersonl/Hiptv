using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.Interfaces;

namespace IptvStarterApp.Infrastructure.Persistence;

public sealed class ContinueWatchingRepository : IContinueWatchingRepository
{
    private readonly IPlaybackProgressRepository _progressRepository;

    public ContinueWatchingRepository(
        IPlaybackProgressRepository progressRepository)
    {
        _progressRepository = progressRepository;
    }

    public async Task<IReadOnlyList<ContinueWatching>> ListAsync(
        string profileId,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var history =
            await _progressRepository.GetHistoryAsync(
                profileId,
                cancellationToken);

        return Array.Empty<ContinueWatching>();
    }
}