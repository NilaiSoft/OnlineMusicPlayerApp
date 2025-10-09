using OnlineMusicPlayerApp.BLL;
using OnlineMusicPlayerApp.Model;
using OnlineMusicPlayerApp.Services;
using Plugin.Toast;
using Plugin.Toast.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OnlineMusicPlayerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class frmSettings : ContentPage
    {
        static BLL.SettingsRepository database;
        public frmSettings()
        {
            InitializeComponent();
            Title = "تنظیمات";
            if (database == null)
            {
                database = new SettingsRepository(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dbLottery.db3"));
            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            var currentSetting_OpenWinnerPage =
                await database.FindAsync(x => x.Key == "settings.lottery.question.openWinnerPage");
            if (currentSetting_OpenWinnerPage == null)
                await database.CreateAsync(new SettingsItem
                {
                    Key = "settings.lottery.question.openWinnerPage",
                    Value = false.ToString()
                });

            var setting_IsReloadInstaFollowers = await database.FindByKeyAsync
                ("settings.lottery.instagram.followers.IsReloadInstaFollowers");

            var setting_IsSendSMS = await database.FindByKeyAsync
                ("settings.lottery.sendsms");

            var setting_IsQuestionSendSMS = await database.FindByKeyAsync
                ("settings.lottery.question.sendsms");

            var setting_IsOpenWinnerPage = await database.FindByKeyAsync
                ("settings.lottery.question.openWinnerPage");

            //txtGroupName.Text = currentSettings.GroupName;
            chkIsRelodFollowerList.IsToggled = bool.Parse(setting_IsReloadInstaFollowers.Value);
            chkSendSMS.IsToggled = bool.Parse(setting_IsSendSMS.Value);
            chkQuestionSMS.IsToggled = bool.Parse(setting_IsQuestionSendSMS.Value);
            chkQuestionSMS.IsEnabled = chkSendSMS.IsToggled;
            chkOpenWinnerPage.IsToggled = bool.Parse(setting_IsOpenWinnerPage.Value);
        }

        private async void btnSubmit_Clicked(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(txtGroupName.Text))
            //{
            //    await DisplayAlert("خطا", "نام گروه را وارد کنید!", "ادامه");
            //    txtGroupName.Focus();
            //    return;
            //}

            var currentSetting_IsReloadInstaFollowers =
                await database.FindAsync(x => x.Key == "settings.lottery.instagram.followers.IsReloadInstaFollowers");
            currentSetting_IsReloadInstaFollowers.Value = chkIsRelodFollowerList.IsToggled.ToString().ToLower();
            var result = await database.Update(currentSetting_IsReloadInstaFollowers);

            var currentSetting_QuestionSendSMS =
            await database.FindAsync(x => x.Key == "settings.lottery.question.sendsms");
            currentSetting_QuestionSendSMS.Value = chkQuestionSMS.IsToggled.ToString().ToLower();
            result = await database.Update(currentSetting_QuestionSendSMS);

            var currentSetting_SendSMS =
            await database.FindAsync(x => x.Key == "settings.lottery.sendsms");
            currentSetting_SendSMS.Value = chkSendSMS.IsToggled.ToString().ToLower();
            result = await database.Update(currentSetting_SendSMS);

            if (result == 1)
            {
                //await Navigation.PushAsync(new MainPage(), false);
                DependencyService.Get<IToastService>()?.Show("تنظیمات با موفقیت انجام شد.");//, toastLength: ToastLength.Short);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PushAsync(new MainPage(), false);
            return true;
        }

        private void btnMenu_Clicked(object sender, EventArgs e)
        {
            App app = Application.Current as App; // Get the current App instance
            var mdPage = app.MainPage as MainPage; // MainPage should be the type of your MasterDetailPage subclass
            mdPage.IsPresented = true; // present the master page
        }

        private async void chkSendSMS_Toggled(object sender, ToggledEventArgs e)
        {
            chkQuestionSMS.IsEnabled = chkSendSMS.IsToggled;

            var currentSetting_SendSMS =
                await database.FindAsync(x => x.Key == "settings.lottery.sendsms");
            currentSetting_SendSMS.Value = chkSendSMS.IsToggled.ToString().ToLower();
            var result = await database.Update(currentSetting_SendSMS);
        }

        private async void chkQuestionSMS_Toggled(object sender, ToggledEventArgs e)
        {
            var currentSetting_QuestionSendSMS =
                await database.FindAsync(x => x.Key == "settings.lottery.question.sendsms");
            currentSetting_QuestionSendSMS.Value = chkQuestionSMS.IsToggled.ToString().ToLower();
            var result = await database.Update(currentSetting_QuestionSendSMS);
        }

        private async void chkOpenWinnerPage_Toggled(object sender, ToggledEventArgs e)
        {
            var currentSetting_OpenWinnerPage =
                await database.FindAsync(x => x.Key == "settings.lottery.question.openWinnerPage");

            if (currentSetting_OpenWinnerPage == null)
            {
                await database.CreateAsync(new SettingsItem
                {
                    Key = "settings.lottery.question.openWinnerPage",
                    Value = chkOpenWinnerPage.IsToggled.ToString().ToLower()
                });
            }
            else
            {
                currentSetting_OpenWinnerPage.Value = chkOpenWinnerPage.IsToggled.ToString().ToLower();
                var result = await database.Update(currentSetting_OpenWinnerPage);
            }
        }
    }
}