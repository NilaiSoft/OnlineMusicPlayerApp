using OnlineMusicPlayerApp.Services.PlayListServices;
using System;
using System.Linq;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp.Views
{
    public partial class ActivityMain : ContentPage
    {
        public ActivityMain()
        {
            InitializeComponent();

            UpdateMiniPlayerVisibility();

            MessagingCenter.Subscribe<object>(this, "PlaylistUpdated", (sender) =>
            {
                var newMiniPlayer = new MiniPlayerView();
                MainGrid.Children.Remove(MiniPlayerViewControl);
                MainGrid.Children.Add(newMiniPlayer);
                Grid.SetRow(newMiniPlayer, 4); // حفظ موقعیت در Grid
            });
        }

        private void UpdateMiniPlayerVisibility()
        {
            MiniPlayerViewControl.IsVisible = PlaybackCapsule.CurrentPlaylist != null && PlaybackCapsule.CurrentPlaylist.Any();
        }

        private void btnMenu_Clicked(object sender, EventArgs e)
        {
            App app = Application.Current as App; // Get the current App instance
            var mdPage = app.MainPage as MainPage; // MainPage should be the type of your MasterDetailPage subclass
            mdPage.IsPresented = true; // present the master page
        }

        private void OnPlaylistClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new DetailPages());
        }
    }
}
