using Xamarin.Forms;
using OnlineMusicPlayerApp.Views;
using OnlineMusicPlayerApp.Services;
using OnlineMusicPlayerApp.Extensions;
using System;

namespace OnlineMusicPlayerApp.Views
{
    public partial class ActivityMain : ContentPage
    {
        public ActivityMain()
        {
            InitializeComponent();
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
