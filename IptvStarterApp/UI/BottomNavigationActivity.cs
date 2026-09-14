using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using IptvStarterApp.Config;
using IptvStarterApp.Models;
using IptvStarterApp.Services;

namespace IptvStarterApp.UI
{
    [Activity(Label = "Hiptv")]
    public class BottomNavigationActivity : Activity
    {
        private readonly MediaCatalogService _catalogService = new();

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_bottom_navigation);

            var title = FindViewById<TextView>(Resource.Id.navTitle);
            if (title is not null)
            {
                title.Text = AppConfig.AppName;
            }

            var liveButton = FindViewById<Button>(Resource.Id.navLive);
            var moviesButton = FindViewById<Button>(Resource.Id.navMovies);
            var seriesButton = FindViewById<Button>(Resource.Id.navSeries);
            var favoritesButton = FindViewById<Button>(Resource.Id.navFavorites);

            if (liveButton is not null) liveButton.Click += (_, _) => OpenCategory(MediaCategory.Live);
            if (moviesButton is not null) moviesButton.Click += (_, _) => OpenCategory(MediaCategory.Movies);
            if (seriesButton is not null) seriesButton.Click += (_, _) => OpenCategory(MediaCategory.Series);
            if (favoritesButton is not null) favoritesButton.Click += (_, _) => OpenCategory(MediaCategory.Favorites);
        }

        private void OpenCategory(string category)
        {
            var grouped = _catalogService.GetGroupedCatalog();
            var items = grouped.TryGetValue(category, out var list) ? list : new List<ChannelItem>();

            var intent = new Intent(this, typeof(MediaListActivity));
            intent.PutExtra("category_name", category);
            intent.PutStringArrayListExtra("channel_names", items.Select(x => x.Name).ToList());
            intent.PutStringArrayListExtra("channel_urls", items.Select(x => x.Url).ToList());
            StartActivity(intent);
        }
    }
}
