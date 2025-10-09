using OnlineMusicPlayerApp.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(GoogleDriveServices))]
public class GoogleDriveServices : IGoogleDriveServices
{
    public async Task<string> DownloadGoogleDriveFileAsync(string url, string fileName)
    {
        try
        {
            if (!await NetworkExtensions.IsInternetAvailableAsync())
            {
                return string.Empty;
            }

            //https://drive.google.com/uc?export=download&id=1Dxo_MO-LcEHUdrO5j9GIy7FGqOkJ4qmc
            //url = $"https://drive.google.com/{url}";
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), fileName);

            using var client = new HttpClient();
            var stream = await client.GetStreamAsync(url);
            using var fileStream = File.Create(path);
            await stream.CopyToAsync(fileStream);
            return path;
        }
        catch (Exception ex)
        {
            // مدیریت خطا
            return "";
        }
    }

    public async Task<string> GetTextFromFileAsync()
    {
        string fileUrl = $"https://drive.google.com/uc?export=download&id=1vOZx4p5atfvPl8GtFpLvVl-TFbw0SWXm";

        if (!await NetworkExtensions.IsConnectedAsync())
        {
            return string.Empty;
        }

        using (var client = new HttpClient())
        {
            try
            {
                string content = await client.GetStringAsync(fileUrl);
                return content;
            }
            catch (Exception ex)
            {
                //return $"خطا در دریافت فایل: {ex.Message}";
                return string.Empty;
            }
        }
    }

    public async Task<string> GetMusicPlayList()
    {
        string fileUrl = $"https://drive.google.com/uc?export=download&id=19mhIWQI8IQ1d-lP3Xyj30XUvO_qW6mny";

        if (!await NetworkExtensions.IsConnectedAsync())
        {
            return string.Empty;
        }

        using (var client = new HttpClient())
        {
            try
            {
                string content = await client.GetStringAsync(fileUrl);
                return content;
            }
            catch (Exception ex)
            {
                //return $"خطا در دریافت فایل: {ex.Message}";
                return string.Empty;
            }
        }
    }

    public async Task<T> LoadJsonFromDriveAsync<T>(string fileUrl)
    {
        using (var client = new HttpClient())
        {
            string json = await client.GetStringAsync(fileUrl);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
