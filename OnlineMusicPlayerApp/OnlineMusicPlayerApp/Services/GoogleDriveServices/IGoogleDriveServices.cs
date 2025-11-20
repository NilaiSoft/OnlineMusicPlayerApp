using OnlineMusicPlayerApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public interface IGoogleDriveServices
{
    Task<string> GetTextFromFileAsync();
    Task<string> DownloadGoogleDriveFileAsync(string url, string fileName);
    Task<string> DownloadGoogleDriveFileWithProgressAsync(string url, string fileName, IProgress<double> progress);

    Task<T> LoadJsonFromDriveAsync<T>(string fileUrl);
    Task<string> GetMusicPlayList();
    Task<Category> LoadCategoryFromGoogleSheet(string csvUrl, string masterName, string albumImage);
    Task<List<Category>> GetCategoriesFromGoogleSheet();
}
