using OnlineMusicPlayerApp.Extensions;
using OnlineMusicPlayerApp.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp.Services
{
    public static class SettingsManager
    {
        public static GoogleDriveSettings CurrentSettings { get; private set; }

        public static async Task LoadAsync()
        {
            if (!await NetworkExtensions.IsConnectedAsync())
            {
                CurrentSettings = new GoogleDriveSettings();
                return;
            }

            string fileUrl = $"https://drive.google.com/uc?export=download&id=1vOZx4p5atfvPl8GtFpLvVl-TFbw0SWXm";
            try
            {
                using (var client = new HttpClient())
                {
                    string json = await DependencyService.Get<IGoogleDriveServices>().GetTextFromFileAsync();

                    if (string.IsNullOrEmpty(json))
                    {
                        CurrentSettings = new GoogleDriveSettings();
                        return;
                    }

                    CurrentSettings = JsonConvert.DeserializeObject<GoogleDriveSettings>(json);
                }
            }
            catch
            {
                CurrentSettings = new GoogleDriveSettings();
            }
        }
    }

}
