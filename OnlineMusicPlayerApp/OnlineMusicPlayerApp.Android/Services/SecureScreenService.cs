using Android.Views;
using OnlineMusicPlayerApp.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(SecureScreenService))]
public class SecureScreenService : ISecureScreen
{
    public void EnableSecure()
    {
        var activity = Xamarin.Essentials.Platform.CurrentActivity;
        activity?.Window?.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
    }

    public void DisableSecure()
    {
        var activity = Xamarin.Essentials.Platform.CurrentActivity;
        activity?.Window?.ClearFlags(WindowManagerFlags.Secure);
    }
}
