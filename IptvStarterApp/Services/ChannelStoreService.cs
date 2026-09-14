using Android.Content;
using IptvStarterApp.Models;
using System.Text.Json;

namespace IptvStarterApp.Services;

public sealed class ChannelStoreService
{
    private const string PreferencesName = "HiptvCatalog";
    private const string CatalogKey = "channels";
    private const string RecentKey = "recent_channels";
    private const string PlaylistUrlKey = "playlist_url";
    private readonly ISharedPreferences? _preferences;

    public ChannelStoreService(Context context)
    {
        _preferences = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
    }

    public string LastPlaylistUrl => _preferences?.GetString(PlaylistUrlKey, string.Empty) ?? string.Empty;
    public IReadOnlyList<ChannelItem> GetChannels() => Read(CatalogKey);
    public IReadOnlyList<ChannelItem> GetRecentChannels() => Read(RecentKey);

    public void SaveChannels(IEnumerable<ChannelItem> channels, string playlistUrl)
    {
        Write(CatalogKey, channels);
        _preferences?.Edit()?.PutString(PlaylistUrlKey, playlistUrl)?.Apply();
    }

    public void AddRecent(ChannelItem channel)
    {
        var recent = GetRecentChannels()
            .Where(item => !string.Equals(item.Url, channel.Url, StringComparison.OrdinalIgnoreCase))
            .Prepend(channel)
            .Take(8);
        Write(RecentKey, recent);
    }

    private IReadOnlyList<ChannelItem> Read(string key)
    {
        var raw = _preferences?.GetString(key, string.Empty);
        if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<ChannelItem>();

        try
        {
            return (JsonSerializer.Deserialize<List<StoredChannel>>(raw) ?? new())
                .Select(item => item.ToChannel())
                .ToArray();
        }
        catch (JsonException)
        {
            return Array.Empty<ChannelItem>();
        }
    }

    private void Write(string key, IEnumerable<ChannelItem> channels)
    {
        var raw = JsonSerializer.Serialize(channels.Select(StoredChannel.FromChannel));
        _preferences?.Edit()?.PutString(key, raw)?.Apply();
    }

    private sealed record StoredChannel(string Name, string Url, string Group, string? LogoUrl)
    {
        public static StoredChannel FromChannel(ChannelItem channel) =>
            new(channel.Name, channel.Url, channel.Group, channel.TvgLogo);

        public ChannelItem ToChannel() => new()
        {
            Name = Name,
            Url = Url,
            Group = Group,
            TvgLogo = LogoUrl
        };
    }
}