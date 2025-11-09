using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Media.App;
using Com.Google.Android.Exoplayer2;
using System;
using System.IO;

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

            // 🎵 اطلاعات آهنگ از intent
            string url = intent.GetStringExtra("url");
            string title = intent.GetStringExtra("title") ?? "در حال پخش موزیک";
            string albumArtPath = intent.GetStringExtra("albumArtPath");

            switch (action)
            {
                case "ACTION_CLOSE":
                    StopForeground(true);
                    StopSelf();
                    return StartCommandResult.NotSticky;

                case "ACTION_PAUSE":
                    player?.Pause();
                    UpdateNotification("", title, null, false);
                    return StartCommandResult.Sticky;

                case "ACTION_PLAY":
                    player?.Play();
                    UpdateNotification("", title, null, true);
                    return StartCommandResult.Sticky;
            }

            // شروع پخش
            if (!string.IsNullOrEmpty(url))
            {
                var mediaItem = MediaItem.FromUri(url);
                player.SetMediaItem(mediaItem);
                player.Prepare();
                player.Play();
            }

            // موقعیت شروع (اختیاری)
            string posStr = intent.GetStringExtra("position");
            if (!string.IsNullOrEmpty(posStr) && double.TryParse(posStr, out var startSeconds))
            {
                long startMs = (long)(startSeconds * 1000);
                new Handler().PostDelayed(() => player.SeekTo(startMs), 100);
            }

            // ایجاد کانال و نمایش نوتیف با عنوان و عکس کاور
            CreateNotificationChannel();
            UpdateNotification(title, "", albumArtPath, true);

            return StartCommandResult.Sticky;
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(CHANNEL_ID, "Music Playback", NotificationImportance.Low)
                {
                    Description = ""//"نغمه‌ای که در پس‌زمینه ادامه دارد"
                };

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        private void UpdateNotification(string title, string text, string albumArtPath, bool isPlaying)
        {
            // تصویر کاور از مسیر فایل
            Bitmap albumArt = null;
            if (!string.IsNullOrEmpty(albumArtPath) && File.Exists(albumArtPath))
            {
                albumArt = BitmapFactory.DecodeFile(albumArtPath);
            }
            else
            {
                //albumArt = BitmapFactory.DecodeResource(Resources, Resource.Drawable.default_cover);
            }

            // Intents
            var playIntent = new Intent(this, typeof(MusicService));
            playIntent.SetAction(isPlaying ? "ACTION_PAUSE" : "ACTION_PLAY");
            var playPendingIntent = PendingIntent.GetService(this, 1, playIntent, PendingIntentFlags.Immutable);

            var closeIntent = new Intent(this, typeof(MusicService));
            closeIntent.SetAction("ACTION_CLOSE");
            var closePendingIntent = PendingIntent.GetService(this, 2, closeIntent, PendingIntentFlags.Immutable);

            // نوتیفیکیشن
            var builder = new AndroidX.Core.App.NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle(title) // 🎵 عنوان آهنگ
                .SetContentText(text)
                .SetSmallIcon(Resource.Drawable.IcMediaPlay)
                .SetLargeIcon(albumArt) // 🖼 عکس کاور
                .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic)
                .SetShowWhen(false)
                .SetOngoing(isPlaying)
                .SetOnlyAlertOnce(true)
                .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle()
                    .SetShowActionsInCompactView(0, 1))
                .AddAction(isPlaying ? Resource.Drawable.IcMediaPause : Resource.Drawable.IcMediaPlay,
                           isPlaying ? "توقف" : "پخش", playPendingIntent)
                .AddAction(Resource.Drawable.IcMenuCloseClearCancel, "❌", closePendingIntent);

            StartForeground(NOTIFICATION_ID, builder.Build());
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            player?.Release();
            player = null;
        }

        public override IBinder OnBind(Intent intent) => null;
    }
}
