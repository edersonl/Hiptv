using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.Enums;
using IptvStarterApp.Domain.Interfaces;
using IptvStarterApp.Domain.ValueObjects;

namespace IptvStarterApp.Tests;

internal sealed class FakeContentRepository : IContentRepository
{
    private readonly Dictionary<MediaId, MediaItem> _items = new();

    public Task<MediaItem?> GetByIdAsync(
        MediaId id,
        CancellationToken cancellationToken = default)
    {
        _items.TryGetValue(id, out var media);
        return Task.FromResult(media);
    }

    public Task<IReadOnlyList<MediaItem>> ListAsync(
        MediaType? mediaType = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MediaItem> result = _items.Values.ToList();
        return Task.FromResult(result);
    }

    public Task SaveAsync(
        MediaItem mediaItem,
        CancellationToken cancellationToken = default)
    {
        _items[mediaItem.Id] = mediaItem;
        return Task.CompletedTask;
    }
}
