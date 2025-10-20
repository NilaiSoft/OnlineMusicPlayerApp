using OnlineMusicPlayerApp.Models;
using OnlineMusicPlayerApp.Services;
using OnlineMusicPlayerApp.Services.PlayListServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp.Views
{
    public partial class MiniPlayerView : ContentView
    {
        private List<Detail> _playableItems;
        private int _currentIndex = -1;
        private bool _isTimerRunning = false;

        public MiniPlayerView()
        {
            InitializeComponent();
            _playableItems = PlaybackCapsule.CurrentPlaylist;
            _currentIndex = PlaybackCapsule.CurrentIndex;
            LoadLastPlaybackInfo();
        }

        public MiniPlayerView(bool isMaximize, int currentIndex, List<Detail> details)
        {
            InitializeComponent();
            MaximizedPanel.IsVisible = true;
            MiniPlayerFrame.IsVisible = false;

            _playableItems = details;
            _currentIndex = currentIndex;

            PlaybackCapsule.CurrentPlaylist = details;
            PlaybackCapsule.CurrentIndex = currentIndex;

            LoadLastPlaybackInfo();
            Task.Run(async () => await PlayNextAsync());
        }

        private void LoadLastPlaybackInfo()
        {
            lblMiniTitle.Text = PlaybackCapsule.LoadCurrentTitleMusic();
            imgMiniCover.Source = PlaybackCapsule.LoadCurrentImageTag();

            var audioService = DependencyService.Get<IAudioService>();
            bool isPlaying = audioService.IsPlaying();
            btnMiniPlay.Source = isPlaying ? "icon_pause1" : "icon_play1";
            btnPlay.ImageSource = isPlaying ? "icon_pause1" : "icon_play1";
        }

        private void OnMiniPlayClicked(object sender, EventArgs e)
        {
            var audioService = DependencyService.Get<IAudioService>();
            bool isPlaying = audioService.IsPlaying();

            if (isPlaying)
            {
                audioService.Pause();
                btnMiniPlay.Source = "icon_play1";
                btnPlay.ImageSource = "icon_play1";
            }
            else
            {
                var isActive = audioService.Resume();
                //if (isActive == null)
                //{
                //    double currentPosition = audioService.GetCurrentPositionSeconds();
                //    PlaybackCapsule.SaveSliderPosition(currentPosition); // ✅ ذخیره موقعیت فعلی
                //    LoadLastPlaybackInfo();
                //    Task.Run(async () => await PlayNextAsync());
                //}

                btnMiniPlay.Source = "icon_pause1";
                btnPlay.ImageSource = "icon_pause1";
            }
        }

        private async void OnMiniPlayerTapped(object sender, EventArgs e)
        {
            var audioService = DependencyService.Get<IAudioService>();
            double currentPosition = audioService.GetCurrentPositionSeconds();
            PlaybackCapsule.SaveSliderPosition(currentPosition); // ✅ ذخیره موقعیت فعلی

            var maximizedView = new MiniPlayerView(true, _currentIndex, _playableItems);
            var page = new ContentPage { Content = maximizedView };
            NavigationPage.SetHasNavigationBar(page, false);
            await Navigation.PushAsync(page);
        }

        private async Task PlayNextAsync()
        {
            if (_currentIndex < 0 || _currentIndex >= _playableItems.Count)
                return;

            var item = _playableItems[_currentIndex];
            if (item == null) return;

            PlaybackCapsule.CurrentIndex = _currentIndex;
            PlaybackCapsule.CurrentTitle = item.Title;
            PlaybackCapsule.CurrentImageTag = item.TagImageSrc;
            PlaybackCapsule.SaveCurrentUrl(item.Href);
            PlaybackCapsule.SaveCurrentAudioParentId(item.ParentId.ToString());

            var audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}");
            if (!File.Exists(audioPath))
            {
                item.Href = await DependencyService.Get<IGoogleDriveServices>()
                    .DownloadGoogleDriveFileAsync(item.Href, Path.GetFileName(audioPath));
            }
            else
            {
                item.Href = audioPath;
            }

            if (string.IsNullOrEmpty(item.Href))
            {
                DependencyService.Get<IToastService>()?.Show($"Href Is Empty");
                return;
            }

            var imagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), $"{item.ParentId}_{item.Id}{Path.GetExtension(item.TagImageSrc)}");
            if (!File.Exists(imagePath))
            {
                item.TagImageSrc = await DependencyService.Get<IGoogleDriveServices>()
                    .DownloadGoogleDriveFileAsync(item.TagImageSrc, Path.GetFileName(imagePath));
            }
            else
            {
                item.TagImageSrc = imagePath;
            }

            CoverImage.Source = item.TagImageSrc;
            imgMiniCover.Source = item.TagImageSrc;
            lblTrackNumber.Text = $"{_currentIndex + 1}/{_playableItems.Count}";
            lblTitle.Text = item.Title;
            lblMiniTitle.Text = item.Title;

            var audioService = DependencyService.Get<IAudioService>();
            double resumePosition = 0;
            double.TryParse(PlaybackCapsule.LoadSeconds(), out resumePosition); // ✅ موقعیت ذخیره‌شده
            audioService.Play(item.Href, resumePosition);
            btnPlay.ImageSource = "icon_pause1";

            if (!_isTimerRunning)
            {
                _isTimerRunning = true;
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
                        _currentIndex++;
                        PlaybackCapsule.SaveSliderPosition(0); // پاک‌سازی موقعیت برای آهنگ بعدی
                        _ = PlayNextAsync();
                        _isTimerRunning = false;
                        return false;
                    }

                    return true;
                });
            }
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

        private async void OnPreviousClicked(object sender, EventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                PlaybackCapsule.SaveSliderPosition(0);
                await PlayNextAsync();
            }
        }

        private void OnPlayClicked(object sender, EventArgs e)
        {
            var audioService = DependencyService.Get<IAudioService>();
            if (audioService.IsPlaying())
            {
                audioService.Pause();
                btnPlay.ImageSource = "icon_play1";
            }
            else
            {
                audioService.Resume();
                btnPlay.ImageSource = "icon_pause1";
            }
        }

        private async void OnNextClicked(object sender, EventArgs e)
        {
            if (_currentIndex < _playableItems.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }

            PlaybackCapsule.SaveSliderPosition(0);
            await PlayNextAsync();
        }
    }
}
