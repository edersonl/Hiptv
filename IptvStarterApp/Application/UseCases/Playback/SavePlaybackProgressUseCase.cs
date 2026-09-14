using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.Interfaces;

namespace IptvStarterApp.Application.UseCases.Playback;

public sealed class SavePlaybackProgressUseCase
{
    private readonly IPlaybackProgressRepository _repository;

    public SavePlaybackProgressUseCase(
        IPlaybackProgressRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task ExecuteAsync(
        PlaybackProgress progress,
        CancellationToken cancellationToken = default)
    {
        return _repository.SaveAsync(
            progress,
            cancellationToken);
    }
}