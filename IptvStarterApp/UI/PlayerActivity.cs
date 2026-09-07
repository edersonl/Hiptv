using Android.App;
using Android.Media;
using Android.OS;
using Android.Widget;
using Android.Views;
using IptvStarterApp.Models;
using IptvStarterApp.Services;

namespace IptvStarterApp.UI
{
    [Activity(Label = "Player")]
    public class PlayerActivity : Activity, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnErrorListener
    {
        private MediaPlayer? _mediaPlayer;
        private TextView? _titleView;
        private ProgressBar? _progressBar;
        private Button? _favoriteButton;
        private string _videoUrl = string.Empty;
        private string _channelName = string.Empty;
        private readonly FavoritesService? _favoritesService;

        public PlayerActivity()
        {
            _favoritesService = null;
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_player);

            _titleView = FindViewById<TextView>(Resource.Id.playerTitle);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.playerProgress);
            _favoriteButton = FindViewById<Button>(Resource.Id.favoriteButton);

            _channelName = Intent.GetStringExtra("channel_name") ?? "Canal IPTV";
            _videoUrl = Intent.GetStringExtra("channel_url") ?? string.Empty;

            _titleView.Text = _channelName;

            var playButton = FindViewById<Button>(Resource.Id.playButton);
            playButton.Click += (_, _) => StartPlayback();

            var stopButton = FindViewById<Button>(Resource.Id.stopButton);
            stopButton.Click += (_, _) => StopPlayback();

            _favoriteButton.Click += (_, _) =>
            {
                var channel = new ChannelItem { Name = _channelName, Url = _videoUrl };
                var service = new FavoritesService(this);
                service.ToggleFavorite(channel);
                var isFavorite = service.IsFavorite(channel);
                _favoriteButton.Text = isFavorite ? "Remover Favorito" : "Adicionar Favorito";
                Toast.MakeText(this, isFavorite ? "Adicionado aos favoritos." : "Removido dos favoritos.", ToastLength.Short)?.Show();
            };
        }

        private void StartPlayback()
        {
            if (string.IsNullOrWhiteSpace(_videoUrl))
            {
                Toast.MakeText(this, "URL do canal não disponível.", ToastLength.Short)?.Show();
                return;
            }

            try
            {
                _progressBar.Visibility = ViewStates.Visible;
                _mediaPlayer?.Release();

                _mediaPlayer = new MediaPlayer();
                _mediaPlayer.SetAudioStreamType(Stream.Music);
                _mediaPlayer.SetOnCompletionListener(this);
                _mediaPlayer.SetOnErrorListener(this);

                _mediaPlayer.SetDataSource(_videoUrl);
                _mediaPlayer.PrepareAsync();
                _mediaPlayer.SetOnPreparedListener(new MediaPlayerPreparedListener(this));
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Erro ao iniciar stream: {ex.Message}", ToastLength.Long)?.Show();
            }
        }

        private void StopPlayback()
        {
            _mediaPlayer?.Stop();
            _mediaPlayer?.Release();
            _mediaPlayer = null;
            _progressBar.Visibility = ViewStates.Gone;
        }

        public void OnCompletion(MediaPlayer? mp)
        {
            _progressBar.Visibility = ViewStates.Gone;
            Toast.MakeText(this, "Stream finalizado.", ToastLength.Short)?.Show();
        }

        public bool OnError(MediaPlayer? mp, MediaError what, int extra)
        {
            _progressBar.Visibility = ViewStates.Gone;
            Toast.MakeText(this, "Erro no stream. Verifique a URL e a conexão.", ToastLength.Long)?.Show();
            return true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _mediaPlayer?.Release();
            _mediaPlayer = null;
        }

        private sealed class MediaPlayerPreparedListener : Java.Lang.Object, MediaPlayer.IOnPreparedListener
        {
            private readonly PlayerActivity _activity;

            public MediaPlayerPreparedListener(PlayerActivity activity)
            {
                _activity = activity;
            }

            public void OnPrepared(MediaPlayer? mp)
            {
                if (mp == null)
                {
                    return;
                }

                mp.Start();
                _activity._progressBar.Visibility = ViewStates.Gone;
            }
        }
    }
}
