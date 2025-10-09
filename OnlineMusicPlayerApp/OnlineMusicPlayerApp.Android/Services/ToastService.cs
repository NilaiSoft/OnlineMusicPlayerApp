using Android.Widget;
using OnlineMusicPlayerApp.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(ToastService))]
public class ToastService : IToastService
{
    public void Show(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        Device.BeginInvokeOnMainThread(() =>
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
        });
    }

    public void Show(string message, ToastLength toastLength)
    {
        if (string.IsNullOrEmpty(message))
            return;

        Device.BeginInvokeOnMainThread(() =>
        {
            Toast.MakeText(Android.App.Application.Context, message, toastLength).Show();
        });
    }
}