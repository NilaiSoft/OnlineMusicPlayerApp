using OnlineMusicPlayerApp.Extensions;
using OnlineMusicPlayerApp.Models;
using OnlineMusicPlayerApp.Services.PlayListServices;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MiniPlayerView : ContentView
    {
        public MiniPlayerView()
        {
            InitializeComponent();
        }

        bool hasLoaded = false;

        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    miniPlayer.LoadCategories();
        //}

        public void LoadCategories()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await FormExtensions.ShowBuildInfoModalAsync(Application.Current.MainPage.Navigation, async () =>
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
                            await Application.Current.MainPage.Navigation.PushAsync(new DetailPages(category.Details));
                        };

                        stack.Children.Add(button);
                    }

                    Content = stack;
                });
            });
        }


        public void DetailPage(List<Detail> details)
        {
           var Title = "جزئیات";

            var stack = new StackLayout { Padding = 20 };

            foreach (var item in details)
            {
                var label = new Label { Text = item.Title };
                stack.Children.Add(label);
            }

            Content = stack;
        }

        private void OnExpandClicked(object sender, System.EventArgs e)
        {

        }

        private void OnPreviousClicked(object sender, System.EventArgs e)
        {

        }

        private void OnPlayClicked(object sender, System.EventArgs e)
        {

        }

        private void OnNextClicked(object sender, System.EventArgs e)
        {

        }

        //protected override void BackButtonBehavior()
        //{
        //    Navigation.PushAsync(new MainPage(), false);
        //    return true;
        //}
    }
}