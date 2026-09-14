using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Widget;
using IptvStarterApp.Models;
using IptvStarterApp.Services;

namespace IptvStarterApp.UI;

[Activity(Label = "Buscar")]
public sealed class SearchActivity : Activity
{
    private IReadOnlyList<ChannelItem> _channels = Array.Empty<ChannelItem>();
    private readonly List<ChannelItem> _results = new();
    private ListView? _resultsView;
    private ChannelStoreService? _channelStore;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_search);

        _channelStore = new ChannelStoreService(this);
        _channels = _channelStore.GetChannels();
        _resultsView = FindViewById<ListView>(Resource.Id.searchResults);
        var input = FindViewById<EditText>(Resource.Id.searchInput)!;
        input.AddTextChangedListener(new SearchTextWatcher(query => Search(query)));
        _resultsView!.ItemClick += (_, args) => OpenPlayer(_results[args.Position]);
        Search(string.Empty);
    }

    private void Search(string query)
    {
        _results.Clear();
        if (!string.IsNullOrWhiteSpace(query))
        {
            _results.AddRange(_channels.Where(channel =>
                channel.Name.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase)));
        }

        _resultsView!.Adapter = new ArrayAdapter<string>(
            this, Android.Resource.Layout.SimpleListItem1, _results.Select(channel => $"{channel.Name}  ·  {channel.Group}").ToList());
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

    private sealed class SearchTextWatcher(Action<string> changed) : Java.Lang.Object, ITextWatcher
    {
        public void AfterTextChanged(Android.Text.IEditable? editable) => changed(editable?.ToString() ?? string.Empty);
        public void BeforeTextChanged(Java.Lang.ICharSequence? text, int start, int count, int after) { }
        public void OnTextChanged(Java.Lang.ICharSequence? text, int start, int before, int count) { }
    }
}