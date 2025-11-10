using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Graphics.Drawable;
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

                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                    System.Environment.Exit(0);

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

        private void UpdateNotification(string title, string artist, string albumArtPath, bool isPlaying)
        {
            Bitmap albumArt = null;

            if (!string.IsNullOrEmpty(albumArtPath) && File.Exists(albumArtPath))
            {
                albumArt = BitmapFactory.DecodeFile(albumArtPath);
            }
            else
            {
                // عکس پیش‌فرض
                albumArt = BitmapFactory.DecodeResource(Resources, Resource.Drawable.MenuFrame);
            }

            // ✅ PendingIntents
            var prevIntent = new Intent(this, typeof(MusicService));
            prevIntent.SetAction("ACTION_PREV");
            var prevPendingIntent = PendingIntent.GetService(this, 1, prevIntent, PendingIntentFlags.Immutable);

            var playIntent = new Intent(this, typeof(MusicService));
            playIntent.SetAction(isPlaying ? "ACTION_PAUSE" : "ACTION_PLAY");
            var playPendingIntent = PendingIntent.GetService(this, 2, playIntent, PendingIntentFlags.Immutable);

            var nextIntent = new Intent(this, typeof(MusicService));
            nextIntent.SetAction("ACTION_NEXT");
            var nextPendingIntent = PendingIntent.GetService(this, 3, nextIntent, PendingIntentFlags.Immutable);

            var closeIntent = new Intent(this, typeof(MusicService));
            closeIntent.SetAction("ACTION_CLOSE");
            var closePendingIntent = PendingIntent.GetService(this, 4, closeIntent, PendingIntentFlags.Immutable);

            byte[] imageBytes = Properties.Resources.netaudioicon;
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
            var icon = IconCompat.CreateWithBitmap(bitmap);

            // ✅ نوتیف مدرن با کنترل‌های رسانه
            var builder = new AndroidX.Core.App.NotificationCompat.Builder(this, CHANNEL_ID)
                .SetSmallIcon(icon) // آیکن کوچک در status bar
                .SetLargeIcon(albumArt)
                .SetContentTitle(title)
                .SetContentText(artist)
                .SetColor(Color.DarkRed) // رنگ تم (می‌تونی تغییر بدی)
                .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle()
                    .SetShowActionsInCompactView(0, 1, 2) // سه دکمه در حالت جمع‌شده
                    .SetMediaSession(null))
                .AddAction(Resource.Drawable.IcMediaPrevious, "قبلی", prevPendingIntent)
                .AddAction(isPlaying ? Resource.Drawable.IcMediaPause : Resource.Drawable.IcMediaPlay,
                           isPlaying ? "توقف" : "پخش", playPendingIntent)
                .AddAction(Resource.Drawable.IcMediaNext, "بعدی", nextPendingIntent)
                .SetOngoing(isPlaying)
                .SetShowWhen(false)
                .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic)
                .SetOnlyAlertOnce(true)
                .SetAutoCancel(false)
                .AddAction(Resource.Drawable.IcMenuCloseClearCancel, "بستن", closePendingIntent)
                .SetSilent(true);

            // نمایش
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
