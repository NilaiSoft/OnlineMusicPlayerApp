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
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class PlayList : ContentPage
    {
        public PlayList()
        {
            InitializeComponent();
            miniPlayerContainer.Content = PlayerManager.MiniPlayerInstance;
        }

        bool hasLoaded = false;

        protected override async void OnAppearing()
        {
            if (hasLoaded) return;
            hasLoaded = true;

            await FormExtensions.ShowBuildInfoModalAsync(this.Navigation, async () =>
            {
                var tets = await DependencyService.Get<IGoogleDriveServices>().GetMusicPlayList();

                var stack = new StackLayout
                {
                    Padding = new Thickness(20),
                    Spacing = 15
                };

                var categories = await DependencyService.Get<IPlayListServices>().GetCategoriesFromJson();
                foreach (var category in categories)
                {
                    var button = new Button
                    {
                        Text = category.Master,
                        BackgroundColor = Color.FromHex("#eeeeee"),
                        TextColor = Color.Black,
                        CornerRadius = 8
                    };

                    button.Clicked += async (s, e) =>
                    {
                        await Navigation.PushAsync(new DetailPages(category.Details));
                    };

                    stack.Children.Add(button);
                }

                Content = stack;
            });
        }

        public void DetailPage(List<Detail> details)
        {
            Title = "جزئیات";

            var stack = new StackLayout { Padding = 20 };

            foreach (var item in details)
            {
                var label = new Label { Text = item.Title };
                stack.Children.Add(label);
            }

            Content = stack;
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PushAsync(new MainPage(), false);
            return true;
        }
    }
}