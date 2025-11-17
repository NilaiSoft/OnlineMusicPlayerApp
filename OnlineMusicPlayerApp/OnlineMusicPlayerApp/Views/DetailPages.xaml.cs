using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using OnlineMusicPlayerApp.Models;
using OnlineMusicPlayerApp.Services.PlayListServices;
using System.Threading.Tasks;
using OnlineMusicPlayerApp.Extensions;
using OnlineMusicPlayerApp.Services;
using Rg.Plugins.Popup.Services;

namespace OnlineMusicPlayerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailPages : ContentPage
    {
        private List<Detail> playableItems;

        public DetailPages()
        {
            InitializeComponent();
            playableItems = new List<Detail>();
            DetailsListView_Refreshing(null, null);
        }

        private async void LoadData()
        {
            DetailsListView.ItemsSource = null;
            var details = new List<Detail>();

            await FormExtensions.ShowBuildInfoModalAsync(this.Navigation, async () =>
            {
                var categories = await DependencyService.Get<IPlayListServices>().GetCategoriesFromJson();

                details = categories.Select(x => new Detail
                {
                    Title = x.Master,
                    Children = x.Details,
                    ListImageSrc = x.AlbumImageSrc
                }).ToList();

                playableItems = Flatten(details);
                DetailsListView.ItemsSource = details;
            });
        }

        private void DetailsListView_Refreshing(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => LoadData());
            DetailsListView.IsRefreshing = false;
        }

        public DetailPages(List<Detail> details)
        {
            InitializeComponent();
            playableItems = Flatten(details);

            details = details
                .OrderByDescending(item =>
                {
                    string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                    return File.Exists(audioPath);
                })
                .ThenBy(item => item.Id)
                .ToList();

            //foreach (var item in details)
            //{
            //    string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
            //    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);

            //    // اگر فایل وجود داشت → دکمه حذف فعال شود
            //    item.IsDeleteVisible = File.Exists(audioPath);
            //}

            DetailsListView.ItemsSource = details;
        }

        private async void DetailsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null; // برای پاک کردن انتخاب

            if (e.Item is Detail item)
            {
                item.Children = item.Children?.Where(x => x.IsVisible).ToList();

                if (item.Children != null && item.Children.Any())
                {
                    await Navigation.PushAsync(new DetailPages(item.Children));
                    return;
                }

                string extension = Path.GetExtension(item.Href);
                if (!new[] { ".mp3", ".mp4" }.Contains(extension))
                    return;

                // 🔽 دانلود در اینجا انجام می‌شود
                string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);

                if (!File.Exists(audioPath))
                {
                    string message = $"آیا مایل هستید آهنگ «{item.Title}» را دانلود کنید؟";
                    var popup = new ConfirmPopup(message);
                    await PopupNavigation.Instance.PushAsync(popup);
                    bool confirm = await popup.ShowAsync();

                    if (!confirm)
                        return;

                    var progressHandler = new Progress<double>(p =>
                    {
                        item.DownloadProgress = p;
                        item.DownloadStatus = $"در حال دانلود {Math.Round(p * 100)}٪";
                    });

                    // شروع دانلود
                    item.IsDownloading = true;
                    item.DownloadStatus = "در حال آماده‌سازی...";
                    item.DownloadProgress = 0;

                    try
                    {
                        item.Href = await DependencyService.Get<IGoogleDriveServices>()
                            .DownloadGoogleDriveFileWithProgressAsync(item.Href, audioFileName, progressHandler);

                        item.DownloadStatus = "دانلود کامل شد ✅";
                        item.IsDownloading = false;
                        item.DownloadProgress = 1;
                    }
                    catch (Exception ex)
                    {
                        item.DownloadStatus = "خطا در دانلود ❌";
                        item.IsDownloading = false;
                    }
                }
                else
                {
                    item.Href = audioPath;
                }

                var currentItem = playableItems.FirstOrDefault(x => x.Id == item.Id);
                currentItem.Href = item.Href;

                // تنظیم لیست و اجرای MiniPlayer
                playableItems = playableItems
                    .OrderByDescending(x =>
                    {
                        string fileName = $"{x.ParentId}_{x.Id}{Path.GetExtension(x.Href)}";
                        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), fileName);
                        return File.Exists(path);
                    })
                    .ThenBy(x => x.Id)
                    .ToList();

                int index = playableItems.FindIndex(x => x.Id == item.Id);
                PlaybackCapsule.CurrentPlaylist = playableItems;
                PlaybackCapsule.CurrentIndex = index;

                var miniPlayer = new MiniPlayerView(true, index, playableItems);
                var page = new ContentPage { Content = miniPlayer };
                NavigationPage.SetHasNavigationBar(page, false);
                await Navigation.PushAsync(page);
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

            flat = flat
                .OrderByDescending(item =>
                {
                    string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                    return File.Exists(audioPath);
                })
                .ThenBy(item => item.Id)
                .ToList();

            return flat;
        }

        private async void OnItemMenuClicked(object sender, EventArgs e)
        {
            var btn = sender as ImageButton;
            if (btn?.CommandParameter is Detail item)
            {
                string action = await DisplayActionSheet(
                    $"گزینه‌های «{item.Title}»",
                    "انصراف", null,
                    "حذف فایل دانلود شده",
                    "اطلاعات آهنگ");

                if (action == "حذف فایل دانلود شده")
                {
                    string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);

                    if (File.Exists(audioPath))
                    {
                        File.Delete(audioPath);
                        await DisplayAlert("حذف شد", "فایل پاک شد.", "باشه");
                    }
                    else
                    {
                        await DisplayAlert("خطا", "فایلی برای حذف وجود ندارد.", "باشه");
                    }
                }
                else if (action == "اطلاعات آهنگ")
                {
                    await DisplayAlert("اطلاعات", $"نام: {item.Title}", "باشه");
                }
            }
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {

        }
    }
}
