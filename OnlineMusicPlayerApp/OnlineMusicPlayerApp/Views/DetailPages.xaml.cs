using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using OnlineMusicPlayerApp.Models;
using OnlineMusicPlayerApp.Services.PlayListServices;
using Xamarin.Essentials;

namespace OnlineMusicPlayerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailPages : ContentPage
    {
        private List<Detail> playableItems;
        private int currentIndex = -1;
        private bool isTimerRunning = false;

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
                    PlaybackCapsule.CurrentPlaylist = playableItems;
                    PlaybackCapsule.CurrentIndex = currentIndex;

                    playerPanel.IsVisible = true;
                    CoverImage.Source = PlaybackCapsule.LoadCurrentImageTag();
                    CoverImage.IsVisible = true;

                    double resumePosition = 0;
                    double.TryParse(PlaybackCapsule.LoadSeconds(), out resumePosition);

                    var audioService = DependencyService.Get<IAudioService>();
                    audioService.Play(lastHref, resumePosition);

                    CurrentTimeLabel.Text = TimeSpan.FromSeconds(resumePosition).ToString(@"m\:ss");
                    lblTitle.Text = playableItems[currentIndex].Title;
                    lblTrackNumber.Text = $"{currentIndex + 1}/{playableItems.Count}";
                    btnPlay.ImageSource = "icon_pause1";

                    StartPlaybackTimer();
                }
            }
        }

        private void StartPlaybackTimer()
        {
            if (isTimerRunning) return;
            isTimerRunning = true;

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
                    PlaybackCapsule.CurrentIndex = currentIndex;
                    PlayNext();
                    isTimerRunning = false;
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

                    int index = playableItems.FindIndex(x => x.Id == item.Id);
                    PlaybackCapsule.CurrentPlaylist = playableItems;
                    PlaybackCapsule.CurrentIndex = index;

                    var miniPlayer = new MiniPlayerView(true, index, playableItems);
                    var page = new ContentPage { Content = miniPlayer };
                    await Navigation.PushAsync(page);
                }
            }

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
            PlaybackCapsule.CurrentIndex = currentIndex;
            PlaybackCapsule.CurrentPlaylist = playableItems;
            PlaybackCapsule.SaveTitle(item.Title);
            PlaybackCapsule.SaveCurrentTagImage(item.TagImageSrc);
            PlaybackCapsule.SaveCurrentUrl(item.Href);
            PlaybackCapsule.SaveCurrentAudioParentId(item.ParentId.ToString());

            lblTitle.Text = item.Title;
            CoverImage.Source = item.TagImageSrc;
            CoverImage.IsVisible = true;
            lblTrackNumber.Text = $"{currentIndex + 1}/{playableItems.Count}";
            btnPlay.ImageSource = "icon_pause1";

            var audioService = DependencyService.Get<IAudioService>();
            audioService.Play(item.Href);

            StartPlaybackTimer();
        }

        private void ProgressSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var audioService = DependencyService.Get<IAudioService>();
            if (Math.Abs(e.NewValue - audioService.GetCurrentPositionSeconds()) > 1)
            {
                long newPositionMs = (long)(e.NewValue * 1000);
                audioService.SeekTo(newPositionMs);
            }
        }

        private void OnPreviousClicked(object sender, EventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                PlaybackCapsule.CurrentIndex = currentIndex;
                PlayNext();
            }
        }

        private void OnNextClicked(object sender, EventArgs e)
        {
            if (currentIndex < playableItems.Count - 1)
            {
                currentIndex++;
            }
            else
            {
                currentIndex = 0;
            }

            PlaybackCapsule.CurrentIndex = currentIndex;
            PlayNext();
        }

        private void OnPlayClicked(object sender, EventArgs e)
        {
            var audioService = DependencyService.Get<IAudioService>();
            if (audioService.IsPlaying())
            {
                btnPlay.ImageSource = "icon_play1";
                audioService.Pause();
            }
            else
            {
                btnPlay.ImageSource = "icon_pause1";
                audioService.Resume();
            }
        }
    }
}
