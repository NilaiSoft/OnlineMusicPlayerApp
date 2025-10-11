using OnlineMusicPlayerApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace OnlineMusicPlayerApp.Views
{
    [DesignTimeVisible(false)]
    public partial class DetailPages : ContentPage
    {
        private List<Detail> playableItems;
        private int currentIndex = 0;

        public DetailPages(List<Detail> details)
        {
            InitializeComponent();
            Title = "سبک‌های موسیقی";

            playableItems = details
                .Where(d => d.Children == null || !d.Children.Any())
                .Where(d => new[] { ".mp3", ".mp4" }.Any(ext => Path.GetExtension(d.Href).Contains(ext)))
                .ToList();

            playerPanel.IsVisible = playableItems.Any();

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
                        await Navigation.PushAsync(new DetailPages(item.Children));
                    }
                    else
                    {
                        string extension = Path.GetExtension(item.Href);
                        if (!new[] { ".mp3", ".mp4" }.Any(ext => extension.Contains(ext)))
                            return;

                        currentIndex = playableItems.FindIndex(d => d.Href == item.Href);
                        PlayNext();
                    }
                };

                stack.Children.Add(button);
            }
        }

        private void PlayNext()
        {
            if (currentIndex >= playableItems.Count)
                return;

            var item = playableItems[currentIndex];
            string decodedUrl = Uri.UnescapeDataString(item.Href);

            CoverImage.Source = item.TagImageSrc;
            CoverImage.IsVisible = !string.IsNullOrEmpty(item.TagImageSrc);
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

            DependencyService.Get<IAudioService>().Play(decodedUrl);

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                var duration = DependencyService.Get<IAudioService>().GetDurationSeconds();
                var position = DependencyService.Get<IAudioService>().GetCurrentPositionSeconds();

                if (!double.IsNaN(duration) && duration > 0)
                {
                    ProgressSlider.Maximum = duration;
                    TotalTimeLabel.Text = TimeSpan.FromSeconds(duration).ToString(@"m\:ss");
                }

                if (!double.IsNaN(position) && position >= 0 && position <= duration)
                {
                    ProgressSlider.Value = position;
                    CurrentTimeLabel.Text = TimeSpan.FromSeconds(position).ToString(@"m\:ss");
                }

                if (position >= duration - 1 && duration > 0)
                {
                    currentIndex++;
                    PlayNext();
                    return false;
                }

                return true;
            });
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
            if (DependencyService.Get<IAudioService>().IsPlaying())
            {
                btnPlay.ImageSource = "icon_play1";
                DependencyService.Get<IAudioService>().Pause();
                return;
            }

            btnPlay.ImageSource = "icon_pause1";
            DependencyService.Get<IAudioService>().Resume();
        }

        private void OnNextClicked(object sender, EventArgs e)
        {
            if (currentIndex < playableItems.Count - 1)
            {
                currentIndex++;
            }
            else
            {
                currentIndex = 0; // بازگشت به ابتدای لیست
            }

            PlayNext();
        }

        private void OnPreviousClicked(object sender, EventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
            }
            else
            {
                currentIndex = playableItems.Count - 1; // رفتن به آخر لیست
            }

            PlayNext();
        }

    }
}
