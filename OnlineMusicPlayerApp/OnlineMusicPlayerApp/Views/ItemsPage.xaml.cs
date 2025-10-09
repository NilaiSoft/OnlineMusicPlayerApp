using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using LotteryApp.Models;
using LotteryApp.Views;
using LotteryApp.ViewModels;
using LotteryApp.Model;
using Plugin.Toast;
using Plugin.Toast.Abstractions;

namespace LotteryApp.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ItemsPage : ContentPage
    {
        ItemsViewModel viewModel;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new ItemsViewModel();
        }
        static BLL.Settings database;
        private async void btnSubmit_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtGroupName.Text))
            {
                //var settings = (SettingsItem)BindingContext;
                SettingsItem settings = new SettingsItem();
                settings.GroupName = txtGroupName.Text;
                await database.SaveNoteAsync(settings);
                pnlGroupName.IsVisible = false;
                pnlLottery.IsVisible = true;
                CrossToastPopUp.Current.ShowToastMessage(txtGroupName.Text, toastLength: ToastLength.Short);
            }
            else
            {
                await DisplayAlert("خطا", "نام گروه را وارد کنید", "ادامه");
            }
        }

        private void btnStart_Clicked(object sender, EventArgs e)
        {
            Random random = new Random();
            btnStart.Text = random.Next(10, 100).ToString();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }
    }
}