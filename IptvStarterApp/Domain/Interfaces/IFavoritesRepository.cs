using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Domain.Interfaces;

public interface IFavoritesRepository
{
    Task<bool> ContainsAsync(string profileId, MediaId mediaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Favorite>> ListAsync(string profileId, CancellationToken cancellationToken = default);
    Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default);
    Task RemoveAsync(string profileId, MediaId mediaId, CancellationToken cancellationToken = default);
}