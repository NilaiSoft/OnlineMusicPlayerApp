using OnlineMusicPlayerApp.Models;
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
        public MiniPlayerView()
        {
            InitializeComponent();
            LoadLastPlaybackInfo();

            //MiniPlayerFrame.IsVisible = (//PlaybackCapsule.LoadCurrentAudioParentIds() != 0);

            //Device.BeginInvokeOnMainThread(async () =>
            //{
            //    _playableItems = new List<Detail>();
            //    var categories = await DependencyService.Get<IPlayListServices>().GetCategoriesFromJson();

            //    _playableItems = categories
            //        .SelectMany(c => c.Details)
            //        .Where(d => d.Children != null)
            //        .SelectMany(d => d.Children)
            //        .Where(child => child.ParentId == //PlaybackCapsule.LoadCurrentAudioParentIds())
            //        .ToList();
            //});
        }

        public MiniPlayerView(bool isMaximize, int currentIndex, List<Detail> details)
        {
            InitializeComponent();
            MaximizedPanel.IsVisible = true;
            MiniPlayerFrame.IsVisible = false;
            _playableItems = details;
            _currentIndex = currentIndex;
            LoadLastPlaybackInfo();
            Task.Run(async () =>
            {
                await PlayNextAsync();
            });
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
        }

        private void LoadLastPlaybackInfo()
        {
            lblMiniTitle.Text = PlaybackCapsule.LoadCurrentTitleMusic();
            imgMiniCover.Source = PlaybackCapsule.LoadCurrentImageTag();
            //CoverImage.Source = //PlaybackCapsule.LoadCurrentImageTag();
            //_currentIndex = //PlaybackCapsule.LoadLastIndexs();

            //_playableItems= _playableItems.Any()? _playableItems:
            var audioService = DependencyService.Get<IAudioService>();
            btnMiniPlay.Source = audioService.IsPlaying() ? "icon_pause1" : "icon_play1";
            btnPlay.ImageSource = audioService.IsPlaying() ? "icon_pause1" : "icon_play1";
        }

        private void OnMiniPlayClicked(object sender, EventArgs e)
        {
            var audioService = DependencyService.Get<IAudioService>();

            if (audioService.IsPlaying())
            {
                audioService.Pause();
                btnMiniPlay.Source = "icon_play1";
                btnPlay.ImageSource = "icon_play1";
            }
            else
            {
                btnMiniPlay.Source = "icon_pause1";
                btnPlay.ImageSource = "icon_pause1";
                audioService.Resume();
            }
        }

        private async void OnMiniPlayerTapped(object sender, EventArgs e)
        {
            var categories = await DependencyService.Get<IPlayListServices>().GetCategoriesFromJson();

            _playableItems = _playableItems.Any() ? _playableItems : categories
                .SelectMany(c => c.Details)
                .Where(d => d.Children != null)
                .SelectMany(d => d.Children)
                .Where(child => child.ParentId == 1)//PlaybackCapsule.LoadCurrentAudioParentIds())
                .ToList();

            var miniPlayer = new MiniPlayerView(true, _currentIndex, _playableItems);
            var page = new ContentPage
            {
                Content = miniPlayer
            };

            await Navigation.PushAsync(page);
        }

        private async Task PlayNextAsync()
        {
            if (_currentIndex < 0 || _currentIndex >= _playableItems.Count)
                return;

            var item = _playableItems[_currentIndex];

            if (item == null)
                return;

            PlaybackCapsule.SaveCurrentTagImage(item.TagImageSrc);
            PlaybackCapsule.SaveTitle(item.Title);
            //PlaybackCapsule.SaveCurrentUrl(item.Href);
            //PlaybackCapsule.SaveCurrentAudioParentId(item.ParentId);
            //PlaybackCapsule.SaveLastIndex(_currentIndex);

            // 🎯 نمایش شماره ترک به‌صورت 1/2
            lblTrackNumber.Text = $"{_currentIndex + 1}/{_playableItems.Count}";

            var audioService = DependencyService.Get<IAudioService>();
            lblTitle.Text = item.Title;

            var imageTagPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), $"{item.ParentId}_{item.Id}{Path.GetExtension(item.TagImageSrc)}");
            if (File.Exists(imageTagPath))
                item.TagImageSrc = imageTagPath;
            else
                item.TagImageSrc = await DependencyService.Get<IGoogleDriveServices>()
                .DownloadGoogleDriveFileAsync(item.TagImageSrc, $"{item.ParentId}_{item.Id}{Path.GetExtension(item.TagImageSrc)}");

            CoverImage.Source = string.IsNullOrEmpty(item.TagImageSrc) ? PlaybackCapsule.LoadCurrentImageTag() : item.TagImageSrc;
            imgMiniCover.Source = CoverImage.Source;
            lblMiniTitle.Text = item.Title;
            lblTitle.Text = item.Title;

            double resumePosition = 0;
            //double.TryParse(//PlaybackCapsule.LoadSeconds(), out resumePosition);

            //if (!audioService.IsPlaying())
            //{
            //    btnPlay.ImageSource = "icon_pause1";
            //    audioService.Play(item.Href, resumePosition);
            //}

            btnPlay.ImageSource = "icon_pause1";

            var audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}");

            if (File.Exists(audioPath))
                item.Href = audioPath;
            else
                item.Href = await DependencyService.Get<IGoogleDriveServices>()
                .DownloadGoogleDriveFileAsync(item.Href, $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}");

            audioService.Play(item.Href, resumePosition);

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
                    //PlaybackCapsule.SaveSliderPosition(position);
                    CurrentTimeLabel.Text = TimeSpan.FromSeconds(position).ToString(@"m\:ss");
                }


                if (position >= duration - 1 && duration > 0)
                {
                    _currentIndex++;
                    _ = PlayNextAsync();
                    return false; // توقف تایمر
                }

                return true; // ادامه تایمر
            });

            item = new Detail();
        }

        private void ProgressSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (Math.Abs(e.NewValue - DependencyService.Get<IAudioService>().GetCurrentPositionSeconds()) > 1)
            {
                long newPositionMs = (long)(e.NewValue * 1000);
                DependencyService.Get<IAudioService>().SeekTo(newPositionMs);
            }
        }

        private async void OnPreviousClicked(object sender, EventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                await PlayNextAsync();
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

        private async void OnNextClicked(object sender, EventArgs e)
        {
            if (_currentIndex < _playableItems.Count - 1)
            {
                _currentIndex++;
                await PlayNextAsync();
            }
            else

            {
                _currentIndex = 0;
                await PlayNextAsync();
            }
        }
    }
}
