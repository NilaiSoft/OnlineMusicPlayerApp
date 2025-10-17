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
            // منطق باز کردن منو
        }

        private void OnPlaylistClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new PlayList());
        }
    }
}
