using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Com.Google.Android.Exoplayer2;

[Service]
public class MusicService : Service
{
    private SimpleExoPlayer player;

    public override void OnCreate()
    {
        base.OnCreate();
        player = new SimpleExoPlayer.Builder(this).Build();
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        string url = intent.GetStringExtra("url");
        var mediaItem = MediaItem.FromUri(url);
        player.SetMediaItem(mediaItem);
        player.Prepare();
        player.Play();

        //var notification = new Notification.Builder(this)
        //    .SetContentTitle("در حال پخش موزیک")
        //    .SetContentText("نغمه‌ای که حتی در سکوت ادامه دارد")
        //    .SetSmallIcon(Resource.Drawable.IcMediaPause)
        //    .Build();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel("music_channel", "Music Playback", NotificationImportance.Default)
            {
                Description = "نغمه‌ای که در پس‌زمینه ادامه دارد"
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
        Notification notification = new Notification.Builder(this, "music_channel")
            .SetContentTitle("در حال پخش موزیک")
            .SetContentText("نغمه‌ای که حتی در سکوت ادامه دارد")
            .SetSmallIcon(Resource.Drawable.IcMediaPlay)
            .Build();

        StartForeground(1, notification);

        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        player?.Release();
        player = null;
    }

    public override IBinder OnBind(Intent intent) => null;
}
