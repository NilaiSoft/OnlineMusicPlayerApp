using OnlineMusicPlayerApp.Models;
using Xamarin.Essentials;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace OnlineMusicPlayerApp.Services.PlayListServices
{
    public static class PlaybackCapsule
    {
        // 🔹 لیست آهنگ‌ها با ذخیره‌سازی دائمی
        public static List<Detail> CurrentPlaylist
        {
            get
            {
                string json = Preferences.Get("CurrentPlaylist", "");
                return string.IsNullOrEmpty(json) ? new List<Detail>() : JsonConvert.DeserializeObject<List<Detail>>(json);
            }
            set
            {
                string json = JsonConvert.SerializeObject(value);
                Preferences.Set("CurrentPlaylist", json);
            }
        }

        // 🔹 ایندکس آهنگ فعلی
        public static int CurrentIndex
        {
            get => Preferences.Get("CurrentIndex", -1);
            set => Preferences.Set("CurrentIndex", value);
        }

        // 🔹 عنوان آهنگ فعلی
        public static string CurrentTitle
        {
            get => Preferences.Get("CurrentTitle", "در حال پخش...");
            set => Preferences.Set("CurrentTitle", value);
        }

        // 🔹 تصویر کاور فعلی
        public static string CurrentImageTag
        {
            get => Preferences.Get("CurrentImageTag", "default_cover.png");
            set => Preferences.Set("CurrentImageTag", value);
        }

        // 🔹 مسیر فایل صوتی فعلی
        public static string CurrentUrl
        {
            get => Preferences.Get("CurrentUrl", "");
            set => Preferences.Set("CurrentUrl", value);
        }

        // 🔹 شناسه والد فایل صوتی
        public static string CurrentAudioParentId
        {
            get => Preferences.Get("CurrentAudioParentId", "");
            set => Preferences.Set("CurrentAudioParentId", value);
        }

        // 🔹 موقعیت فعلی پخش (ثانیه)
        public static double CurrentSliderPosition
        {
            get => Preferences.Get("CurrentSliderPosition", 0.0);
            set => Preferences.Set("CurrentSliderPosition", value);
        }

        // 🎯 متدهای ذخیره‌سازی
        public static void SaveTitle(string title) => CurrentTitle = title;
        public static void SaveCurrentTagImage(string imagePath) => CurrentImageTag = imagePath;
        public static void SaveCurrentUrl(string url) => CurrentUrl = url;
        public static void SaveCurrentAudioParentId(string parentId) => CurrentAudioParentId = parentId;
        public static void SaveSliderPosition(double seconds) => CurrentSliderPosition = seconds;

        // 🎯 متدهای بازیابی
        public static string LoadCurrentTitleMusic() => CurrentTitle;
        public static string LoadCurrentImageTag() => CurrentImageTag;
        public static string LoadHref() => CurrentUrl;
        public static string LoadParentId() => CurrentAudioParentId;
        public static string LoadSeconds() => CurrentSliderPosition.ToString();
    }
}
