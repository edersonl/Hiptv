using IptvStarterApp.Domain.Interfaces;
using IptvStarterApp.Domain.Entities;

namespace IptvStarterApp.Application.UseCases;

public sealed class LoadPlaylistUseCase
{
    private readonly IPlaylistRepository _playlistRepository;

    public LoadPlaylistUseCase(IPlaylistRepository playlistRepository)
    {
            _playlistRepository = playlistRepository
            ?? throw new ArgumentNullException(nameof(playlistRepository));
    }

    public Task<IReadOnlyList<Channel>> ExecuteAsync(
        string playlistUrl,
        CancellationToken cancellationToken = default)
    {
        return _playlistRepository.LoadAsync(playlistUrl, cancellationToken);
    }
}
