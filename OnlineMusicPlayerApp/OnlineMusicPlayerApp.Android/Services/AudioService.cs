using Android.Content;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioService))]
public class AudioService : IAudioService
{
    public void Play(string url)
    {
        var intent = new Intent(Android.App.Application.Context, typeof(MusicService));
        intent.PutExtra("url", url);
        Android.App.Application.Context.StartService(intent);
    }

    public void Stop()
    {
        var intent = new Intent(Android.App.Application.Context, typeof(MusicService));
        Android.App.Application.Context.StopService(intent);
    }
}
