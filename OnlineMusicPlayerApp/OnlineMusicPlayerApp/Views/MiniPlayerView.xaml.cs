using System;
using Xamarin.Forms;
using OnlineMusicPlayerApp.Extensions;
using OnlineMusicPlayerApp.Services;
using OnlineMusicPlayerApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnlineMusicPlayerApp.Services.PlayListServices;

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

            Device.BeginInvokeOnMainThread(async () =>
            {
                _playableItems = new List<Detail>();
                var categories = await DependencyService.Get<IPlayListServices>().GetCategoriesFromJson();

                _playableItems = categories
                    .SelectMany(c => c.Details)
                    .Where(d => d.Children != null)
                    .SelectMany(d => d.Children)
                    .Where(child => child.ParentId == 1)
                    .ToList();
            });
        }

        public MiniPlayerView(bool isMaximize, int currentIndex, List<Detail> details)
        {
            InitializeComponent();
            MaximizedPanel.IsVisible = true;
            MiniPlayerFrame.IsVisible = false;
            _playableItems = details;
            _currentIndex = currentIndex;
            PlayNext();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
        }

        private void LoadLastPlaybackInfo()
        {
            lblMiniTitle.Text = PlaybackCapsule.LoadCurrentTitle();
            imgMiniCover.Source = PlaybackCapsule.LoadCurrentImageTag();
            //_playableItems= _playableItems.Any()? _playableItems:
            var audioService = DependencyService.Get<IAudioService>();
            btnMiniPlay.Source = audioService.IsPlaying() ? "icon_pause1" : "icon_play1";
        }

        private void OnMiniPlayClicked(object sender, EventArgs e)
        {
            var audioService = DependencyService.Get<IAudioService>();

            btnMiniPlay.Source = audioService.IsPlaying() ? "icon_pause1" : "icon_play1";

            if (audioService.IsPlaying())
            {
                audioService.Pause();
                return;
            }

            string lastHref = PlaybackCapsule.LoadHref();
            if (string.IsNullOrEmpty(lastHref))
                return;

            double resumePosition = 0;
            double.TryParse(PlaybackCapsule.LoadSeconds(), out resumePosition);

            btnMiniPlay.Source = "icon_pause1";
            audioService.Play(lastHref, resumePosition);

        }

        private void OnMiniNextClicked(object sender, EventArgs e)
        {
            OnNextClicked(sender, e);
        }

        private void OnMiniPrevClicked(object sender, EventArgs e)
        {
            OnPreviousClicked(sender, e);
        }

        private async void OnMiniPlayerTapped(object sender, EventArgs e)
        {
            //var index = _playableItems.FindIndex(x => x.Id == 0);
            var miniPlayer = new MiniPlayerView(true, _currentIndex, _playableItems);
            var page = new ContentPage
            {
                Content = miniPlayer
            };

            await Navigation.PushAsync(page);
        }

        private void PlayNext()
        {
            if (_currentIndex < 0 || _currentIndex >= _playableItems.Count)
                return;

            var item = _playableItems[_currentIndex];

            if (item == null)
                return;

            lblTitle.Text = item.Title;
            CoverImage.Source = item.TagImageSrc;
            PlaybackCapsule.SaveCurrentTagImage(item.TagImageSrc);
            PlaybackCapsule.SaveTitle(item.Title);
            PlaybackCapsule.SaveCurrentUrl(item.Href);
            PlaybackCapsule.SaveCurrentAudioParentId(item.ParentId);
            CoverImage.IsVisible = true;

            // 🎯 نمایش شماره ترک به‌صورت 1/2
            lblTrackNumber.Text = $"{_currentIndex + 1}/{_playableItems.Count}";

            btnPlay.ImageSource = "icon_pause1";
            var audioService = DependencyService.Get<IAudioService>();
            CoverImage.Source = item.TagImageSrc;
            imgMiniCover.Source = item.TagImageSrc;
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
                    _currentIndex++;
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
            if (_currentIndex > 0)
            {
                _currentIndex--;
                PlayNext();
            }
        }

        private void OnPlayClicked(object sender, EventArgs e)
        {

        }

        private void OnNextClicked(object sender, EventArgs e)
        {
            if (_currentIndex < _playableItems.Count - 1)
            {
                _currentIndex++;
                PlayNext();
            }
        }
    }
}
