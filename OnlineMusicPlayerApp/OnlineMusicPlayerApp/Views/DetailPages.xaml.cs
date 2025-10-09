using OnlineMusicPlayerApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp.Views
{
    [DesignTimeVisible(false)]
    public partial class DetailPages : ContentPage
    {
        public DetailPages(List<Detail> details)
        {
            InitializeComponent();

            Title = "سبک‌های موسیقی";

            //var stack = new StackLayout { Padding = 20, Spacing = 15 };

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

                        string extension = Path.GetExtension(item.Href);

                        if (!new[] { ".mp3", ".mp4" }.Any(ext => extension.Contains(ext)))
                        {
                            return;
                        }

                        string decodedUrl = Uri.UnescapeDataString(item.Href);
                        CoverImage.Source = item.TagImageSrc;
                        lblTitle.Text = item.Title;
                        Player.Source = decodedUrl;
                        Player.Play();

                        Player.MediaEnded += (s, e) => DisplayAlert("پایان", "آهنگ به پایان رسید", "باشه");

                        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                        {
                            if (Player.CurrentState == MediaElementState.Playing && Player.Duration.HasValue)
                            {
                                ProgressSlider.Maximum = Player.Duration.Value.TotalSeconds;
                                ProgressSlider.Value = Player.Position.TotalSeconds;
                            }
                            return true;
                        });
                        // یا اگر از Shell استفاده می‌کنی:
                        // await Shell.Current.GoToAsync(item.Href);
                    }
                };

                stack.Children.Add(button);
            }

            Content = new ScrollView { Content = stack };
        }

        private void OnPlayClicked(object sender, EventArgs e)
        {
            Player.Play();
        }

        private void OnPauseClicked(object sender, EventArgs e)
        {
            Player.Pause();
        }
    }
}
