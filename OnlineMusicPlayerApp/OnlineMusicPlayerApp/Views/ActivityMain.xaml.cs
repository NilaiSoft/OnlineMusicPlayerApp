using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Logger;
using OnlineMusicPlayerApp.BLL;
using OnlineMusicPlayerApp.Model;
using System;
using System.Diagnostics;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OnlineMusicPlayerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityMain : ContentPage
    {
        private static IInstaApi _instaApi;
        const string StateFile = "state.bin";
        static BLL.UserNameRepository _UserNameRepository;
        static BLL.SettingsRepository _SettingsRepository;
        public ActivityMain()
        {
            InitializeComponent();
            if (_UserNameRepository == null)
            {
                _UserNameRepository = new UserNameRepository(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dbLottery.db3"));
            }
            if (_SettingsRepository == null)
            {
                _SettingsRepository = new SettingsRepository(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dbLottery.db3"));
            }
            LoadForm();
        }

        private async void LoadForm()
        {
            var isSettings = await _SettingsRepository.Any();
            if (!isSettings)
            {
                await _SettingsRepository.SaveNoteAsync(new SettingsItem());
            }
        }

        private void btnMenu_Clicked(object sender, EventArgs e)
        {
            App app = Application.Current as App; // Get the current App instance
            var mdPage = app.MainPage as MainPage; // MainPage should be the type of your MasterDetailPage subclass
            mdPage.IsPresented = true; // present the master page
        }

        private async void btnPeoples_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LotteryPage(),false);
            //var result = await Navigation.ShowPopupAsync(new PopupSelectOrInsertGroupName());
        }

        private async void alert_Clicked(object sender, EventArgs e)
        {
            var response = await DisplayAlert("Alert", "Message", "ok", "cancel");

            if (response)
            {
                //user click ok  
                await DisplayAlert("response", "ok", "Exit");
            }

            else
            {
                //user click cancel  
                await DisplayAlert("response", "cancel", "Exit");

            }
        }

        [Obsolete]
        private async void btnInstagram_Clicked(object sender, EventArgs e)
        {
            //else
            //    await Navigation.PushAsync(new LotteryInstagramFollowers());
        }

        void LoadSession()
        {
            try
            {
                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StateFile);
                if (System.IO.File.Exists(fileName))
                {
                    Debug.WriteLine("Loading state from file");
                    using (var fs = System.IO.File.OpenRead(fileName))
                    {
                        _instaApi.LoadStateDataFromStream(fs);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            return false;
        }
    }
}