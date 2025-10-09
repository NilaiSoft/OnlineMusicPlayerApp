using OnlineMusicPlayerApp.Extensions;
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
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class AboutPage : ContentPage
    {
        HtmlWebViewSource htmlSource;
        public AboutPage()
        {
            InitializeComponent();

            htmlSource = htmlSource.GetHtmlText("OflinePage.html");

            var version = DependencyService.Get<Services.IAppVersionProvider>().GetVersion();
            lblVersion.Text = $"{version}";
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PushAsync(new MainPage(), false);
            return true;
        }

        int touchCount = 0;
        private async void OnLabelTapped(object sender, EventArgs e)
        {
            touchCount++;

            if (touchCount > 3)
            {
                //DependencyService.Get<IToastService>()?.Show(($"{7 - touchCount}", toastLength: ToastLength.Short);
                //DependencyService.Get<IToastService>()?.Show($"{7 - touchCount}");
            }

            if (touchCount == 7)
            {
                touchCount = 0;

                var text = SettingsManager.CurrentSettings.BuildNumberMessage ?? "";
                var imageSrc = SettingsManager.CurrentSettings.BuildNumberImage ?? "";
                // نمایش صفحه‌ی لودینگ
                await FormExtensions.ShowBuildInfoModalAsync(this.Navigation, text, imageSrc, async (result) =>
                {
                    if (result == "Confirm")
                        DependencyService.Get<IPhoneService>()?.Call("09358350348");
                    else if (result == "Close")
                        await Navigation.PopAsync();
                });
            }
        }

        private async void btnCafeBazar_Clicked(object sender, EventArgs e)
        {
            var modal = new HtmlModalPage("https://cafebazaar.ir/app/com.ehsan.nozari.invite.luck");
            await Navigation.PushModalAsync(modal);
        }
    }
}