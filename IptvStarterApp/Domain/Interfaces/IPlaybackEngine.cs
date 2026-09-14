using IptvStarterApp.Domain.Playback;

namespace IptvStarterApp.Domain.Interfaces;

public interface IPlaybackEngine
{
    event EventHandler<PlaybackStateChangedEventArgs>? StateChanged;

    Task PlayAsync(PlaybackRequest request, CancellationToken cancellationToken = default);
    Task PauseAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
