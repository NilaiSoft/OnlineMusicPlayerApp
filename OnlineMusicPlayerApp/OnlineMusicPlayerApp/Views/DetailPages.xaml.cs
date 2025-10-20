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
