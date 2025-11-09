using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Media.App;
using Com.Google.Android.Exoplayer2;
using System;
using static Android.App.Notification;

namespace OnlineMusicPlayerApp.Droid.Services
{
    [Service(Exported = true)]
    [Obsolete]
    public class MusicService : Service
    {
        public static SimpleExoPlayer player;
        private const string CHANNEL_ID = "music_channel";
        private const int NOTIFICATION_ID = 1001;

        public override void OnCreate()
        {
            base.OnCreate();
            player = new SimpleExoPlayer.Builder(this).Build();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            string action = intent?.Action;

            // 🎯 کنترل دکمه‌های نوتیفیکیشن
            switch (action)
            {
                case "ACTION_CLOSE":
                    StopForeground(true);
                    StopSelf();
                    return StartCommandResult.NotSticky;

                case "ACTION_PAUSE":
                    player?.Pause();
                    ShowNotification("پخش متوقف شد", "برای ادامه پخش ضربه بزنید", false);
                    return StartCommandResult.Sticky;

                case "ACTION_PLAY":
                    player?.Play();
                    ShowNotification("در حال پخش موزیک", "نغمه‌ای که حتی در سکوت ادامه دارد", true);
                    return StartCommandResult.Sticky;
            }

            // 🎶 شروع پخش جدید
            string url = intent.GetStringExtra("url");
            if (!string.IsNullOrEmpty(url))
            {
                var mediaItem = MediaItem.FromUri(url);
                player.SetMediaItem(mediaItem);
                player.Prepare();
                player.Play();

                // موقعیت شروع (اختیاری)
                string posStr = intent.GetStringExtra("position");
                if (!string.IsNullOrEmpty(posStr) && double.TryParse(posStr, out var startSeconds))
                {
                    long startMs = (long)(startSeconds * 1000);
                    new Handler().PostDelayed(() =>
                    {
                        player.SeekTo(startMs);
                    }, 100);
                }
            }

            CreateNotificationChannel();
            ShowNotification("در حال پخش موزیک", "نغمه‌ای که حتی در سکوت ادامه دارد", true);

            return StartCommandResult.Sticky;
        }

        // ✅ ساخت NotificationChannel فقط یکبار
        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(CHANNEL_ID, "Music Playback", NotificationImportance.Low)
                {
                    Description = "نغمه‌ای که در پس‌زمینه ادامه دارد"
                };

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        // ✅ نمایش نوتیفیکیشن با دکمه‌های پخش و بستن
        private void ShowNotification(string title, string text, bool isPlaying)
        {
            // Intent برای پخش یا توقف
            var playIntent = new Intent(this, typeof(MusicService));
            playIntent.SetAction(isPlaying ? "ACTION_PAUSE" : "ACTION_PLAY");
            var playPendingIntent = PendingIntent.GetService(this, 1, playIntent, PendingIntentFlags.Immutable);

            // Intent برای بستن
            var closeIntent = new Intent(this, typeof(MusicService));
            closeIntent.SetAction("ACTION_CLOSE");
            var closePendingIntent = PendingIntent.GetService(this, 2, closeIntent, PendingIntentFlags.Immutable);

            // طراحی نوتیفیکیشن
            var builder = new AndroidX.Core.App.NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetSmallIcon(Resource.Drawable.IcMediaPlay)
                .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.ButtonStar))
                .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic)
                .SetShowWhen(false)
                .SetOngoing(isPlaying)
                .SetOnlyAlertOnce(true)
                .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle()
    .SetShowActionsInCompactView(0, 1))
                .AddAction(isPlaying ? Resource.Drawable.IcMediaPause : Resource.Drawable.IcMediaPlay,
                           isPlaying ? "توقف" : "پخش", playPendingIntent)
                .AddAction(Resource.Drawable.IcMenuCloseClearCancel, "بستن", closePendingIntent);

            // نمایش در Foreground
            StartForeground(NOTIFICATION_ID, builder.Build());
        }

        public bool IsPlaying() => player?.IsPlaying ?? false;

        public override void OnDestroy()
        {
            base.OnDestroy();
            player?.Release();
            player = null;
        }

        public override IBinder OnBind(Intent intent) => null;
    }
}
