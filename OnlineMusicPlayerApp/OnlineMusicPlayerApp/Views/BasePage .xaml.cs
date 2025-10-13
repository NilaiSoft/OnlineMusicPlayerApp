using InstaSharper.Classes.Models;
using OnlineMusicPlayerApp.Extensions;
using OnlineMusicPlayerApp.Models;
using OnlineMusicPlayerApp.Services;
using OnlineMusicPlayerApp.Services.PlayListServices;
using OnlineMusicPlayerApp.Views.Popups;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace OnlineMusicPlayerApp.Views
{
    public partial class BasePage : ContentPage
    {
        protected Views.MiniPlayerView miniPlayer;

        public BasePage()
        {
            var layout = new AbsoluteLayout();

            // محتوای صفحه اصلی
            var mainContent = new ContentView();
            AbsoluteLayout.SetLayoutBounds(mainContent, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(mainContent, AbsoluteLayoutFlags.All);

            // MiniPlayer پایین صفحه
            miniPlayer = new Views.MiniPlayerView();
            AbsoluteLayout.SetLayoutBounds(miniPlayer, new Rectangle(0, 1, 1, 80));
            AbsoluteLayout.SetLayoutFlags(miniPlayer, AbsoluteLayoutFlags.WidthProportional | AbsoluteLayoutFlags.PositionProportional);

            layout.Children.Add(mainContent);
            layout.Children.Add(miniPlayer);

            Content = layout;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // فعال‌سازی یا به‌روزرسانی MiniPlayer
            miniPlayer.IsVisible = true;
            //miniPlayer.UpdateTrack("نغمه‌ای که در سکوت ادامه دارد...");
        }

        private void OnExpandClicked(object sender, EventArgs e)
        {

        }
    }
}