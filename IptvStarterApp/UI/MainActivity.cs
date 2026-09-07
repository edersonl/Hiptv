using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using IptvStarterApp.Config;
using IptvStarterApp.Models;
using IptvStarterApp.Services;
using System.Net;

namespace IptvStarterApp.UI
{
    [Activity(Label = "Hiptv", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private readonly string _defaultPlaylistUrl = AppConfig.DefaultPlaylistUrl;

        private EditText? _urlText;
        private Button? _loadButton;
        private TextView? _statusText;
        private ListView? _channelList;
        private ArrayAdapter<string>? _adapter;
        private readonly List<ChannelItem> _channels = new();
        private readonly PlaylistService _playlistService = new();

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _urlText = FindViewById<EditText>(Resource.Id.playlistUrl);
            _loadButton = FindViewById<Button>(Resource.Id.loadPlaylistButton);
            _statusText = FindViewById<TextView>(Resource.Id.statusText);
            _channelList = FindViewById<ListView>(Resource.Id.channelList);

            _urlText.Text = _defaultPlaylistUrl;

            _loadButton.Click += async (_, _) => await LoadChannelsAsync();

            _channelList.ItemClick += (_, e) =>
            {
                var channel = _channels[e.Position];
                var intent = new Intent(this, typeof(PlayerActivity));
                intent.PutExtra("channel_name", channel.Name);
                intent.PutExtra("channel_url", channel.Url);
                StartActivity(intent);
            };

            _statusText.Text = $"{AppConfig.WarningMessage}\n\nLinks:\n{AppConfig.DownloadUrl}\n{AppConfig.DownloadUrlAlt}\n\nLite:\n{AppConfig.LiteApkUrl}\n{AppConfig.LiteApkUrlAlt}\n\nVPN:\n{AppConfig.ProtonVpnUrl}\n{AppConfig.CloudflareWarpUrl}\n\n{AppConfig.AppInfoMessage}";
        }

        private async Task LoadChannelsAsync()
        {
            if (_urlText == null || _loadButton == null || _statusText == null || _channelList == null)
            {
                return;
            }

            var url = _urlText.Text?.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                _statusText.Text = "Informe uma URL de playlist M3U válida.";
                return;
            }

            _loadButton.Enabled = false;
            _statusText.Text = "Carregando canais...";

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var channels = await _playlistService.LoadAsync(url);
                _channels.Clear();
                _channels.AddRange(channels);

                var names = _channels.Select(x => x.Name).ToList();
                _adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, names);
                _channelList.Adapter = _adapter;

                _statusText.Text = channels.Count > 0
                    ? "Lista carregada. Use VPN antes do acesso para evitar bloqueios e preservar privacidade."
                    : "Nenhum canal foi encontrado na playlist.";
            }
            catch (Exception ex)
            {
                _statusText.Text = $"Erro ao carregar a playlist: {ex.Message}";
            }
            finally
            {
                _loadButton.Enabled = true;
            }
        }
    }
}
