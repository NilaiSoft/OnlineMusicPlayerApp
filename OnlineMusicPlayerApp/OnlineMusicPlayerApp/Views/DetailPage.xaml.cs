using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Logger;
using OnlineMusicPlayerApp.BLL;
using OnlineMusicPlayerApp.Model;
using OnlineMusicPlayerApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OnlineMusicPlayerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailPage : ContentPage
    {
        public DetailPage(List<Detail> details)
        {
            Title = "سبک‌های موسیقی";

            var stack = new StackLayout
            {
                Padding = new Thickness(20),
                Spacing = 15
            };

            foreach (var item in details)
            {
                var button = new Button
                {
                    Text = item.Title,
                    BackgroundColor = Color.FromHex("#eeeeee"),
                    TextColor = Color.Black,
                    CornerRadius = 8
                };

                button.Clicked += async (s, e) =>
                {
                    // رفتن به مسیر مربوطه
                    await DisplayAlert("مسیریابی", $"رفتن به: {item.Href}", "باشه");

                    // اگر از Shell استفاده می‌کنی:
                    // await Shell.Current.GoToAsync(item.Href);

                    // یا اگر صفحه خاصی داری:
                    // await Navigation.PushAsync(new GenrePage(item.Id));
                };

                stack.Children.Add(button);
            }

            Content = new ScrollView { Content = stack };
        }
        protected override bool OnBackButtonPressed()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            return false;
        }
    }
}