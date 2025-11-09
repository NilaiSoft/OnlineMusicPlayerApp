using Android.Content;
using Com.Google.Android.Exoplayer2;
using OnlineMusicPlayerApp.Droid.Services;
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
        if (MusicService.player == null)
            return false;

        return MusicService.player.IsPlaying;
    }

    public void Pause()
    {
        if (MusicService.player == null)
            return;

        MusicService.player.Pause();
    }

    public void Resume()
    {
        if (MusicService.player == null)
            return;

        MusicService.player.Play();
    }

    public double GetDurationSeconds()
    {
        return MusicService.player?.Duration / 1000.0 ?? 0;
    }

    public double GetCurrentPositionSeconds()
    {
        return MusicService.player?.CurrentPosition / 1000.0 ?? 0;
    }

    public void SeekTo(long positionMs)
    {
        MusicService.player?.SeekTo(positionMs);
    }

    public void Close()
    {
        var intent = new Intent(Android.App.Application.Context, typeof(MusicService));
        Android.App.Application.Context.StopService(intent);
        Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
    }

    public void Play(string url, string title, string TagImageSrc, double startSeconds)
    {
        var intent = new Intent(Android.App.Application.Context, typeof(MusicService));
        intent.PutExtra("url", url);
        intent.PutExtra("position", startSeconds.ToString()); // ذخیره به‌صورت string

        intent.PutExtra("url", url);
        intent.PutExtra("title", title);        // 🎵 عنوان آهنگ
        intent.PutExtra("albumArtPath", TagImageSrc); // 🖼 مسیر عکس تگ (می‌تونه FilePath باشه)

        Android.App.Application.Context.StartService(intent);
    }

    public bool IsInitialized()
    {
        return MusicService.player != null;
    }
}
