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
        private IAudioService _audioService;
        public MiniPlayerView()
        {
            _playableItems = PlaybackCapsule.CurrentPlaylist;

            InitializeComponent();
            _audioService = DependencyService.Get<IAudioService>();

            _playableItems = _playableItems
            .OrderByDescending(item =>
            {
                string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                return System.IO.File.Exists(audioPath);
            })
            .ThenBy(item => item.Id) // سپس مرتب‌سازی بر اساس Id
            .ToList();

            var c = _playableItems.FirstOrDefault(x => x.IsPlay);
            _currentIndex = _playableItems.FindIndex(x => x.IsPlay);
            LoadLastPlaybackInfo();
            imgMiniCover.Source = GetAlbumArt(PlaybackCapsule.LoadHref(), c?.TagImageSrc);
        }

        public MiniPlayerView(bool isMaximize, int currentIndex, List<Detail> details)
        {
            InitializeComponent();
            _audioService = DependencyService.Get<IAudioService>();
            MaximizedPanel.IsVisible = true;
            MiniPlayerFrame.IsVisible = false;

            details = details
                .OrderByDescending(item =>
                {
                    string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                    return System.IO.File.Exists(audioPath);
                })
                .ThenBy(item => item.Id) // سپس مرتب‌سازی بر اساس Id
                .ToList();


            details.ForEach(d => d.IsPlay = false);

            var current = details[currentIndex];
            var c = details.FirstOrDefault(x => x.Id == current.Id);
            c.IsPlay = true;

            _playableItems = details;
            _currentIndex = currentIndex;
            PlaybackCapsule.CurrentPlaylist = details;
            PlaybackCapsule.CurrentIndex = currentIndex;
            //MessagingCenter.Send<object>(this, "PlaylistUpdated");

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

        public void ResetPage()
        {
            var c = _playableItems[_currentIndex];
            lblMiniTitle.Text = c.Title;
            imgMiniCover.Source = GetAlbumArt(c.Href, c.TagImageSrc);
            BlurBackground.Source = imgMiniCover.Source;

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
            MaximizedPanel.IsVisible = false;
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
            //BlurBackground.IsVisible = false;

            _playableItems = _playableItems
                .OrderByDescending(item =>
                {
                    string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                    return System.IO.File.Exists(audioPath);
                })
                .ThenBy(item => item.Id) // سپس مرتب‌سازی بر اساس Id
                .ToList();

            var c = _playableItems.FirstOrDefault(x => x.IsPlay);
            _currentIndex = _playableItems.FindIndex(x => x.IsPlay);

            var maximizedView = new MiniPlayerView(true, _currentIndex, _playableItems);
            var page = new ContentPage { Content = maximizedView };
            NavigationPage.SetHasNavigationBar(page, false);
            await Navigation.PushAsync(page);
        }

        //private async void OnMiniPlayerTapped(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // گرفتن سرویس صوتی و موقعیت فعلی پخش
        //        var audioService = DependencyService.Get<IAudioService>();
        //        double currentPosition = audioService.GetCurrentPositionSeconds();
        //        PlaybackCapsule.SaveSliderPosition(currentPosition); // ذخیره موقعیت فعلی

        //        // مرتب‌سازی آیتم‌ها: اول فایل‌های موجود، سپس بر اساس Id
        //        _playableItems = _playableItems
        //            .OrderByDescending(item =>
        //            {
        //                string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
        //                string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
        //                return System.IO.File.Exists(audioPath);
        //            })
        //            .ThenBy(item => item.Id)
        //            .ToList();

        //        // پیدا کردن آیتم در حال پخش و ایندکس آن
        //        var currentItem = _playableItems.FirstOrDefault(x => x.IsPlay);
        //        _currentIndex = _playableItems.FindIndex(x => x.IsPlay);

        //        // ساخت نمای بزرگ‌شده‌ی پلیر
        //        var maximizedView = new MiniPlayerView(true, currentIndex: _currentIndex, _playableItems);
        //        var contentPage = new ContentPage { Content = maximizedView };

        //        // حذف نوار ناوبری
        //        NavigationPage.SetHasNavigationBar(contentPage, false);

        //        // باز کردن صفحه به صورت مودال تمام‌صفحه
        //        await Navigation.PushModalAsync(new NavigationPage(contentPage));
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine("خطا در باز کردن پلیر: " + ex.Message);
        //        await Application.Current.MainPage.DisplayAlert("خطا", "امکان باز کردن پلیر وجود ندارد.", "باشه");
        //    }
        //}

        private async Task PlayNextAsync(string e = "")
        {
            if (_currentIndex < 0 || _currentIndex >= _playableItems.Count)
                return;

            _playableItems = _playableItems
                .OrderByDescending(item =>
                {
                    string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                    return System.IO.File.Exists(audioPath);
                })
                .ThenBy(item => item.Id) // سپس مرتب‌سازی بر اساس Id
                .ToList();

            var item = _playableItems[_currentIndex];

            _playableItems.ForEach(d => d.IsPlay = false);
            var current = _playableItems[_currentIndex];
            var c = _playableItems.FirstOrDefault(x => x.Id == current.Id);
            c.IsPlay = true;


            //var item = PlaybackCapsule.CurrentPlaylist.FirstOrDefault(x => x.IsPlay);
            if (item == null) return;

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
                    if (e == "+")
                        _currentIndex--;
                    if (e == "-")
                        _currentIndex++;
                    //btnMiniPlay.Source = "icon_play1";
                    //btnPlay.Source = "icon_play1";
                    //await PlayNextAsync();
                    return;
                }
                //_audioService.SeekTo(0);
                PlaybackCapsule.CurrentIndex = _currentIndex;
                PlaybackCapsule.CurrentTitle = item.Title;
                PlaybackCapsule.SaveCurrentAudioParentId(item.ParentId.ToString());


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
                        DownloadMessageLabel.Text = $"دانلود {Math.Round(p * 100)}٪ - بارگذاری {item.Title}";
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
                var imgCover = GetAlbumArt(item.Href, item.TagImageSrc);
                CoverImage.Source = imgCover;
                imgMiniCover.Source = imgCover;
                BlurBackground.Source = imgCover;

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
                // ⏱️ نوار زمان و پخش خودکار آهنگ بعدی
                _isTimerRunning = false; // اطمینان از ریست قبل از شروع

                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    var audioService = DependencyService.Get<IAudioService>();
                    var duration = audioService.GetDurationSeconds();
                    var position = audioService.GetCurrentPositionSeconds();

                    if (!_isTimerRunning)
                        return false;

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
                            _isTimerRunning = false;
                            _currentIndex++;
                            PlaybackCapsule.SaveSliderPosition(0);
                            _ = PlayNextAsync();
                        }
                    });

                    return true;
                });

                _isTimerRunning = true;
            }
        }

        private Xamarin.Forms.ImageSource GetAlbumArt(string mp3Path, string myTag)
        {
            try
            {
                //// اگر myTag مقدار داشت، همون تصویر رو برگردون
                //if (!string.IsNullOrEmpty(myTag))
                //{
                //    return Xamarin.Forms.ImageSource.FromFile(myTag);
                //}
                // اگر myTag مقدار داشت و آدرس اینترنتی بود، تصویر از وب لود بشه
                if (!string.IsNullOrEmpty(myTag))
                {
                    if (myTag.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        return Xamarin.Forms.ImageSource.FromUri(new Uri(myTag));
                    }
                    else
                    {
                        return Xamarin.Forms.ImageSource.FromFile(myTag);
                    }
                }

                // اگر myTag خالی بود، تصویر تگ داخل فایل MP3 رو بخون
                if (!System.IO.File.Exists(mp3Path))
                    return null;

                var file = TagLib.File.Create(mp3Path);
                var picture = file.Tag.Pictures.FirstOrDefault(x => x.Data != null);

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
            //var button = sender as ImageButton;
            //var parameter = button?.CommandParameter;

            //if (parameter != null)
            //{
            //    if (parameter == "minPreview")
            //    {
            //        BlurBackground.IsVisible = false;
            //        MaximizedPanel.IsVisible = false;
            //    }
            //}

            if (_currentIndex > 0)
            {
                _currentIndex--;
                PlaybackCapsule.SaveSliderPosition(0);
                await PlayNextAsync("-");
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
            //var button = sender as ImageButton;
            //var parameter = button?.CommandParameter;

            //if (parameter != null)
            //{
            //    if(parameter== "minNext")
            //    {
            //        BlurBackground.IsVisible = false;
            //        MaximizedPanel.IsVisible = false;
            //    }
            //}

            if (_currentIndex < _playableItems.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }

            PlaybackCapsule.SaveSliderPosition(0);
            await PlayNextAsync("+");
        }

        public void RefreshMiniPlayerFrame()
        {
            _playableItems = PlaybackCapsule.CurrentPlaylist;
            _currentIndex = PlaybackCapsule.CurrentIndex;

            _playableItems = _playableItems
                .OrderByDescending(item =>
                {
                    string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                    return System.IO.File.Exists(audioPath);
                })
                .ThenBy(item => item.Id) // سپس مرتب‌سازی بر اساس Id
                .ToList();

            if (_playableItems == null || !_playableItems.Any() || _currentIndex < 0 || _currentIndex >= _playableItems.Count)
                return;

            var item = _playableItems[_currentIndex];

            lblMiniTitle.Text = item.Title;
            lblTrackNumber.Text = $"{_currentIndex + 1}/{_playableItems.Count}";

            var imageSource = string.IsNullOrEmpty(item.TagImageSrc)
                ? GetAlbumArt(item.Href, "")
                : item.TagImageSrc;

            imgMiniCover.Source = imageSource;

            var audioService = DependencyService.Get<IAudioService>();
            bool isPlaying = audioService?.IsPlaying() ?? false;

            btnMiniPlay.Source = isPlaying ? "icon_pause1" : "icon_play1";
            btnMiniPrev.Source = "icon_back";
            btnMiniNext.Source = "icon_next";

            MiniPlayerFrame.IsVisible = true;
        }
    }
}
