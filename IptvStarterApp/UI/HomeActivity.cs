using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using IptvStarterApp.Config;
using IptvStarterApp.Models;
using IptvStarterApp.Services;
using System.Linq;

namespace IptvStarterApp.UI
{
    [Activity(Label = "Hiptv")]
    public class HomeActivity : Activity
    {
        private readonly PlaylistService _playlistService = new();

        private ChannelStoreService? _channelStore;
        private FavoritesService? _favoritesService;
        private PlaybackProgressService? _playbackProgressService;

        private EditText? _playlistUrl;
        private Button? _connectButton;
        private TextView? _status;

        private LinearLayout? _recentContainer;
        private LinearLayout? _favoritesContainer;
        private LinearLayout? _categoriesContainer;
        private LinearLayout? _continueWatchingContainer;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_home);

            _channelStore = new ChannelStoreService(this);
            _favoritesService = new FavoritesService(this);
            _playbackProgressService = new PlaybackProgressService(this);

            _playlistUrl = FindViewById<EditText>(Resource.Id.playlistUrlInput);
            _connectButton = FindViewById<Button>(Resource.Id.connectPlaylistButton);
            _status = FindViewById<TextView>(Resource.Id.homeStatus);

            _recentContainer = FindViewById<LinearLayout>(Resource.Id.recentContainer);
            _favoritesContainer = FindViewById<LinearLayout>(Resource.Id.favoritesContainer);
            _categoriesContainer = FindViewById<LinearLayout>(Resource.Id.categoriesContainer);
            _continueWatchingContainer = FindViewById<LinearLayout>(Resource.Id.continueWatchingContainer);

            if (_playlistUrl is not null)
            {
                _playlistUrl.Text = _channelStore.LastPlaylistUrl;
            }

            if (_connectButton is not null)
            {
                _connectButton.Click += async (_, _) =>
                    await ConnectPlaylistAsync();
            }

            FindViewById<Button>(Resource.Id.openLiveButton)!
                .Click += (_, _) => OpenChannels();

            FindViewById<Button>(Resource.Id.openFavoritesButton)!
                .Click += (_, _) => OpenFavorites();

            FindViewById<Button>(Resource.Id.openSearchButton)!
                .Click += (_, _) =>
                    StartActivity(new Intent(this, typeof(SearchActivity)));

            RenderHome();
        }

        protected override void OnResume()
        {
            base.OnResume();
            RenderHome();
        }

        private async Task ConnectPlaylistAsync()
        {
            var url = _playlistUrl?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(url))
            {
                SetStatus("Informe a URL da sua playlist M3U.");
                return;
            }

            _connectButton!.Enabled = false;
            SetStatus("Conectando à playlist...");

            try
            {
                var channels =
                    await _playlistService.LoadAsync(url);

                _channelStore!.SaveChannels(channels, url);

                SetStatus($"{channels.Count} canais disponíveis.");

                RenderHome();
            }
            catch (Exception exception)
            {
                Android.Util.Log.Warn(
                    "Hiptv/Home",
                    exception.GetType().Name);

                SetStatus(
                    "Não foi possível carregar a playlist. Verifique a URL e a conexão.");
            }
            finally
            {
                _connectButton.Enabled = true;
            }
        }

        private void RenderHome()
        {
            if (_channelStore is null ||
                _favoritesService is null)
            {
                return;
            }

            RenderContinueWatching();

            RenderChannelButtons(
                _recentContainer,
                _channelStore.GetRecentChannels(),
                "Nenhum canal recente");

            RenderChannelButtons(
                _favoritesContainer,
                _favoritesService.GetFavorites(),
                "Seus favoritos aparecerão aqui");

            if (_categoriesContainer is null)
            {
                return;
            }

            _categoriesContainer.RemoveAllViews();

            var groups =
                _channelStore.GetChannels()
                    .Select(channel => channel.Group)
                    .Where(group => !string.IsNullOrWhiteSpace(group))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(group => group)
                    .Take(12)
                    .ToArray();

            if (groups.Length == 0)
            {
                _categoriesContainer.AddView(
                    CreateEmptyText(
                        "Conecte uma playlist para ver categorias"));

                return;
            }

            foreach (var group in groups)
            {
                var button =
                    CreateChannelButton(group);

                button.Click += (_, _) =>
                    OpenChannels(group);

                _categoriesContainer.AddView(button);
            }
        }

        private void RenderContinueWatching()
        {
            if (_continueWatchingContainer is null ||
                _playbackProgressService is null)
    {
        return;
    }

    _continueWatchingContainer.RemoveAllViews();

    var items =
        _playbackProgressService
            .GetHistory()
            .OrderByDescending(x => x.LastPlayedAt)
            .Take(8)
            .ToArray();

    if (items.Length == 0)
    {
        _continueWatchingContainer.AddView(
            CreateEmptyText(
                "Nenhum conteúdo em andamento"));

        return;
    }

    foreach (var item in items)
    {
        var button =
            CreateChannelButton(
                item.MediaId.ToString());

        _continueWatchingContainer.AddView(button);
    }
}

private void RenderChannelButtons(
    LinearLayout? container,
    IEnumerable<ChannelItem> channels,
    string emptyText)
{
    if (container is null) return;

    container.RemoveAllViews();

    var items = channels.Take(8).ToArray();

    if (items.Length == 0)
    {
        container.AddView(CreateEmptyText(emptyText));
        return;
    }

    foreach (var channel in items)
    {
        var button = CreateChannelButton(channel.Name);
        button.Click += (_, _) => OpenPlayer(channel);
        container.AddView(button);
    }
}

private Button CreateChannelButton(string text)
{
    var button = new Button(this)
    {
        Text = text
    };

    button.SetMinWidth(180);
    button.SetMinHeight(64);

    button.LayoutParameters =
        new LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.WrapContent,
            LinearLayout.LayoutParams.WrapContent)
        {
            RightMargin = 12
        };

    return button;
}

private TextView CreateEmptyText(string text)
{
    var view = new TextView(this)
    {
        Text = text,
        TextSize = 15
    };

    view.SetTextColor(
        Android.Graphics.Color.ParseColor("#9CA3AF"));

    return view;
}

private void OpenChannels(string? group = null)
{
    var intent = new Intent(this, typeof(MediaListActivity));

    if (!string.IsNullOrWhiteSpace(group))
    {
        intent.PutExtra("selected_group", group);
    }

    StartActivity(intent);
}

private void OpenFavorites()
{
    var intent = new Intent(this, typeof(MediaListActivity));

    intent.PutExtra("favorites_only", true);

    StartActivity(intent);
}

private void OpenPlayer(ChannelItem channel)
{
    _channelStore?.AddRecent(channel);

    var intent = new Intent(this, typeof(PlayerActivity));

    intent.PutExtra("channel_name", channel.Name);
    intent.PutExtra("channel_url", channel.Url);
    intent.PutExtra("channel_group", channel.Group);

    StartActivity(intent);
}

private void SetStatus(string message)
{
    if (_status is not null)
    {
        _status.Text = message;
    }
}
    }
}