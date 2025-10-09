using OnlineMusicPlayerApp.Services;
using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            //OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://Linqe.ir/Ehsannozari"));
            Instagram = new Command(async () => await Browser.OpenAsync("https://www.instagram.com/ehsannozari/"));
            Telegram = new Command(async () => await Browser.OpenAsync("https://www.t.me/+989358350348/"));
            Phone = new Command(async () => PhoneDialer.Open(SettingsManager.CurrentSettings.DeveloperMobile ?? "09358350348"));
            //Phone = new Command(async () => DependencyService.Get<IPhoneService>()?.Call("09358350348"));
        }

        public ICommand Phone { get; }
        public ICommand Telegram { get; }
        public ICommand Instagram { get; }
    }
}