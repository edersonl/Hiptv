using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using IptvStarterApp.Config;

namespace IptvStarterApp.Platforms.AndroidTV
{
    [Activity(Label = "IPTV Smart TV", Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class AndroidTVMainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var layout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };
            layout.SetPadding(32, 32, 32, 32);

            var title = new TextView(this)
            {
                Text = AppConfig.AppName,
                TextSize = 28,
                Gravity = GravityFlags.CenterHorizontal
            };

            var warning = new TextView(this)
            {
                Text = AppConfig.WarningMessage,
                TextSize = 18,
                Gravity = GravityFlags.CenterHorizontal
            };
            warning.SetPadding(20, 20, 20, 20);

            var btn = new Button(this)
            {
                Text = "Abrir player IPTV"
            };

            btn.Click += (_, _) =>
            {
                Toast.MakeText(this, "Use a playlist real em AppConfig.DefaultPlaylistUrl.", ToastLength.Long)?.Show();
            };

            layout.AddView(title);
            layout.AddView(warning);
            layout.AddView(btn);

            SetContentView(layout);
        }
    }
}
