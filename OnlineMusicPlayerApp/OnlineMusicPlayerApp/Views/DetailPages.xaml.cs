using OnlineMusicPlayerApp.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp.Views
{
    [DesignTimeVisible(false)]
    public partial class DetailPages : ContentPage
    {
        public DetailPages(List<Detail> details)
        {
            Title = "سبک‌های موسیقی";

            var stack = new StackLayout { Padding = 20, Spacing = 15 };

            foreach (var item in details)
            {
                var button = new Button
                {
                    Text = item.Title,
                    BackgroundColor = Color.FromHex("#eeeeee"),
                    TextColor = Color.Black,
                    CornerRadius = 8
                };

                button.Clicked += async (s, e) =>
                {
                    if (item.Children != null && item.Children.Any())
                    {
                        // رفتن به صفحه‌ی جدید با زیرمجموعه‌ها
                        await Navigation.PushAsync(new DetailPages(item.Children));
                    }
                    else
                    {
                        // اگر زیرمجموعه نداره، رفتن به href یا نمایش پیام
                        // await DisplayAlert("مسیریابی", $"رفتن به: {item.Href}", "باشه");
                        Player.Source = item.Href;
                        Player.Play();
                        // یا اگر از Shell استفاده می‌کنی:
                        // await Shell.Current.GoToAsync(item.Href);
                    }
                };

                stack.Children.Add(button);
            }

            Content = new ScrollView { Content = stack };
        }
    }
}
