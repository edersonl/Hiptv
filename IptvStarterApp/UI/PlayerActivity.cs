using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;
using AndroidX.Media3.UI;
using IptvStarterApp.Domain.Interfaces;
using IptvStarterApp.Domain.Playback;
using IptvStarterApp.Domain.Entities;
using IptvStarterApp.Domain.ValueObjects;
using IptvStarterApp.Infrastructure.Playback;
using IptvStarterApp.Models;
using IptvStarterApp.Services;

namespace IptvStarterApp.UI
{
    [Activity(Label = "Player")]
    public class PlayerActivity : Activity
    {
        private IPlaybackEngine? _playbackEngine;
        private TextView? _titleView;
        private ProgressBar? _progressBar;
        private PlayerView? _playerView;
        private Button? _favoriteButton;
        private TextView? _channelInfoView;
        private string _videoUrl = string.Empty;
        private string _channelName = string.Empty;
        private string _channelGroup = "TV ao vivo";
        private string _nextChannelName = string.Empty;
        private string _nextChannelUrl = string.Empty;
        private string _nextChannelGroup = string.Empty;
        private bool _usingLegacyPlayer;
        private bool _released;
        private readonly SemaphoreSlim _fallbackLock = new(1, 1);
        private CancellationTokenSource? _playbackLifecycle;
        private PlaybackProgressService? _playbackProgressService;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_player);

            _titleView = FindViewById<TextView>(Resource.Id.playerTitle);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.playerProgress);
            _playerView = FindViewById<PlayerView>(Resource.Id.mediaPlayerView);
            _favoriteButton = FindViewById<Button>(Resource.Id.favoriteButton);
            _channelInfoView = FindViewById<TextView>(Resource.Id.channelInfo);

            _channelName = Intent?.GetStringExtra("channel_name") ?? "Canal IPTV";
            _videoUrl = Intent?.GetStringExtra("channel_url") ?? string.Empty;
            _channelGroup = Intent?.GetStringExtra("channel_group") ?? "TV ao vivo";
            _nextChannelName = Intent?.GetStringExtra("next_channel_name") ?? string.Empty;
            _nextChannelUrl = Intent?.GetStringExtra("next_channel_url") ?? string.Empty;
            _nextChannelGroup = Intent?.GetStringExtra("next_channel_group") ?? string.Empty;
            _playbackProgressService =
                new PlaybackProgressService(this);

            InitializePlaybackEngine();

            if (_titleView is not null)
            {
                _titleView.Text = _channelName;
            }

            if (_channelInfoView is not null)
            {
                var host = Uri.TryCreate(_videoUrl, UriKind.Absolute, out var source) ? source.Host : "Fonte externa";
                _channelInfoView.Text = $"{_channelGroup}  •  {host}";
            }

            var playButton = FindViewById<Button>(Resource.Id.playButton);
            if (playButton is not null)
            {
                playButton.Click += async (_, _) => await StartPlaybackAsync();
            }

            var stopButton = FindViewById<Button>(Resource.Id.stopButton);
            if (stopButton is not null)
            {
                stopButton.Click += async (_, _) => await StopPlaybackAsync();
            }

            if (_favoriteButton is not null)
            {
                var favoriteChannel = new ChannelItem { Name = _channelName, Url = _videoUrl, Group = _channelGroup };
                var favoritesService = new FavoritesService(this);
                _favoriteButton.Text = favoritesService.IsFavorite(favoriteChannel) ? "Remover favorito" : "Favoritar";
                _favoriteButton.Click += (_, _) =>
                {
                    favoritesService.ToggleFavorite(favoriteChannel);
                    var isFavorite = favoritesService.IsFavorite(favoriteChannel);
                    _favoriteButton.Text = isFavorite ? "Remover favorito" : "Favoritar";
                    Toast.MakeText(this, isFavorite ? "Adicionado aos favoritos." : "Removido dos favoritos.", ToastLength.Short)?.Show();
                };
            }

            var nextButton = FindViewById<Button>(Resource.Id.nextChannelButton);
            if (nextButton is not null)
            {
                nextButton.Text = string.IsNullOrWhiteSpace(_nextChannelName)
                    ? "Próximo indisponível"
                    : $"Próximo: {_nextChannelName}";
                nextButton.Enabled = !string.IsNullOrWhiteSpace(_nextChannelUrl);
                nextButton.Click += (_, _) => OpenNextChannel();
            }

            _ = StartPlaybackAsync();
        }

        private void OpenNextChannel()
        {
            if (string.IsNullOrWhiteSpace(_nextChannelUrl)) return;
            var next = new ChannelItem { Name = _nextChannelName, Url = _nextChannelUrl, Group = _nextChannelGroup };
            new ChannelStoreService(this).AddRecent(next);
            var intent = new Android.Content.Intent(this, typeof(PlayerActivity));
            intent.PutExtra("channel_name", next.Name);
            intent.PutExtra("channel_url", next.Url);
            intent.PutExtra("channel_group", next.Group);
            StartActivity(intent);
            Finish();
        }

        private IPlaybackEngine CreatePlaybackEngine()
        {
            try
            {
                _usingLegacyPlayer = false;
                return new Media3PlaybackEngine(this, _playerView!);
            }
            catch
            {
                Android.Util.Log.Warn("Hiptv/Playback", "Media3 indisponível; usando player legado.");
                _usingLegacyPlayer = true;
                return new LegacyMediaPlayerEngine();
            }
        }

        private void InitializePlaybackEngine()
        {
            if (_playbackEngine is not null)
            {
                return;
            }

            _released = false;
            _playbackLifecycle?.Dispose();
            _playbackLifecycle = new CancellationTokenSource();
            _playbackEngine = DecoratePlaybackEngine(CreatePlaybackEngine());
            _playbackEngine.StateChanged += OnPlaybackStateChanged;
        }

        private static IPlaybackEngine DecoratePlaybackEngine(IPlaybackEngine engine) =>
            new HardenedPlaybackEngine(engine, new PlaybackDiagnostics(
                message => Android.Util.Log.Debug("Hiptv/Playback", message)));

        private async Task StartPlaybackAsync()
        {
            if (string.IsNullOrWhiteSpace(_videoUrl))
            {
                Toast.MakeText(this, "URL do canal não disponível.", ToastLength.Short)?.Show();
                return;
            }

            try
            {
                SetProgressVisible(true);
                var playbackEngine = _playbackEngine;
                var cancellationToken = _playbackLifecycle?.Token ?? CancellationToken.None;
                if (playbackEngine is null || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await playbackEngine.PlayAsync(new PlaybackRequest(_videoUrl, _channelName), cancellationToken);
            }
            catch (System.OperationCanceledException) when (_released)
            {
            }
            catch (Exception ex)
            {
                Android.Util.Log.Warn("Hiptv/Playback", $"Falha ao iniciar reprodução: {ex.GetType().Name}");
                if (!_usingLegacyPlayer)
                {
                    await SwitchToLegacyAndPlayAsync();
                    return;
                }

                ShowPlaybackError();
            }
        }

        private async Task StopPlaybackAsync()
        {
            if (_playbackEngine is not null)
            {
                await _playbackEngine.StopAsync(_playbackLifecycle?.Token ?? CancellationToken.None);
            }
        }

        private async Task SwitchToLegacyAndPlayAsync()
        {
            var cancellationToken = _playbackLifecycle?.Token ?? CancellationToken.None;
            var lockAcquired = false;
            try
            {
                await _fallbackLock.WaitAsync(cancellationToken);
                lockAcquired = true;
                if (_usingLegacyPlayer || _released || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (_playbackEngine is not null)
                {
                    _playbackEngine.StateChanged -= OnPlaybackStateChanged;
                }

                (_playbackEngine as IDisposable)?.Dispose();
                cancellationToken.ThrowIfCancellationRequested();
                _playbackEngine = DecoratePlaybackEngine(new LegacyMediaPlayerEngine());
                _usingLegacyPlayer = true;
                _playbackEngine.StateChanged += OnPlaybackStateChanged;
                await _playbackEngine.PlayAsync(
                    new PlaybackRequest(_videoUrl, _channelName),
                    cancellationToken);
            }
            catch (System.OperationCanceledException) when (_released)
            {
            }
            finally
            {
                if (lockAcquired)
                {
                    _fallbackLock.Release();
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            InitializePlaybackEngine();
        }

        protected override void OnPause()
        {
            SavePlaybackProgress();

            ReleasePlaybackEngine();

            base.OnPause();
        }

        protected override void OnStop()
        {
            ReleasePlaybackEngine();
            base.OnStop();
        }

        private void OnPlaybackStateChanged(object? sender, PlaybackStateChangedEventArgs args)
        {
            RunOnUiThread(async () =>
            {
                SetProgressVisible(args.State == PlaybackState.Buffering);
                if (args.State == PlaybackState.Error && !_usingLegacyPlayer)
                {
                    await SwitchToLegacyAndPlayAsync();
                    return;
                }

                if (args.State == PlaybackState.Completed)
                {
                    Toast.MakeText(this, "Stream finalizado.", ToastLength.Short)?.Show();
                }
                else if (args.State == PlaybackState.Error)
                {
                    Android.Util.Log.Warn("Hiptv/Playback", $"Erro técnico: {args.Message ?? "sem detalhes"}");
                    ShowPlaybackError();
                }
            });
        }

        protected override void OnDestroy()
        {
            ReleasePlaybackEngine();
            base.OnDestroy();
        }
        private void SavePlaybackProgress()
        {
            if (_playbackEngine is null)
            {
                return;
            }

            var duration =
                _playbackEngine.Duration;

            if (duration is null)
            {
                return;
            }

            var position =
                _playbackEngine.Position;

            if (position < TimeSpan.FromSeconds(30))
            {
                return;
            }

            var progress =
            new PlaybackProgress(
                new MediaId(_videoUrl),
                position,
                duration,
                DateTimeOffset.UtcNow);

            var history =
              _playbackProgressService?
                .GetHistory()
                .ToList()
                ?? new List<PlaybackProgress>();

            history.RemoveAll(
                item => item.MediaId.ToString() == _videoUrl);

            history.Add(progress);

            _playbackProgressService?
             .SaveHistory(history);
        }
        private void ReleasePlaybackEngine()
        {
            if (_released)
            {
                return;
            }

            _released = true;
            _playbackLifecycle?.Cancel();
            if (_playbackEngine is null)
            {
                return;
            }

            _playbackEngine.StateChanged -= OnPlaybackStateChanged;
            (_playbackEngine as IDisposable)?.Dispose();
            _playbackEngine = null;
            _playbackLifecycle?.Dispose();
            _playbackLifecycle = null;
        }

        private void SetProgressVisible(bool visible)
        {
            if (_progressBar is not null)
            {
                _progressBar.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        private void ShowPlaybackError()
        {
            SetProgressVisible(false);
            Toast.MakeText(this, "Não foi possível reproduzir o stream. Verifique a conexão e tente novamente.", ToastLength.Long)?.Show();
        }
    }
}
