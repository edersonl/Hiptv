using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.Interfaces;

namespace IptvStarterApp.Infrastructure.Persistence;

public sealed class ContinueWatchingRepository : IContinueWatchingRepository
{
    private readonly IPlaybackProgressRepository _progressRepository;
    private readonly IContentRepository _contentRepository;

    public ContinueWatchingRepository(
        IPlaybackProgressRepository progressRepository,
        IContentRepository contentRepository)
    {
        _progressRepository = progressRepository
            ?? throw new ArgumentNullException(nameof(progressRepository));

        _contentRepository = contentRepository
            ?? throw new ArgumentNullException(nameof(contentRepository));
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

        var eligible =
            history
                .Where(progress => progress.IsEligibleForContinueWatching)
                .Take(limit)
                .ToList();

        var result = new List<ContinueWatching>();

        foreach (var progress in eligible)
        {
            var mediaItem =
                await _contentRepository.GetByIdAsync(
                    progress.MediaId,
                    cancellationToken);

            if (mediaItem is null)
            {
                continue;
            }

            result.Add(
                new ContinueWatching(
                    mediaItem,
                    progress));
        }

        return result;
    }
}