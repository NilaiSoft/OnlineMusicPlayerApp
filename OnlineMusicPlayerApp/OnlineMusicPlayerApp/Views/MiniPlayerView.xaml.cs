using FFImageLoading.Work;
using OnlineMusicPlayerApp.Models;
using OnlineMusicPlayerApp.Services;
using OnlineMusicPlayerApp.Services.PlayListServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TagLib;
using Xamarin.Forms;
using FFImageLoading.Transformations;
using FFImageLoading.Work;


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
            var c = _playableItems.FirstOrDefault(x => x.IsPlay);
            _currentIndex = _playableItems.FindIndex(x => x.IsPlay);
            LoadLastPlaybackInfo();
            imgMiniCover.Source = GetAlbumArt(PlaybackCapsule.LoadHref(), "");
        }

        public MiniPlayerView(bool isMaximize, int currentIndex, List<Detail> details)
        {
            InitializeComponent();
            MaximizedPanel.IsVisible = true;
            MiniPlayerFrame.IsVisible = false;

            details.ForEach(d => d.IsPlay = false);

            var current = details[currentIndex];
            var c = details.FirstOrDefault(x => x.Id == current.Id);
            c.IsPlay = true;

            _playableItems = details;

            PlaybackCapsule.CurrentPlaylist = details;
            PlaybackCapsule.CurrentIndex = currentIndex;

            LoadLastPlaybackInfo();
            Task.Run(async () => await PlayNextAsync());
        }

        private void LoadLastPlaybackInfo()
        {
            lblMiniTitle.Text = PlaybackCapsule.LoadCurrentTitleMusic();
            imgMiniCover.Source = PlaybackCapsule.LoadCurrentImageTag();
            BlurBackground.Source = PlaybackCapsule.LoadCurrentImageTag();

            var audioService = DependencyService.Get<IAudioService>();
            bool isPlaying = audioService.IsPlaying();
            btnMiniPlay.Source = isPlaying ? "icon_pause1" : "icon_play1";
            btnPlay.Source = isPlaying ? "icon_pause1" : "icon_play1";
        }

        private async void OnMiniPlayClicked(object sender, EventArgs e)
        {
            var audioService = DependencyService.Get<IAudioService>();
            bool isPlaying = audioService.IsPlaying();
            BlurBackground.IsVisible = false;
            if (isPlaying)
            {
                audioService.Pause();
                btnMiniPlay.Source = "icon_play1";
                btnPlay.Source = "icon_play1";
            }
            else
            {
                //audioService.Resume();
                await PlayNextAsync();
                //if (isActive == null)
                //{
                //    double currentPosition = audioService.GetCurrentPositionSeconds();
                //    PlaybackCapsule.SaveSliderPosition(currentPosition); // ✅ ذخیره موقعیت فعلی
                //    LoadLastPlaybackInfo();
                //    Task.Run(async () => await PlayNextAsync());
                //}

                btnMiniPlay.Source = "icon_pause1";
                btnPlay.Source = "icon_pause1";
            }
        }

        private async void OnMiniPlayerTapped(object sender, EventArgs e)
        {
            var audioService = DependencyService.Get<IAudioService>();
            double currentPosition = audioService.GetCurrentPositionSeconds();
            PlaybackCapsule.SaveSliderPosition(currentPosition); // ✅ ذخیره موقعیت فعلی

            var c = _playableItems.FirstOrDefault(x => x.IsPlay);
            _currentIndex = _playableItems.FindIndex(x => x.IsPlay);

            var maximizedView = new MiniPlayerView(true, _currentIndex, _playableItems);
            var page = new ContentPage { Content = maximizedView };
            NavigationPage.SetHasNavigationBar(page, false);
            await Navigation.PushAsync(page);
        }

        private async Task PlayNextAsync()
        {
            //if (_currentIndex < 0 || _currentIndex >= _playableItems.Count)
            //    return;

            var item = _playableItems[_currentIndex];
            //var item = PlaybackCapsule.CurrentPlaylist.FirstOrDefault(x => x.IsPlay);
            if (item == null) return;

            PlaybackCapsule.CurrentIndex = _currentIndex;
            PlaybackCapsule.CurrentTitle = item.Title;
            PlaybackCapsule.SaveCurrentAudioParentId(item.ParentId.ToString());

            string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
            string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);

            // 🎵 دانلود آهنگ با تأیید کاربر
            if (!System.IO.File.Exists(audioPath))
            {
                string message = $"به نظر می‌رسد آهنگ «{item.Title}» هنوز آماده پخش نیست.\nمایلی آن را برایت فراهم کنیم؟";
                var popup = new ConfirmPopup(message);
                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(popup);
                bool confirm = await popup.ShowAsync();

                if (!confirm)
                {
                    return;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    DownloadPanel.IsVisible = true;
                    DownloadProgressBar.Progress = 0;
                    DownloadMessageLabel.Text = $"در حال دریافت «{item.Title}»...";
                });

                var progressHandler = new Progress<double>(p =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DownloadProgressBar.Progress = p;
                        DownloadMessageLabel.Text = $"دانلود {Math.Round(p * 100)}٪ - شاید خاطره‌ای در راه باشد...";
                    });
                });

                item.Href = await DependencyService.Get<IGoogleDriveServices>()
                    .DownloadGoogleDriveFileWithProgressAsync(item.Href, audioFileName, progressHandler);

                Device.BeginInvokeOnMainThread(() =>
                {
                    DownloadPanel.IsVisible = false;
                });
            }
            else
            {
                item.Href = audioPath;
                PlaybackCapsule.SaveCurrentUrl(item.Href);
            }

            if (string.IsNullOrEmpty(item.Href))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DependencyService.Get<IToastService>()?.Show("دانلود آهنگ ناموفق بود.");
                });
                return;
            }

            // 🎧 نمایش تصویر و عنوان
            Device.BeginInvokeOnMainThread(() =>
            {
                CoverImage.Source = string.IsNullOrEmpty(item.TagImageSrc) ? GetAlbumArt(item.Href, "") : item.TagImageSrc;
                imgMiniCover.Source = CoverImage.Source;
                BlurBackground.Source = CoverImage.Source;

                BlurBackground.Transformations = new List<ITransformation>
        {
            new BlurredTransformation(10),
        };

                lblTrackNumber.Text = $"{_currentIndex + 1}/{_playableItems.Count}";
                lblTitle.Text = item.Title;
                lblMiniTitle.Text = item.Title;
                btnPlay.Source = "icon_pause1";
                btnMiniPlay.Source = "icon_pause1";
            });

            // 🎶 پخش آهنگ
            var audioService = DependencyService.Get<IAudioService>();
            double resumePosition = 0;
            double.TryParse(PlaybackCapsule.LoadSeconds(), out resumePosition);
            audioService.Play(item.Href, resumePosition);

            // ⏱️ نوار زمان و پخش خودکار آهنگ بعدی
            if (!_isTimerRunning)
            {
                _isTimerRunning = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    var duration = audioService.GetDurationSeconds();
                    var position = audioService.GetCurrentPositionSeconds();

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (!double.IsNaN(duration) && duration > 0)
                        {
                            ProgressSlider.Maximum = duration;
                        }

                        if (!double.IsNaN(position) && position >= 0 && position <= duration)
                        {
                            ProgressSlider.Value = position;
                            PlaybackCapsule.SaveSliderPosition(position);
                            CurrentTimeLabel.Text = TimeSpan.FromSeconds(position).ToString(@"m\:ss");
                            TotalTimeLabel.Text = TimeSpan.FromSeconds(duration - position).ToString(@"m\:ss");
                        }

                        if (position >= duration - 1 && duration > 0)
                        {
                            _currentIndex++;
                            PlaybackCapsule.SaveSliderPosition(0);
                            _ = PlayNextAsync();
                            _isTimerRunning = false;
                        }
                    });

                    return !(position >= duration - 1 && duration > 0);
                });
            }
        }

        private Xamarin.Forms.ImageSource GetAlbumArt(string mp3Path, string tag)
        {
            try
            {
                var file = TagLib.File.Create(mp3Path);
                var picture = file.Tag.Pictures.FirstOrDefault(x => x.Data != null);

                if (picture == null)
                {
                    file.Tag.Pictures = new IPicture[]
                    {
                            new Picture(tag) // مسیر تصویر
                    };

                    file.Save();
                    return GetAlbumArt(mp3Path, "");
                }

                if (picture != null)
                {
                    var imageBytes = picture.Data.Data;
                    return Xamarin.Forms.ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("خطا در خواندن تصویر تگ: " + ex.Message);
            }

            return null;
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
                btnPlay.Source = "icon_play1";
            }
            else
            {
                audioService.Resume();
                btnPlay.Source = "icon_pause1";
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
