using OnlineMusicPlayerApp.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using OnlineMusicPlayerApp.Models;
using System.Collections.Generic;
using System.Linq;

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
        //#if DEBUG
        //fileId = "1Fg7i1jbE498ihc1ZjB-n-Xf-HT4Of6fp";
        //#else
        if (!await NetworkExtensions.IsConnectedAsync())
        {
            return string.Empty;
        }
        //#endif

        string url = $"https://drive.google.com/uc?export=download&id={fileId}";


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

    public async Task<Category> LoadCategoryFromGoogleSheet(string csvUrl, string masterName, string albumImage)
    {
        try
        {
            csvUrl = "https://docs.google.com/spreadsheets/d/1TxEMoJVEupNGldUc-l8YhejDb8Fdfy62/export?format=csv";

            using (HttpClient client = new HttpClient())
            {
                string csv = await client.GetStringAsync(csvUrl);

                var lines = csv.Split('\n');

                // لیست دیتیل‌ها
                List<Detail> details = new List<Detail>();

                // شروع از 1 چون ردیف اول header است
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var cols = line.Split(',');

                    if (cols.Length < 7) continue;

                    details.Add(new Detail
                    {
                        Id = int.Parse(cols[0]),
                        ParentId = int.Parse(cols[1]),
                        Title = cols[2],
                        Href = cols[3],
                        TagImageSrc = cols[4],
                        ListImageSrc = cols[5],
                        IsVisible = cols[6] == "1" || cols[6].ToLower() == "true"
                    });
                }

                // ساخت ساختار درختی
                foreach (var item in details)
                {
                    item.Children = details.FindAll(d => d.ParentId == item.Id);
                }

                // فقط Root ها
                var rootDetails = details.FindAll(d => d.ParentId == 0);

                return new Category
                {
                    Master = masterName,
                    AlbumImageSrc = albumImage,
                    Details = rootDetails
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading Google Sheet: " + ex.Message);
            return null;
        }
    }

    public static List<Detail> BuildTree(List<Detail> all)
    {
        var lookup = all.ToDictionary(x => x.Id);
        var roots = new List<Detail>();

        foreach (var d in all)
        {
            // اگر ParentId نداشت → ریشه است
            if (d.ParentId == null || d.ParentId == 0)
            {
                roots.Add(d);
            }
            else if (lookup.TryGetValue(d.ParentId.Value, out var parent))
            {
                parent.Children.Add(d);
            }
        }

        return roots;
    }

    public async Task<List<Category>> GetCategoriesFromGoogleSheet()
    {
        // string url = "https://docs.google.com/spreadsheets/d/e/2PACX-1vTywA04lOdRwH-qOWxLU4FQRU472gsSqccfrWxF-BYMLtFPIj92CsfxIpxpkfQEUSy4K3N9_UNNGCJi/export?format=csv&gid=1525582622";
        string url = "https://docs.google.com/spreadsheets/d/e/2PACX-1vTywA04lOdRwH-qOWxLU4FQRU472gsSqccfrWxF-BYMLtFPIj92CsfxIpxpkfQEUSy4K3N9_UNNGCJi/pub?gid=1525582622&single=true&output=csv";

        using (HttpClient client = new HttpClient())
        {
            string csv = await client.GetStringAsync(url);
            var lines = csv.Split('\n');

            List<Detail> items = new List<Detail>();

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var c = line.Split(',');

                items.Add(new Detail
                {
                    Id = int.Parse(c[0]),
                    ParentId = int.TryParse(c[1], out var pid) ? pid : (int?)null,
                    RepeatId = int.TryParse(c[2], out var rep) ? rep : (int?)null,
                    Title = c[3],
                    Href = c[4],
                    TagImageSrc = c[5],
                    ListImageSrc = c[6],
                    IsVisible = c[7] == "1" || c[7].ToLower() == "true"
                });
            }

            var tree = BuildTree(items);

            return new List<Category>
        {
            new Category
            {
                Master = "Masters",
                Details = tree,
                AlbumImageSrc = ""
            }
        };
        }
    }

    public async Task<List<Category>> GetCategoriesFromGoogleSheetLocal(bool isConnect)
    {
        string url = "https://docs.google.com/spreadsheets/d/e/2PACX-1vTywA04lOdRwH-qOWxLU4FQRU472gsSqccfrWxF-BYMLtFPIj92CsfxIpxpkfQEUSy4K3N9_UNNGCJi/pub?gid=1525582622&single=true&output=csv";

        // مسیر personal
        string personalPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string csvPath = Path.Combine(personalPath, "categories.csv");

        // اگر فایل وجود ندارد، دانلود کن
        //if (!File.Exists(csvPath))
        if (isConnect)
        {
            await DownloadCsvAsync(url, csvPath);
        }

        // خواندن CSV از پوشه Personal
        string csv = await File.ReadAllTextAsync(csvPath);

        var lines = csv.Split('\n');
        List<Detail> items = new List<Detail>();

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var c = line.Split(',');

            items.Add(new Detail
            {
                Id = int.Parse(c[0]),
                ParentId = int.TryParse(c[1], out var pid) ? pid : (int?)null,
                RepeatId = int.TryParse(c[2], out var rep) ? rep : (int?)null,
                Title = c[3],
                Href = c[4],
                TagImageSrc = c[5],
                ListImageSrc = c[6],
                IsVisible = c[7] == "1" || c[7].ToLower() == "true"
            });
        }

        var tree = BuildTree(items);

        return new List<Category>
        {
            new Category
            {
                Master = "Masters",
                Details = tree,
                AlbumImageSrc = ""
            }
        };
    }

    private async Task DownloadCsvAsync(string url, string localPath)
    {
        using HttpClient client = new HttpClient();

        string csv = await client.GetStringAsync(url);

        File.WriteAllText(localPath, csv);
    }

}
