using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using OnlineMusicPlayerApp.Models;
using OnlineMusicPlayerApp.Services.PlayListServices;
using Xamarin.Essentials;
using System.Threading.Tasks;
using static Android.Telecom.Call;
using OnlineMusicPlayerApp.Extensions;

namespace OnlineMusicPlayerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailPages : ContentPage
    {
        private List<Detail> playableItems;
        private bool isTimerRunning = false;

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
                    ListImageSrc=x.AlbumImageSrc
                }).ToList();

                playableItems = Flatten(details);
                DetailsListView.ItemsSource = details;
            });
        }

        private void DetailsListView_Refreshing(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                LoadData(); // بارگذاری مجدد داده‌ها
            });

            DetailsListView.IsRefreshing = false; // توقف spinner
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
    return System.IO.File.Exists(audioPath);
})
.ThenBy(item => item.Id) // سپس مرتب‌سازی بر اساس Id
.ToList();
            DetailsListView.ItemsSource = details;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private async void DetailsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Detail item)
            {
                item.Children = item.Children.Where(x => x.IsVisible).ToList();

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

                    playableItems = playableItems
                        .OrderByDescending(item =>
                        {
                            string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                            string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                            return System.IO.File.Exists(audioPath);
                        })
                        .ThenBy(item => item.Id) // سپس مرتب‌سازی بر اساس Id
                        .ToList();

                    PlaybackCapsule.CurrentPlaylist = playableItems;
                    PlaybackCapsule.CurrentIndex = index;

                    var miniPlayer = new MiniPlayerView(true, index, playableItems);
                    var page = new ContentPage { Content = miniPlayer };
                    NavigationPage.SetHasNavigationBar(page, false);
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

            flat = flat
    .OrderByDescending(item =>
    {
        string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
        string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
        return System.IO.File.Exists(audioPath);
    })
    .ThenBy(item => item.Id) // سپس مرتب‌سازی بر اساس Id
    .ToList();

            return flat;
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
    }
}
