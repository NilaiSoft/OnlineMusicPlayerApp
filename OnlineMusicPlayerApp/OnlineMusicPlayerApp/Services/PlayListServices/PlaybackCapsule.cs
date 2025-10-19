using OnlineMusicPlayerApp.Models;
using System.Collections.Generic;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp.Services.PlayListServices
{
    public static class PlaybackCapsule
    {
        public static List<Detail> CurrentPlaylist { get; set; } = new List<Detail>();
        public static int CurrentIndex { get; set; } = -1;

        public static string CurrentTitle { get; set; } = "در حال پخش...";
        public static string CurrentImageTag { get; set; } = "default_cover.png";

        public static void SaveTitle(string title)
        {
            CurrentTitle = title;
        }

        public static void SaveCurrentTagImage(string imagePath)
        {
            CurrentImageTag = imagePath;
        }

        public static string LoadCurrentTitleMusic()
        {
            return CurrentTitle;
        }

        public static string LoadCurrentImageTag()
        {
            return CurrentImageTag;
        }

        public static string CurrentUrl { get; set; } = string.Empty;

        // متد ذخیره‌سازی مسیر فایل صوتی فعلی
        public static void SaveCurrentUrl(string url)
        {
            CurrentUrl = url;
        }

        // متد بازیابی مسیر فایل صوتی فعلی
        public static string LoadHref()
        {
            return CurrentUrl;
        }
        // سایر پراپرتی‌ها...
        public static string CurrentAudioParentId { get; set; } = string.Empty;

        // ذخیره‌سازی شناسه‌ی والد
        public static void SaveCurrentAudioParentId(string parentId)
        {
            CurrentAudioParentId = parentId;
        }

        // بازیابی شناسه‌ی والد
        public static string LoadParentId()
        {
            return CurrentAudioParentId;
        }

        // سایر پراپرتی‌ها...
        public static double CurrentSliderPosition { get; set; } = 0;

        // ذخیره‌سازی موقعیت اسلایدر
        public static void SaveSliderPosition(double seconds)
        {
            CurrentSliderPosition = seconds;
        }

        // بازیابی موقعیت اسلایدر
        public static string LoadSeconds()
        {
            return CurrentSliderPosition.ToString();
        }
    }
}
