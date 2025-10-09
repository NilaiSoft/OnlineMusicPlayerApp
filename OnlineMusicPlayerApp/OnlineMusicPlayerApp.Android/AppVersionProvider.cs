using Android.Content.PM;
using Android.App;
using OnlineMusicPlayerApp.Droid;
using OnlineMusicPlayerApp.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(AppVersionProvider))]
namespace OnlineMusicPlayerApp.Droid
{
    public class AppVersionProvider : IAppVersionProvider
    {
        public string GetVersion()
        {
            var context = Android.App.Application.Context;
            PackageInfo info = context.PackageManager.GetPackageInfo(context.PackageName, 0);
            return info.VersionName; // مثلاً "1.0.3"
        }
    }
}
