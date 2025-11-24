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
using Rg.Plugins.Popup.Services;
using static System.Net.Mime.MediaTypeNames;

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
            InitializeComponent();
            _audioService = DependencyService.Get<IAudioService>();

            // فقط آهنگ‌های دانلود شده
            _playableItems = PlaybackCapsule.CurrentPlaylist?
                .Where(item =>
                {
                    string audioFileName = $"{item.RepeatId! ?? item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                    return System.IO.File.Exists(audioPath);
                })
                .OrderBy(item => item.Id)
                .ToList() ?? new List<Detail>();

            _currentIndex = _playableItems.FindIndex(x => x.IsPlay);
            LoadLastPlaybackInfo();

            var current = _playableItems.FirstOrDefault(x => x.IsPlay);
            if (current != null)
                imgMiniCover.Source = GetAlbumArt(current.Href, current.TagImageSrc);
        }

        public MiniPlayerView(bool isMaximize, int currentIndex, List<Detail> details)
        {
            InitializeComponent();
            _audioService = DependencyService.Get<IAudioService>();

            MaximizedPanel.IsVisible = true;
            BlurBackground.IsVisible = true;
            MiniPlayerFrame.IsVisible = false;

            // فقط فایل‌های دانلودشده
            details = details
                .Where(item =>
                {
                    string audioFileName = $"{item.RepeatId! ?? item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                    return System.IO.File.Exists(audioPath);
                }).OrderByDescending(item =>
                {
                    //string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                    string audioFileName = $"{item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                    return System.IO.File.Exists(audioPath);
                })
                .ThenByDescending(item => item.Id)
                .ToList();

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
            BlurBackground.Source = PlaybackCapsule.LoadCurrentImageTag();

            bool isPlaying = _audioService.IsPlaying();
            btnMiniPlay.Source = isPlaying ? "icon_pause1" : "icon_play1";
            btnPlay.Source = isPlaying ? "icon_pause1" : "icon_play1";
        }

        private async Task PlayNextAsync(string direction = "")
        {
            if (_currentIndex < 0 || _currentIndex >= _playableItems.Count)
                return;

            var item = _playableItems[_currentIndex];
            _playableItems.ForEach(d => d.IsPlay = false);
            item.IsPlay = true;

            PlaybackCapsule.CurrentPlaylist = _playableItems;

            string audioFileName = $"{item.RepeatId! ?? item.Id}{Path.GetExtension(item.Href)}";
            string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);

            // فقط آهنگ‌هایی که دانلود شدن
            if (!System.IO.File.Exists(audioPath))
            {
                DependencyService.Get<IToastService>()?.Show("این آهنگ هنوز دانلود نشده است.");
                return;
            }

            item.Href = audioPath;
            PlaybackCapsule.SaveCurrentUrl(item.Href);

            Device.BeginInvokeOnMainThread(() =>
            {
                var imgCover = GetAlbumArt(item.Href, item.TagImageSrc);
                CoverImage.Source = imgCover;
                imgMiniCover.Source = imgCover;
                BlurBackground.Source = imgCover;
                BlurBackground.Transformations = new List<ITransformation> { new BlurredTransformation(10) };

                lblTrackNumber.Text = $"{_currentIndex + 1}/{_playableItems.Count}";
                lblTitle.Text = item.Title;
                lblMiniTitle.Text = item.Title;
                btnPlay.Source = "icon_pause1";
                btnMiniPlay.Source = "icon_pause1";
            });

            double resumePosition = 0;
            if (direction == "")
            {
                double.TryParse(PlaybackCapsule.LoadSeconds(), out resumePosition);
            }
            else
            {
                resumePosition = 0;
            }
            _audioService.Play(item.Href, item.Title, item.TagImageSrc, resumePosition, _currentIndex);

            Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
            {
                DependencyService.Get<IEqualizerService>().InitAudioEffects();
                DependencyService.Get<IEqualizerService>().Init();
                DependencyService.Get<IEqualizerService>().InitAudioEffects2();
                return false;
            });

            //int index = int.Parse(((Slider)s).AutomationId.Replace("band_", ""));
            //short newLevel = (short)e.NewValue;
            //_audioService.SetBandLevel(index, newLevel);
            // تایمر زمان پخش
            if (!_isTimerRunning)
            {
                _isTimerRunning = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    if (!_isTimerRunning)
                        return false;

                    var duration = _audioService.GetDurationSeconds();
                    var position = _audioService.GetCurrentPositionSeconds();

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (duration > 0)
                        {
                            ProgressSlider.Maximum = duration;
                            ProgressSlider.Value = position;
                            PlaybackCapsule.SaveSliderPosition(position);
                            CurrentTimeLabel.Text = TimeSpan.FromSeconds(position).ToString(@"m\:ss");
                            TotalTimeLabel.Text = TimeSpan.FromSeconds(duration - position).ToString(@"m\:ss");
                        }

                        if (duration > 0 && position >= duration - 0.5)
                        {
                            _isTimerRunning = false; // اول تایمر فعلی را قطع کن

                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                if (_currentIndex < _playableItems.Count - 1)
                                {
                                    _currentIndex++;
                                }
                                else
                                {
                                    _currentIndex = 0; // برگشت به اولین آهنگ
                                }

                                await PlayNextAsync("+");
                            });

                            return; // توقف تایمر فعلی
                        }
                    });

                    return true;
                });
            }
        }

        private async void OnMiniPlayClicked(object sender, EventArgs e)
        {
            bool isPlaying = _audioService.IsPlaying();
            if (isPlaying)
            {
                _audioService.Pause();
                btnMiniPlay.Source = "icon_play1";
                btnPlay.Source = "icon_play1";
            }
            else
            {
                //_audioService.Resume();
                await PlayNextAsync();
                btnMiniPlay.Source = "icon_pause1";
                btnPlay.Source = "icon_pause1";
            }
        }

        private async void OnNextClicked(object sender, EventArgs e)
        {
            if (_currentIndex < _playableItems.Count - 1)
                _currentIndex++;
            else
                _currentIndex = 0;

            await PlayNextAsync("+");
        }

        private async void OnPreviousClicked(object sender, EventArgs e)
        {
            if (_currentIndex > 0)
                _currentIndex--;
            else
                _currentIndex = _playableItems.Count - 1;

            await PlayNextAsync("-");
        }

        private void OnPlayClicked(object sender, EventArgs e)
        {
            if (_audioService.IsPlaying())
            {
                _audioService.Pause();
                btnPlay.Source = "icon_play1";
            }
            else
            {
                _audioService.Resume();
                btnPlay.Source = "icon_pause1";
            }
        }

        private Xamarin.Forms.ImageSource GetAlbumArt(string mp3Path, string myTag)
        {
            try
            {
                if (!string.IsNullOrEmpty(myTag))
                {
                    if (myTag.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        return Xamarin.Forms.ImageSource.FromUri(new Uri(myTag));
                    else
                        return Xamarin.Forms.ImageSource.FromFile(myTag);
                }

                if (!System.IO.File.Exists(mp3Path))
                    return null;

                var file = TagLib.File.Create(mp3Path);
                var picture = file.Tag.Pictures.FirstOrDefault();
                if (picture != null)
                {
                    var imageBytes = picture.Data.Data;
                    return Xamarin.Forms.ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
            }
            catch { }
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

        private async void OnMiniPlayerTapped(object sender, EventArgs e)
        {
            var audioService = DependencyService.Get<IAudioService>();
            double currentPosition = audioService.GetCurrentPositionSeconds();
            PlaybackCapsule.SaveSliderPosition(currentPosition); // ✅ ذخیره موقعیت فعلی
            //BlurBackground.IsVisible = false;

            //var c = _playableItems.FirstOrDefault(x => x.IsPlay);
            //_currentIndex = _playableItems.FindIndex(x => x.IsPlay);
            _playableItems = PlaybackCapsule.CurrentPlaylist;
            _currentIndex = PlaybackCapsule.CurrentIndex;

            var maximizedView = new MiniPlayerView(true, _currentIndex, _playableItems);
            var page = new ContentPage { Content = maximizedView };
            NavigationPage.SetHasNavigationBar(page, false);
            await Navigation.PushAsync(page);
        }

        private async void OnEqualizerClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new EqualizerPage());
        }

        private async void OnPlaylistClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DetailPages());
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            string message = $"آیا مایل هستید آهنگ «{lblTitle.Text}» را حذف کنید؟";
            var popup = new ConfirmPopup(message);
            await PopupNavigation.Instance.PushAsync(popup);
            bool confirm = await popup.ShowAsync();
            if (!confirm)
                return;

            string audioFileName = PlaybackCapsule.LoadHref();
            string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);

            if (System.IO.File.Exists(audioPath))
            {
                System.IO.File.Delete(audioPath);
                await Navigation.PushAsync(new DetailPages());
                //item.IsDeleteVisible = false;  // اگه دکمه داری برای مخفی کردن
            }
            else
            {

            }
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
    }
}
