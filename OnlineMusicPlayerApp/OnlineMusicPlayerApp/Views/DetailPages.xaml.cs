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

        // --------------------------------------------------------
        //  Load Data From GoogleSheet Tree (Parent/Child Preserved)
        // --------------------------------------------------------
        private async void LoadData()
        {
            DetailsListView.ItemsSource = null;
            List<Detail> rootItems = new List<Detail>();

            if (!await NetworkExtensions.IsConnectedAsync())
            {
                DependencyService.Get<IToastService>()?.Show("اتصال اینترنت بررسی شود");
                return;
            }

            await FormExtensions.ShowBuildInfoModalAsync(this.Navigation, async () =>
            {
                var categories = await DependencyService.Get<IPlayListServices>()
                                                        .GetCategoriesFromGoogleSheet();

                // درخت واقعی root → child
                rootItems = categories.First().Details;

                // فقط leaf ها برای player
                playableItems = Flatten(rootItems);

                DetailsListView.ItemsSource = rootItems;
            });
        }

        private void DetailsListView_Refreshing(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => LoadData());
            DetailsListView.IsRefreshing = false;
        }

        // --------------------------------------------------------
        //   Constructor هنگام باز کردن دسته فرعی
        // --------------------------------------------------------
        public DetailPages(List<Detail> details)
        {
            InitializeComponent();
            playableItems = Flatten(details);

            details = details
                .OrderByDescending(item =>
                {
                    //string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                    string audioFileName = $"{item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                    return File.Exists(audioPath);
                })
                .ThenBy(item => item.Id)
                .ToList();

            foreach (var item in details)
            {
                //string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                string audioFileName = $"{item.Id}{Path.GetExtension(item.Href)}";
                string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                item.IsDeleteVisible = File.Exists(audioPath);
            }

            DetailsListView.ItemsSource = details;
        }

        // --------------------------------------------------------
        //     ItemTapped Handler
        // --------------------------------------------------------
        private async void DetailsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;

            if (e.Item is Detail item)
            {
                item.Children = item.Children?.Where(x => x.IsVisible).ToList();

                // ------------ اگر Parent → باز کردن صفحه بعد -----------
                if (item.Children != null && item.Children.Any())
                {
                    await Navigation.PushAsync(new DetailPages(item.Children));
                    return;
                }

                // ------------ اگر آهنگ است → دانلود یا پخش -------------
                string extension = Path.GetExtension(item.Href);

                bool isLocal = new[] { ".mp3", ".mp4" }.Contains(extension);
                bool isGoogleDrive = item.Href.StartsWith("https://drive.google.com/", StringComparison.OrdinalIgnoreCase);

                if (!isLocal && !isGoogleDrive)
                    return;

                string audioFileName = $"{item.RepeatId! ?? item.Id}{Path.GetExtension(item.Href)}";
                string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);

                if (!File.Exists(audioPath))
                {
                    var popup = new ConfirmPopup($"آیا مایل هستید آهنگ «{item.Title}» را دانلود کنید؟");
                    await PopupNavigation.Instance.PushAsync(popup);

                    bool confirm = await popup.ShowAsync();
                    if (!confirm) return;

                    var progressHandler = new Progress<double>(p =>
                    {
                        item.DownloadProgress = p;
                        item.DownloadStatus = $"در حال دانلود {Math.Round(p * 100)}٪";
                    });

                    item.IsDownloading = true;
                    item.DownloadStatus = "در حال آماده‌سازی...";
                    item.DownloadProgress = 0;

                    try
                    {
                        item.Href = await DependencyService.Get<IGoogleDriveServices>()
                            .DownloadGoogleDriveFileWithProgressAsync(item.Href, audioFileName, progressHandler);

                        item.DownloadStatus = "دانلود کامل شد";
                        item.IsDownloading = false;
                        item.DownloadProgress = 1;
                    }
                    catch
                    {
                        item.DownloadStatus = "خطا در دانلود";
                        item.IsDownloading = false;
                    }
                }
                else
                {
                    item.Href = audioPath;
                }

                var currentItem = playableItems.FirstOrDefault(x => x.Id == item.Id);
                currentItem.Href = item.Href;

                playableItems = playableItems
                    .OrderByDescending(x =>
                    {
                        //string file = $"{x.ParentId}_{x.Id}{Path.GetExtension(x.Href)}";
                        string file = $"{x.Id}{Path.GetExtension(x.Href)}";
                        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), file);
                        return File.Exists(path);
                    })
                    .ThenBy(x => x.Id)
                    .ToList();

                int index = playableItems.FindIndex(x => x.Id == item.Id);
                PlaybackCapsule.CurrentPlaylist = playableItems;
                PlaybackCapsule.CurrentIndex = index;

                var mini = new MiniPlayerView(true, index, playableItems);
                var page = new ContentPage { Content = mini };
                NavigationPage.SetHasNavigationBar(page, false);
                await Navigation.PushAsync(page);
            }

            ((ListView)sender).SelectedItem = null;
        }

        // --------------------------------------------------------
        //      Flatten-list ساخت لیست آهنگ‌ها از درخت
        // --------------------------------------------------------
        private List<Detail> Flatten(List<Detail> details)
        {
            var flat = new List<Detail>();

            void AddRecursive(Detail d)
            {
                if (d.Children.Any())
                {
                    foreach (var ch in d.Children)
                        AddRecursive(ch);
                }
                else
                {
                    flat.Add(d);
                }
            }

            foreach (var d in details)
                AddRecursive(d);

            return flat
                .OrderByDescending(x =>
                {
                    //string audioFileName = $"{x.ParentId}_{x.Id}{Path.GetExtension(x.Href)}";
                    string audioFileName = $"{x.Id}{Path.GetExtension(x.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                    return File.Exists(audioPath);
                })
                .ThenBy(x => x.Id)
                .ToList();
        }

        // --------------------------------------------------------
        //       حذف (Swipe Menu)
        // --------------------------------------------------------
        private async void OnItemMenuClicked(object sender, EventArgs e)
        {
            var btn = sender as ImageButton;
            if (btn?.CommandParameter is Detail item)
            {
                string action = await DisplayActionSheet($"«{item.Title}»", "انصراف", null, "حذف");

                if (action == "حذف")
                {
                    string audioFileName = $"{item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);

                    if (!File.Exists(audioPath))
                    {
                        await DisplayAlert("خطا", "فایلی برای حذف وجود ندارد.", "باشه");
                        return;
                    }

                    bool confirm = await DisplayAlert("حذف", "آیا مطمئن هستید؟", "بله", "خیر");
                    if (!confirm) return;

                    File.Delete(audioPath);

                    await DisplayAlert("حذف شد", "فایل با موفقیت حذف شد.", "باشه");

                    item.IsDeleteVisible = false;
                    item.DownloadStatus = "";
                    item.DownloadProgress = 0;
                }
            }
        }
    }
}
