using Android.Content;
using IptvStarterApp.Models;
using System.Text.Json;

namespace IptvStarterApp.Services
{
    public class FavoritesService
    {
        private const string PrefName = "HiptvFavorites";
        private const string Key = "favorite_items_v2";

        private readonly Context _context;

        public FavoritesService(Context context)
        {
            _context = context;
        }

        public IReadOnlyList<ChannelItem> GetFavorites()
        {
            var prefs = _context.GetSharedPreferences(PrefName, FileCreationMode.Private);
            var raw = prefs?.GetString(Key, string.Empty);

            if (string.IsNullOrWhiteSpace(raw))
            {
                return Array.Empty<ChannelItem>();
            }

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

        public void ToggleFavorite(ChannelItem channel)
        {
            ArgumentNullException.ThrowIfNull(channel);
            var favorites = GetFavorites().ToList();
            var existing = favorites.FindIndex(item =>
                string.Equals(item.Url, channel.Url, StringComparison.OrdinalIgnoreCase));

            if (existing >= 0)
            {
                favorites.RemoveAt(existing);
            }
            else
            {
                favorites.Insert(0, channel);
            }

            SaveFavorites(favorites);
        }

        public bool IsFavorite(ChannelItem channel)
        {
            return GetFavorites().Any(item =>
                string.Equals(item.Url, channel.Url, StringComparison.OrdinalIgnoreCase));
        }

        private void SaveFavorites(IEnumerable<ChannelItem> favorites)
        {
            var prefs = _context.GetSharedPreferences(PrefName, FileCreationMode.Private);
            var editor = prefs?.Edit();
            if (editor is null)
            {
                return;
            }

            editor.PutString(Key, JsonSerializer.Serialize(favorites.Select(StoredChannel.FromChannel)));
            editor.Apply();
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
}
