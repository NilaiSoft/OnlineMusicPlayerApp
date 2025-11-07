using OnlineMusicPlayerApp.Services;
using OnlineMusicPlayerApp.ViewModels;
using OnlineMusicPlayerApp.Views.Popups;
using Plugin.Toast;
using Plugin.Toast.Abstractions;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OnlineMusicPlayerApp.Views
{
    public partial class LoadingBoxPage : ContentPage
    {
        public LoadingBoxPage()
        {
            BackgroundColor = Color.FromRgba(0, 0, 0, 0); // نیمه‌شفاف برای پس‌زمینه

            var box = new Frame
            {
                BackgroundColor = Color.White,
                CornerRadius = 12,
                Padding = new Thickness(20),
                HasShadow = true,
                Content = new StackLayout
                {
                    Spacing = 15,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                {
                    new ActivityIndicator
                    {
                        IsRunning = true,
                        Color = Color.Gray,
                        WidthRequest = 40,
                        HeightRequest = 40
                    },
                    new Label
                    {
                        Text = "در حال آماده‌سازی...",
                        FontSize = 14,
                        TextColor = Color.Black,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                }
                }
            };

            Content = new Grid
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children = { box }
            };
        }
    }
}