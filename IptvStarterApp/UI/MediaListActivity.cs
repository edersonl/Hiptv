using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace IptvStarterApp.UI
{
    [Activity(Label = "Hiptv Catalog")]
    public class MediaListActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_media_list);

            var categoryName = Intent.GetStringExtra("category_name") ?? "Conteúdo";
            var title = FindViewById<TextView>(Resource.Id.mediaTitle);
            title.Text = categoryName;

            var names = Intent.GetStringArrayListExtra("channel_names") ?? new Java.Util.ArrayList();
            var urls = Intent.GetStringArrayListExtra("channel_urls") ?? new Java.Util.ArrayList();

            var listView = FindViewById<ListView>(Resource.Id.mediaList);
            var items = new List<string>();

            for (var i = 0; i < names.Count; i++)
            {
                items.Add(names[i].ToString());
            }

            listView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, items);

            listView.ItemClick += (_, args) =>
            {
                var url = urls.Count > args.Position ? urls[args.Position]?.ToString() : string.Empty;
                var intent = new Intent(this, typeof(PlayerActivity));
                intent.PutExtra("channel_name", items[args.Position]);
                intent.PutExtra("channel_url", url);
                StartActivity(intent);
            };
        }
    }
}
