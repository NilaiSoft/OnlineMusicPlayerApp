using Newtonsoft.Json;
using OnlineMusicPlayerApp.Models;
using OnlineMusicPlayerApp.Services.PlayListServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;


[assembly: Dependency(typeof(PlayListServices))]
public class PlayListServices : IPlayListServices
{
    public async Task<List<Category>> GetCategoriesFromJson()
    {
        var json = await DependencyService.Get<IGoogleDriveServices>().GetMusicPlayList();
        var wrapper = JsonConvert.DeserializeObject<CategoryWrapper>(json);
        return wrapper?.Categories ?? new List<Category>();
    }

    public static class PlaybackCapsule
    {
        public static List<Detail> CurrentPlaylist { get; set; } = new List<Detail>();
        public static int CurrentIndex { get; set; } = -1;

        public static string CurrentTitle { get; set; } = "در حال پخش...";
        public static string CurrentImageTag { get; set; } = "default_cover.png";
        public static string CurrentUrl { get; set; } = string.Empty;
        public static string CurrentAudioParentId { get; set; } = string.Empty;
        public static double CurrentSliderPosition { get; set; } = 0;

        // ذخیره‌سازی
        public static void SaveTitle(string title) => CurrentTitle = title;
        public static void SaveCurrentTagImage(string imagePath) => CurrentImageTag = imagePath;
        public static void SaveCurrentUrl(string url) => CurrentUrl = url;
        public static void SaveCurrentAudioParentId(string parentId) => CurrentAudioParentId = parentId;
        public static void SaveSliderPosition(double seconds) => CurrentSliderPosition = seconds;

        // بازیابی
        public static string LoadCurrentTitleMusic() => CurrentTitle;
        public static string LoadCurrentImageTag() => CurrentImageTag;
        public static string LoadHref() => CurrentUrl;
        public static string LoadParentId() => CurrentAudioParentId;
        public static string LoadSeconds() => CurrentSliderPosition.ToString();
    }
}
