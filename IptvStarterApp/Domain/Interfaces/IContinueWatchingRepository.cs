using IptvStarterApp.Domain.Entities;

namespace IptvStarterApp.Domain.Interfaces;

public interface IContinueWatchingRepository
{
    Task<IReadOnlyList<ContinueWatching>> ListAsync(
        string profileId,
        int limit = 20,
        CancellationToken cancellationToken = default);
}