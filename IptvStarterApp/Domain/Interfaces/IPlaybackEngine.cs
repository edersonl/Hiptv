using IptvStarterApp.Domain.Playback;

namespace IptvStarterApp.Domain.Interfaces;

public interface IPlaybackEngine
{
    event EventHandler<PlaybackStateChangedEventArgs>? StateChanged;

    TimeSpan Position { get; }

    TimeSpan? Duration { get; }

    Task PlayAsync(
        PlaybackRequest request,
        CancellationToken cancellationToken = default);

    Task PauseAsync(
        CancellationToken cancellationToken = default);

    Task StopAsync(
        CancellationToken cancellationToken = default);
}