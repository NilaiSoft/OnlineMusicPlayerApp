using OnlineMusicPlayerApp.Services;
using OnlineMusicPlayerApp.Views.Popups;
using OnlineMusicPlayerApp.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp.Extensions
{
    public class FormExtensions
    {
        public static async Task ShowBuildInfoModalAsync(INavigation navigation, string text, string image, Func<string, Task> resultHandler)
        {
            await navigation.PushModalAsync(new LoadingBoxPage());

            var modal = await HtmlModalPage.CreateAsync(text, image, "ادامه", "بستن");

            await navigation.PopModalAsync();
            await navigation.PushModalAsync(modal);

            string result = await modal.Result;
            await resultHandler.Invoke(result);
        }
    }
}
