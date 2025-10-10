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

                        Color[] warmPalette = {
                            Color.FromRgb(255, 87, 34),
                            Color.FromRgb(244, 67, 54),
                            Color.FromRgb(255, 193, 7)
                        };

                        Random rand = new Random();
                        Color color1 = warmPalette[rand.Next(warmPalette.Length)];
                        Color color2 = warmPalette[rand.Next(warmPalette.Length)];

                        this.Background = new LinearGradientBrush
                        {
                            StartPoint = new Point(0, 0),
                            EndPoint = new Point(1, 1),
                            GradientStops = new GradientStopCollection
                                {
                                    new GradientStop { Color = color1, Offset = 0.0F },
                                    new GradientStop { Color = color2, Offset = 1.5F }
                                }
                        };

                        //Player.Source = decodedUrl;
                        //Player.Play();

                        DependencyService.Get<IAudioService>().Play(decodedUrl);

                        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                        {
                            var duration = DependencyService.Get<IAudioService>().GetDurationSeconds();
                            var position = DependencyService.Get<IAudioService>().GetCurrentPositionSeconds();

                            ProgressSlider.Maximum = duration;
                            ProgressSlider.Value = position;

                            return true; // ادامه بده
                        });



                        //Player.MediaEnded += (s, e) => DisplayAlert("پایان", "آهنگ به پایان رسید", "باشه");

                        //Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                        //{
                        //    if (Player.CurrentState == MediaElementState.Playing && Player.Duration.HasValue)
                        //    {
                        //        ProgressSlider.Maximum = Player.Duration.Value.TotalSeconds;
                        //        ProgressSlider.Value = Player.Position.TotalSeconds;
                        //    }
                        //    return true;
                        //});
                        // یا اگر از Shell استفاده می‌کنی:
                        // await Shell.Current.GoToAsync(item.Href);
                    }
                };

                stack.Children.Add(button);
            }

            Content = new ScrollView { Content = stack };
        }

        private void ProgressSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (Math.Abs(e.NewValue - DependencyService.Get<IAudioService>().GetCurrentPositionSeconds()) > 1)
            {
                long newPositionMs = (long)(e.NewValue * 1000);
                DependencyService.Get<IAudioService>().SeekTo(newPositionMs);
            }
        }


        private void OnPlayClicked(object sender, EventArgs e)
        {
            //if (Player.CurrentState == MediaElementState.Playing)
            if (DependencyService.Get<IAudioService>().IsPlaying())
            {
                btnPlay.ImageSource = "icon_play3";
                DependencyService.Get<IAudioService>().Pause();
                //Player.Pause();
                return;
            }

            btnPlay.ImageSource = "icon_pause";
            DependencyService.Get<IAudioService>().Resume();
            //Player.Play();
        }
    }
}
