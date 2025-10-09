using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Xamarin.Essentials;

namespace OnlineMusicPlayerApp
{
    public static class Utility
    {
        public static int ToInt32(this string inputStringNumber)
        {
            return Convert.ToInt32(inputStringNumber);
        }

        public static bool IsConnectedToInternet()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool CheckConnection()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.None)
            {
                return false;
            }
            return true;
        }
    }
}
