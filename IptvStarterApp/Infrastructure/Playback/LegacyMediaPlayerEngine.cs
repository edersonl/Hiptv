using Android.Media;
using IptvStarterApp.Domain.Interfaces;
using IptvStarterApp.Domain.Playback;

namespace IptvStarterApp.Infrastructure.Playback;

public sealed class LegacyMediaPlayerEngine : Java.Lang.Object, IPlaybackEngine,
    MediaPlayer.IOnPreparedListener, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnErrorListener
{
    private readonly object _sync = new();
    private MediaPlayer? _player;
    private bool _disposed;

    public event EventHandler<PlaybackStateChangedEventArgs>? StateChanged;

    public Task PlayAsync(PlaybackRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            ThrowIfDisposed();
            ReleasePlayer();
            _player = new MediaPlayer();
            var audioAttributes = new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Media)
                .SetContentType(AudioContentType.Movie)
                .Build() ?? throw new InvalidOperationException("Não foi possível configurar os atributos de áudio.");
            _player.SetAudioAttributes(audioAttributes);
            _player.SetOnPreparedListener(this);
            _player.SetOnCompletionListener(this);
            _player.SetOnErrorListener(this);
            _player.SetDataSource(request.Url);
            RaiseState(PlaybackState.Buffering);
            _player.PrepareAsync();
        }

        return Task.CompletedTask;
    }

    public Task PauseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            if (_player?.IsPlaying == true)
            {
                _player.Pause();
                RaiseState(PlaybackState.Paused);
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            ReleasePlayer();
            RaiseState(PlaybackState.Stopped);
        }

        return Task.CompletedTask;
    }

    public void OnPrepared(MediaPlayer? mp)
    {
        if (mp is null)
        {
            RaiseState(PlaybackState.Error, "O player legado não foi preparado.");
            return;
        }

        mp.Start();
        RaiseState(PlaybackState.Playing);
    }

    public void OnCompletion(MediaPlayer? mp) => RaiseState(PlaybackState.Completed);

    public bool OnError(MediaPlayer? mp, MediaError what, int extra)
    {
        RaiseState(PlaybackState.Error, $"Falha no player legado: {what} ({extra}).");
        return true;
    }

    public new void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            ReleasePlayer();
        }

        base.Dispose();
    }

    private void ReleasePlayer()
    {
        if (_player is null)
        {
            return;
        }

        try
        {
            if (_player.IsPlaying)
            {
                _player.Stop();
            }
        }
        catch (Java.Lang.IllegalStateException)
        {
        }

        _player.Release();
        _player = null;
    }

    private void RaiseState(PlaybackState state, string? message = null) =>
        StateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(state, message));

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(LegacyMediaPlayerEngine));
        }
    }
}
