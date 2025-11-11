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

        // 🔹 ذخیره عنوان و کاور فعلی برای جلوگیری از بازگشت به پیش‌فرض
        private string _currentTitle = "در حال پخش موزیک";
        private string _currentArtist = "";
        private string _currentAlbumArtPath = "";

        public override void OnCreate()
        {
            base.OnCreate();
            player = new SimpleExoPlayer.Builder(this).Build();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            string action = intent?.Action;

            // 🎵 اطلاعات آهنگ از Intent (در صورت وجود)
            string title = intent?.GetStringExtra("title");
            string artist = intent?.GetStringExtra("artist");
            string albumArtPath = intent?.GetStringExtra("albumArtPath");
            string url = intent?.GetStringExtra("url");

            // اگر داده جدید اومد، ذخیره کن
            if (!string.IsNullOrEmpty(title))
                _currentTitle = title;

            if (!string.IsNullOrEmpty(artist))
                _currentArtist = artist;

            if (!string.IsNullOrEmpty(albumArtPath))
                _currentAlbumArtPath = albumArtPath;

            switch (action)
            {
                case "ACTION_CLOSE":
                    StopForeground(true);
                    StopSelf();

                    // خروج کامل از برنامه
                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                    System.Environment.Exit(0);
                    return StartCommandResult.NotSticky;

                case "ACTION_PAUSE":
                    player?.Pause();
                    UpdateNotification(_currentTitle, _currentArtist, _currentAlbumArtPath, false);
                    return StartCommandResult.Sticky;

                case "ACTION_PLAY":
                    player?.Play();
                    UpdateNotification(_currentTitle, _currentArtist, _currentAlbumArtPath, true);
                    return StartCommandResult.Sticky;

                case "ACTION_NEXT":
                    // اینجا می‌تونی تابع NextSong بنویسی
                    UpdateNotification(_currentTitle, _currentArtist, _currentAlbumArtPath, true);
                    return StartCommandResult.Sticky;

                case "ACTION_PREV":
                    // اینجا هم تابع قبلی رو بنویس
                    UpdateNotification(_currentTitle, _currentArtist, _currentAlbumArtPath, true);
                    return StartCommandResult.Sticky;
            }

            // شروع پخش موزیک جدید (در صورت وجود URL)
            if (!string.IsNullOrEmpty(url))
            {
                var mediaItem = MediaItem.FromUri(url);
                player.SetMediaItem(mediaItem);
                player.Prepare();
                player.Play();
            }

            // موقعیت شروع (اختیاری)
            string posStr = intent?.GetStringExtra("position");
            if (!string.IsNullOrEmpty(posStr) && double.TryParse(posStr, out var startSeconds))
            {
                long startMs = (long)(startSeconds * 1000);
                new Handler().PostDelayed(() => player.SeekTo(startMs), 100);
            }

            CreateNotificationChannel();
            UpdateNotification(_currentTitle, _currentArtist, _currentAlbumArtPath, true);

            return StartCommandResult.Sticky;
        }

        // 🔹 ساخت کانال نوتیف برای Android O+
        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(CHANNEL_ID, "Music Playback", NotificationImportance.Low)
                {
                    Description = "پخش موسیقی در پس‌زمینه"
                };

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        // 🔹 ایجاد نوتیف مدرن با کنترل‌ها
        private void UpdateNotification(string title, string artist, string albumArtPath, bool isPlaying)
        {
            Bitmap albumArt = null;

            if (!string.IsNullOrEmpty(albumArtPath) && File.Exists(albumArtPath))
            {
                albumArt = BitmapFactory.DecodeFile(albumArtPath);
            }
            else
            {
                // تصویر پیش‌فرض از Resources
                //albumArt = BitmapFactory.DecodeResource(Resources, Resource.Drawable.netaudioicon);
            }

            // 🔹 PendingIntents برای کنترل‌ها
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

            // 🔹 ساخت نوتیف
            var builder = new AndroidX.Core.App.NotificationCompat.Builder(this, CHANNEL_ID)
                .SetSmallIcon(icon)
                .SetLargeIcon(albumArt)
                .SetContentTitle(title)
                .SetContentText(artist)
                .SetColor(Color.ParseColor("#B71C1C")) // رنگ قرمز تیره
                .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle()
                    .SetShowActionsInCompactView(0, 1, 2)
                    .SetMediaSession(null))
                .AddAction(Resource.Drawable.IcMediaPrevious, "قبلی", prevPendingIntent)
                .AddAction(isPlaying ? Resource.Drawable.IcMediaPause : Resource.Drawable.IcMediaPlay,
                           isPlaying ? "توقف" : "پخش", playPendingIntent)
                .AddAction(Resource.Drawable.IcMediaNext, "بعدی", nextPendingIntent)
                .SetOngoing(isPlaying)
                .SetShowWhen(false)
                .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic)
                .SetOnlyAlertOnce(true)
                .AddAction(Resource.Drawable.IcMenuCloseClearCancel, "بستن", closePendingIntent)
                .SetSilent(true);

            // نمایش نوتیف در Foreground
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
