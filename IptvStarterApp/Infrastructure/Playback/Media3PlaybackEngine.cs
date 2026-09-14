using Android.Content;
using AndroidX.Media3.Common;
using AndroidX.Media3.ExoPlayer;
using AndroidX.Media3.UI;
using IptvStarterApp.Domain.Interfaces;
using IptvStarterApp.Domain.Playback;
using DomainPlaybackStateChangedEventArgs = IptvStarterApp.Domain.Playback.PlaybackStateChangedEventArgs;

namespace IptvStarterApp.Infrastructure.Playback;

public sealed class Media3PlaybackEngine : Java.Lang.Object, IPlaybackEngine
{
    private readonly IPlayer _player;
    private readonly PlayerListener _playerListener;
    private readonly PlayerView _playerView;
    private bool _disposed;

    public Media3PlaybackEngine(Context context, PlayerView playerView)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(playerView);
        _player = new ExoPlayerBuilder(context.ApplicationContext).Build()
            ?? throw new InvalidOperationException("Media3 não criou uma instância do player.");
        _playerListener = new PlayerListener(this);
        _playerView = playerView;
        _playerView.Player = _player;
        _player.AddListener(_playerListener);
    }

    public event EventHandler<DomainPlaybackStateChangedEventArgs>? StateChanged;

    public Task PlayAsync(PlaybackRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var uri = Android.Net.Uri.Parse(request.Url)
            ?? throw new ArgumentException("A URL de reprodução é inválida.", nameof(request));
        var builder = new MediaItem.Builder();
        builder.SetUri(uri);
        if (request.IsHls)
        {
            builder.SetMimeType(MimeTypes.ApplicationM3u8);
        }

        var mediaItem = builder.Build()
            ?? throw new InvalidOperationException("Media3 não criou o item de mídia.");
        _player.SetMediaItem(mediaItem);
        if (request.ResumePosition is { } resume && resume > TimeSpan.Zero)
        {
            _player.SeekTo((long)resume.TotalMilliseconds);
        }

        RaiseState(PlaybackState.Buffering);
        _player.Prepare();
        _player.Play();
        return Task.CompletedTask;
    }

    public Task PauseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        _player.Pause();
        RaiseState(PlaybackState.Paused);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_disposed)
        {
            _player.Stop();
            RaiseState(PlaybackState.Stopped);
        }

        return Task.CompletedTask;
    }

    public new void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    _player.RemoveListener(_playerListener);
    _playerView.Player = null;
        _player.Release();
        base.Dispose();
    }

    private void RaiseState(PlaybackState state, string? message = null) =>
        StateChanged?.Invoke(this, new DomainPlaybackStateChangedEventArgs(state, message));

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(Media3PlaybackEngine));
        }
    }

    private sealed class PlayerListener : Java.Lang.Object, IPlayerListener
    {
        private readonly Media3PlaybackEngine _engine;

        public PlayerListener(Media3PlaybackEngine engine)
        {
            _engine = engine;
        }

        public void OnPlaybackStateChanged(int playbackState)
        {
            switch (playbackState)
            {
                case 2:
                    _engine.RaiseState(PlaybackState.Buffering);
                    break;
                case 3:
                    _engine.RaiseState(PlaybackState.Playing);
                    break;
                case 4:
                    _engine.RaiseState(PlaybackState.Completed);
                    break;
            }
        }

        public void OnIsPlayingChanged(bool isPlaying)
        {
            if (!isPlaying)
            {
                return;
            }

            _engine.RaiseState(PlaybackState.Playing);
        }

        public void OnPlayerError(PlaybackException? error)
        {
            _engine.RaiseState(PlaybackState.Error, error?.Message);
        }
    }
}
