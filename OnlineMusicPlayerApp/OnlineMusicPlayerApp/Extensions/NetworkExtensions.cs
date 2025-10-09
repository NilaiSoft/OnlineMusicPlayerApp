using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OnlineMusicPlayerApp.Extensions
{
    public static class NetworkExtensions
    {
        public static async Task<bool> IsInternetAvailableAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    //var response = await client.GetAsync("https://www.microsoft.com");
                    var response = await client.GetAsync("https://clients3.google.com/generate_204");
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> IsConnectedAsync()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                return false;

            return await IsInternetAvailableAsync();
        }
    }
}
