using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using OnlineMusicPlayerApp.Models;

namespace OnlineMusicPlayerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailPages : ContentPage
    {
        private List<Detail> playableItems;
        private int currentIndex = -1;

        public DetailPages(List<Detail> details)
        {
            InitializeComponent();

            playableItems = Flatten(details);
            DetailsListView.ItemsSource = details;
        }

        private async void DetailsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Detail item)
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
            }

            // Unselect item after tap
            ((ListView)sender).SelectedItem = null;
        }

        private List<Detail> Flatten(List<Detail> details)
        {
            var flat = new List<Detail>();
            foreach (var item in details)
            {
                if (item.Children != null && item.Children.Any())
                    flat.AddRange(item.Children);
                else
                    flat.Add(item);
            }
            return flat;
        }

        private void PlayNext()
        {
            if (currentIndex < 0 || currentIndex >= playableItems.Count)
                return;

            var item = playableItems[currentIndex];
            lblTitle.Text = item.Title;
            CoverImage.Source = item.TagImageSrc;
            CoverImage.IsVisible = true;

            btnPlay.ImageSource = "icon_pause1";
            var audioService = DependencyService.Get<IAudioService>();
            audioService.Play(item.Href);

            // 🎯 شروع تایمر برای آپدیت زمان و اسلایدر
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                var duration = audioService.GetDurationSeconds();
                var position = audioService.GetCurrentPositionSeconds();

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
                    return false; // توقف تایمر
                }

                return true; // ادامه تایمر
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

        private void OnPreviousClicked(object sender, EventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                PlayNext();
            }
        }

        private void OnNextClicked(object sender, EventArgs e)
        {
            if (currentIndex < playableItems.Count - 1)
            {
                currentIndex++;
                PlayNext();
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
    }
}
