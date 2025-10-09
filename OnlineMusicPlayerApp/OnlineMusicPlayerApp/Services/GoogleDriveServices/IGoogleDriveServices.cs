using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public interface IGoogleDriveServices
{
    Task<string> GetTextFromFileAsync();
    Task<string> DownloadGoogleDriveFileAsync(string url, string fileName);
    Task<T> LoadJsonFromDriveAsync<T>(string fileUrl);
}
