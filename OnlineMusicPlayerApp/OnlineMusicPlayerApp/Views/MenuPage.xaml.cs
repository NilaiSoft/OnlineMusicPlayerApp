using OnlineMusicPlayerApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OnlineMusicPlayerApp.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MenuPage : ContentPage
    {
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        List<HomeMenuItem> menuItems;
        [Obsolete]
        public MenuPage()
        {
            InitializeComponent();

            menuItems = new List<HomeMenuItem>
            {
                new HomeMenuItem {Id = MenuItemType.ActivityMain , Title="خانه",Icon="🏡" },
                new HomeMenuItem {Id = MenuItemType.DetailPages , Title="پلی لیست",Icon="🎵" },
                //new HomeMenuItem {Id = MenuItemType.frmSettings, Title="تنظیمات" ,Icon="⚙️"},
                new HomeMenuItem {Id = MenuItemType.About, Title="درباره ی من",Icon="✍️" },
                new HomeMenuItem {Id = MenuItemType.CloseMediaPlayer, Title="خروج",Icon="✍️" }
            };

            ListViewMenu.ItemsSource = menuItems;

            //ListViewMenu.SelectedItem = menuItems[0];
            ListViewMenu.ItemTapped += async (sender, e) =>
            {
                if (e.Item == null)
                    return;

                var id = (int)((HomeMenuItem)e.Item).Id;
                await RootPage.NavigateFromMenu(id);

                ((ListView)sender).SelectedItem = null; // برای پاک کردن انتخاب
            };
        }

        private void ListViewMenu_ItemTapped(object sender, ItemTappedEventArgs e)
        {

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var version = DependencyService.Get<Services.IAppVersionProvider>().GetVersion();
            lblVersion.Text = $"نسخه {version}";
        }
    }
}