using Android.Content;
using IptvStarterApp.Models;

namespace IptvStarterApp.Services
{
    public class FavoritesService
    {
        private const string PrefName = "HiptvFavorites";
        private const string Key = "favorite_urls";

        private readonly Context _context;

        public FavoritesService(Context context)
        {
            _context = context;
        }

        public HashSet<string> LoadFavorites()
        {
            var prefs = _context.GetSharedPreferences(PrefName, FileCreationMode.Private);
            var raw = prefs.GetString(Key, string.Empty);

            if (string.IsNullOrWhiteSpace(raw))
            {
                return new HashSet<string>();
            }

            return new HashSet<string>(raw.Split('|', StringSplitOptions.RemoveEmptyEntries));
        }

        public void ToggleFavorite(ChannelItem channel)
        {
            var favorites = LoadFavorites();
            var key = BuildKey(channel);

            if (favorites.Contains(key))
            {
                favorites.Remove(key);
            }
            else
            {
                favorites.Add(key);
            }

            SaveFavorites(favorites);
        }

        public bool IsFavorite(ChannelItem channel)
        {
            return LoadFavorites().Contains(BuildKey(channel));
        }

        private void SaveFavorites(HashSet<string> favorites)
        {
            var prefs = _context.GetSharedPreferences(PrefName, FileCreationMode.Private);
            var editor = prefs.Edit();
            editor.PutString(Key, string.Join("|", favorites));
            editor.Apply();
        }

        private static string BuildKey(ChannelItem channel)
        {
            return $"{channel.Name}|{channel.Url}";
        }
    }
}
