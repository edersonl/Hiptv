using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.Interfaces;

namespace IptvStarterApp.Application.UseCases.Playback;

public sealed class GetContinueWatchingUseCase
{
    private readonly IContinueWatchingRepository _repository;

    public GetContinueWatchingUseCase(
        IContinueWatchingRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<IReadOnlyList<ContinueWatching>> ExecuteAsync(
        string profileId = "local",
        CancellationToken cancellationToken = default)
    {
        return _repository.ListAsync(
            profileId,
            20,
            cancellationToken);
    }
}