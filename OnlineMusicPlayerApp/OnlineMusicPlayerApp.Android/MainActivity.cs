using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Views;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp.Droid
{
    //[Activity(Label = "قرعه کشی", Icon = "@mipmap/ehsannozari", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    [Activity(Label = "قرعه کشی", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, WindowSoftInputMode = SoftInput.AdjustResize, Exported = true)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.SetFlags("MediaElement_Experimental");
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#1B263B"));

            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#222831")); // رنگ تیره
            Window.DecorView.SystemUiVisibility = 0; // فونت روشن


            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}