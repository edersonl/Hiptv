using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using IptvStarterApp.Models;
using IptvStarterApp.Services;

namespace IptvStarterApp.UI
{
    [Activity(Label = "Hiptv Catalog")]
    public class MediaListActivity : Activity
    {
        private readonly List<ChannelItem> _visibleChannels = new();
        private IReadOnlyList<ChannelItem> _allChannels = Array.Empty<ChannelItem>();
        private ChannelStoreService? _channelStore;
        private ListView? _listView;
        private Spinner? _categoryFilter;
        private TextView? _previewName;
        private TextView? _previewInfo;
        private Button? _watchButton;
        private ChannelItem? _selectedChannel;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_media_list);

            _channelStore = new ChannelStoreService(this);
            var favoritesOnly = Intent?.GetBooleanExtra("favorites_only", false) == true;
            var categoryName = favoritesOnly ? "Favoritos" : "TV ao vivo";
            var title = FindViewById<TextView>(Resource.Id.mediaTitle);
            if (title is not null)
            {
                title.Text = categoryName;
            }

            _allChannels = favoritesOnly
                ? new FavoritesService(this).GetFavorites()
                : _channelStore.GetChannels();
            _listView = FindViewById<ListView>(Resource.Id.mediaList);
            _categoryFilter = FindViewById<Spinner>(Resource.Id.categoryFilter);
            _previewName = FindViewById<TextView>(Resource.Id.previewName);
            _previewInfo = FindViewById<TextView>(Resource.Id.previewInfo);
            _watchButton = FindViewById<Button>(Resource.Id.watchPreviewButton);

            var groups = _allChannels.Select(channel => channel.Group)
                .Where(group => !string.IsNullOrWhiteSpace(group))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(group => group).ToList();
            groups.Insert(0, "Todos os canais");
            _categoryFilter!.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, groups);
            _categoryFilter.ItemSelected += (_, args) => ApplyFilter(args.Position == 0 ? null : groups[args.Position]);

            var requestedGroup = Intent?.GetStringExtra("selected_group");
            var requestedIndex = groups.FindIndex(group => string.Equals(group, requestedGroup, StringComparison.OrdinalIgnoreCase));
            if (requestedIndex >= 0) _categoryFilter.SetSelection(requestedIndex);
            else ApplyFilter(null);

            _listView!.ItemClick += (_, args) => SelectChannel(_visibleChannels[args.Position]);
            _watchButton!.Click += (_, _) =>
            {
                if (_selectedChannel is not null) OpenPlayer(_selectedChannel);
            };
        }

        private void ApplyFilter(string? group)
        {
            _visibleChannels.Clear();
            _visibleChannels.AddRange(string.IsNullOrWhiteSpace(group)
                ? _allChannels
                : _allChannels.Where(channel => string.Equals(channel.Group, group, StringComparison.OrdinalIgnoreCase)));
            _listView!.Adapter = new ArrayAdapter<string>(
                this, Android.Resource.Layout.SimpleListItemActivated1, _visibleChannels.Select(channel => channel.Name).ToList());
            if (_visibleChannels.Count > 0) SelectChannel(_visibleChannels[0]);
            else ShowEmptyPreview();
        }

        private void SelectChannel(ChannelItem channel)
        {
            _selectedChannel = channel;
            _previewName!.Text = channel.Name;
            _previewInfo!.Text = $"{channel.Group}  •  Toque em Assistir para abrir o player";
            _watchButton!.Enabled = true;
        }

        private void ShowEmptyPreview()
        {
            _selectedChannel = null;
            _previewName!.Text = "Nenhum canal encontrado";
            _previewInfo!.Text = "Conecte uma playlist ou escolha outra categoria.";
            _watchButton!.Enabled = false;
        }

        private void OpenPlayer(ChannelItem channel)
        {
            _channelStore?.AddRecent(channel);
            var currentIndex = _visibleChannels.IndexOf(channel);
            var next = _visibleChannels.Count > 1 ? _visibleChannels[(currentIndex + 1) % _visibleChannels.Count] : null;
            var intent = new Intent(this, typeof(PlayerActivity));
            intent.PutExtra("channel_name", channel.Name);
            intent.PutExtra("channel_url", channel.Url);
            intent.PutExtra("channel_group", channel.Group);
            if (next is not null)
            {
                intent.PutExtra("next_channel_name", next.Name);
                intent.PutExtra("next_channel_url", next.Url);
                intent.PutExtra("next_channel_group", next.Group);
            }
            StartActivity(intent);
        }
    }
}
