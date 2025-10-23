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
        string fileId = "1NRhEt-01wf5MSqYnjvoxkPB5tFXqfkiw";
        string url = $"https://drive.google.com/uc?export=download&id={fileId}";

        //if (!await NetworkExtensions.IsConnectedAsync())
        //{
        //    return string.Empty;
        //}

        using (var client = new HttpClient())
        {
            try
            {
                string content = await client.GetStringAsync(url);
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

    public async Task<string> DownloadGoogleDriveFileWithProgressAsync(string url, string fileName, IProgress<double> progress)
    {
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string filePath = Path.Combine(folderPath, fileName);

        using var client = new HttpClient();
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes != -1 && progress != null;

        using var stream = await response.Content.ReadAsStreamAsync();
        using var fileStream = File.Create(filePath);

        var buffer = new byte[8192];
        long totalRead = 0;
        int read;

        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, read);
            totalRead += read;

            if (canReportProgress)
            {
                double percent = (double)totalRead / totalBytes;
                progress.Report(percent);
            }
        }

        return filePath;
    }
}
