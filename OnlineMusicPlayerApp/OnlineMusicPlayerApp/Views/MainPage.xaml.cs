using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using OnlineMusicPlayerApp.Models;
using System.IO;
using Xamarin.Essentials;
using OnlineMusicPlayerApp.Services;

namespace OnlineMusicPlayerApp.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : MasterDetailPage
    {
        Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();
        private readonly BLL.UserNameRepository _UserNameRepository;
        IChatService _chatService;
        public MainPage()
        {
            InitializeComponent();
            //_chatService = DependencyService.Get<IChatService>();
            //Connectivity.ConnectivityChanged += ConnectivityChangedHandler;
            if (_UserNameRepository == null)
            {
                _UserNameRepository = new BLL.UserNameRepository(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dbLottery.db3"));
            }

            MasterBehavior = MasterBehavior.Popover;

            MenuPages.Add((int)MenuItemType.ActivityMain, (NavigationPage)Detail);

        }

        private async void ConnectivityChangedHandler(object sender, ConnectivityChangedEventArgs e)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.None)
                await _chatService.Connect();
        }

        const string StateFile = "state.bin";
        [Obsolete]
        public async Task NavigateFromMenu(int id)
        {
            if (!MenuPages.ContainsKey(id))
            {
                switch (id)
                {
                    case (int)MenuItemType.ActivityMain:
                        MenuPages.Add(id, new NavigationPage(new ActivityMain()));
                        break;
                    case (int)MenuItemType.About:
                        MenuPages.Add(id, new NavigationPage(new AboutPage()));
                        break;
                    case (int)MenuItemType.frmSettings:
                        MenuPages.Add(id, new NavigationPage(new frmSettings()));
                        break;
                    case (int)MenuItemType.LogouFromInstagram:
                        if (await DisplayAlert("پرسش", "برای خروج از حساب اینستاگرام اطمینان دارید؟", "بله", "خیر"))
                        {
                            await LogoutFromInstagram();
                            return;
                        }
                        else return;
                }
            }

            var newPage = MenuPages[id];

            if (newPage != null && Detail != newPage)
            {
                Detail = newPage;

                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);

                MenuPages.Remove(id);

                IsPresented = false;
            }
        }

        async Task LogoutFromInstagram()
        {
            try
            {
                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StateFile);
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    var current = _UserNameRepository.GetCurrentUserAsync();
                    await _UserNameRepository.DeleteAsync(x => x.Id == current.Result.Id);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {

        }
    }
}