using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Views;
using Xamarin.Forms;
using Android.Content;
using Android.Media;

namespace OnlineMusicPlayerApp.Droid
{
    [Activity(Label = "NetAudio", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize,
        WindowSoftInputMode = SoftInput.AdjustResize, Exported = true)]

    [Obsolete]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private HeadphoneReceiver _receiver;

        protected override void OnResume()
        {
            base.OnResume();
            SetTransparentStatusBar();
            _receiver = new HeadphoneReceiver();
            RegisterReceiver(_receiver, new IntentFilter(AudioManager.ActionAudioBecomingNoisy));
        }

        protected override void OnPause()
        {
            base.OnPause();

            if (_receiver != null)
            {
                UnregisterReceiver(_receiver);
                _receiver = null;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Forms.SetFlags("MediaElement_Experimental");
            base.OnCreate(savedInstanceState);

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                    SystemUiFlags.LayoutStable |
                    SystemUiFlags.LayoutFullscreen |
                    SystemUiFlags.LightStatusBar
                );

                Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
                Window.SetNavigationBarColor(Android.Graphics.Color.Transparent);
            }

            // 🔥 درخواست مجوز نوتیفیکیشن برای Android 13+
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                if (CheckSelfPermission(Android.Manifest.Permission.PostNotifications) != Permission.Granted)
                {
                    RequestPermissions(new string[] { Android.Manifest.Permission.PostNotifications }, 1001);
                }
            }

            Rg.Plugins.Popup.Popup.Init(this);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            SetTransparentStatusBar();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#222831"));
            Window.DecorView.SystemUiVisibility = 0;

            if (requestCode == 1001)
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    Android.Util.Log.Info("NotifyPermission", "Notification permission granted.");
                }
                else
                {
                    Android.Util.Log.Warn("NotifyPermission", "Notification permission denied.");
                }
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        [Obsolete]
        public void SetTransparentStatusBar()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.AddFlags(Android.Views.WindowManagerFlags.LayoutNoLimits);
                Window.ClearFlags(Android.Views.WindowManagerFlags.LayoutNoLimits);
                Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                    Android.Views.SystemUiFlags.LayoutStable |
                    Android.Views.SystemUiFlags.LayoutFullscreen |
                    Android.Views.SystemUiFlags.LightStatusBar);
            }
        }
    }
}
