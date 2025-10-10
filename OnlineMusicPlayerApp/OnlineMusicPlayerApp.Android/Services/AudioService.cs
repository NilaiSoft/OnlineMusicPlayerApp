using Android.Content;
using Com.Google.Android.Exoplayer2;
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

    public bool IsPlaying()
    {
        return MusicService.player.IsPlaying;
    }

    public void Pause()
    {
        MusicService.player.Pause();
    }

    public void Resume()
    {
        MusicService.player.Play();
    }
}
