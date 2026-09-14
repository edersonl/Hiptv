using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.Enums;
using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Domain.Interfaces;

public interface IContentRepository
{
    Task<MediaItem?> GetByIdAsync(MediaId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MediaItem>> ListAsync(MediaType? mediaType = null, CancellationToken cancellationToken = default);
    Task SaveAsync(MediaItem mediaItem, CancellationToken cancellationToken = default);
}