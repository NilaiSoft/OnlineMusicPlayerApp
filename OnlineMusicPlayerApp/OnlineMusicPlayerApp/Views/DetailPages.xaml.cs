using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using OnlineMusicPlayerApp.Models;
using Xamarin.Essentials;

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

        protected override void OnAppearing()
        {
            base.OnAppearing();

            string lastHref = PlaybackCapsule.LoadHref();
            if (!string.IsNullOrEmpty(lastHref))
            {
                int index = playableItems.FindIndex(d => d.Href == lastHref);
                if (index >= 0)
                {
                    currentIndex = index;
                    playerPanel.IsVisible = true;

                    // بازیابی تصویر کاور
                    CoverImage.Source = PlaybackCapsule.LoadCurrentImageTag();
                    CoverImage.IsVisible = true;
                    // بازیابی موقعیت پخش
                    double resumePosition = 0;
                    double.TryParse(PlaybackCapsule.LoadSeconds(), out resumePosition);

                    // پخش از موقعیت قبلی
                    var audioService = DependencyService.Get<IAudioService>();
                    audioService.Play(lastHref, resumePosition);

                    // تنظیم اسلایدر و برچسب‌ها
                    //ProgressSlider.Value = resumePosition;
                    CurrentTimeLabel.Text = TimeSpan.FromSeconds(resumePosition).ToString(@"m\:ss");

                    lblTitle.Text = playableItems[currentIndex].Title;
                    lblTrackNumber.Text = $"{currentIndex + 1}/{playableItems.Count}";
                    btnPlay.ImageSource = "icon_pause1";

                    // شروع تایمر برای آپدیت
                    StartPlaybackTimer();
                }
            }
        }


        private void StartPlaybackTimer()
        {
            var audioService = DependencyService.Get<IAudioService>();

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
                    PlaybackCapsule.SaveSliderPosition(position);
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
                    playerPanel.IsVisible = true;
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
            PlaybackCapsule.SaveCurrentTagImage(item.TagImageSrc);
            PlaybackCapsule.SaveCurrentUrl(item.Href);
            CoverImage.IsVisible = true;

            // 🎯 نمایش شماره ترک به‌صورت 1/2
            lblTrackNumber.Text = $"{currentIndex + 1}/{playableItems.Count}";

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
                    PlaybackCapsule.SaveSliderPosition(position);
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

        private void OnMinimizeClicked(object sender, EventArgs e)
        {

        }
    }
}
