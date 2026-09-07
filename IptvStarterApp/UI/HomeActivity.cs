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
    public class HomeActivity : Activity
    {
        private readonly MediaCatalogService _catalogService = new();
        private readonly Dictionary<string, List<ChannelItem>> _catalog = new();

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_home);

            var categories = new[]
            {
                MediaCategory.Live,
                MediaCategory.Movies,
                MediaCategory.Series,
                MediaCategory.Favorites
            };

            var categoryList = FindViewById<ListView>(Resource.Id.categoryList);
            var title = FindViewById<TextView>(Resource.Id.homeTitle);
            title.Text = AppConfig.AppName;

            var groupedCatalog = _catalogService.GetGroupedCatalog();
            foreach (var key in categories)
            {
                if (groupedCatalog.ContainsKey(key))
                {
                    _catalog[key] = groupedCatalog[key].ToList();
                }
            }

            categoryList.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, categories);

            categoryList.ItemClick += (_, args) =>
            {
                var selectedCategory = categories[args.Position];
                var items = _catalog.TryGetValue(selectedCategory, out var list) ? list : new List<ChannelItem>();

                var intent = new Intent(this, typeof(MediaListActivity));
                intent.PutExtra("category_name", selectedCategory);
                intent.PutStringArrayListExtra("channel_names", items.Select(x => x.Name).ToList());
                intent.PutStringArrayListExtra("channel_urls", items.Select(x => x.Url).ToList());
                StartActivity(intent);
            };
        }
    }
}
