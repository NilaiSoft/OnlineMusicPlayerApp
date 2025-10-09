using OnlineMusicPlayerApp.BLL;
using OnlineMusicPlayerApp.Model;
using OnlineMusicPlayerApp.Models;
using OnlineMusicPlayerApp.Services;
using OnlineMusicPlayerApp.ViewModels;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Toast.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class LotteryPage : ContentPage
    {
        ItemsViewModel viewModel;
        static SettingsRepository _SettingService;
        static PeoplesRepository TbPeople;
        static LotteryRepository TbLottery;
        private readonly LotteryGroupsRepository _lotteryGroupsRepository;
        IChatService _chatService;
        INotifyService _notifyService;
        private List<PeoplesItem> peoplesItems;
        private int? _groupId;

        [Obsolete]
        public LotteryPage(int? groupId = null)
        {
            InitializeComponent();
            _chatService = DependencyService.Get<IChatService>();
            _notifyService = DependencyService.Get<INotifyService>();
            Title = "قرعه کشی براساس لیست ورودی";
            TbPeople = new PeoplesRepository(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dbLottery.db3"));
            _SettingService = new SettingsRepository(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dbLottery.db3"));
            TbLottery = new LotteryRepository(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dbLottery.db3"));
            _lotteryGroupsRepository = new LotteryGroupsRepository(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dbLottery.db3"));
            BindingContext = viewModel = new ItemsViewModel();
            _groupId = groupId;
        }

        [Obsolete]
        protected override async void OnAppearing()
        {

            //var text = await DependencyService.Get<IGoogleDriveServices>()
            //    .GetTextFromFileAsync("1vOZx4p5atfvPl8GtFpLvVl-TFbw0SWXm");

            //DependencyService.Get<IToastService>().Show(text);

            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Sms);
            if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                {
                    await DisplayAlert("Need Sms", "", "");
                }

                var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Sms);
                //Best practice to always check that the key exists
                if (results.ContainsKey(Permission.Sms))
                    status = results[Permission.Sms];
            }

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
            ddlGroupName.ItemsSource =
                await _lotteryGroupsRepository.GetAllAsync
                (x => x.LotteryType == (int)LotteryType.Internal
                && (x.Id == _groupId || _groupId == null));

            if (_groupId != null)
            {
                ddlGroupName.SelectedIndex = 0;
                ddlGroupName.IsEnabled = false;
            }

            base.OnAppearing();
        }

        private async void btnSubmit_Clicked(object sender, EventArgs e)
        {
            //if (!string.IsNullOrEmpty(txtGroupName.Text))
            if (ddlGroupName.SelectedIndex >= 0)
            {
                //var settings = (SettingsItem)BindingContext;
                SettingsItem settings = new SettingsItem();
                //settings.GroupName = txtGroupName.Text;
                await _SettingService.SaveNoteAsync(settings);
                //pnlGroupName.IsVisible = false;
                //pnlLottery.IsVisible = true;
                //CrossToastPopUp.Current.ShowToastMessage(txtGroupName.Text, toastLength: ToastLength.Short);
            }
            else
            {
                await DisplayAlert("خطا", "نام گروه را وارد کنید", "ادامه");
            }
        }

        [Obsolete]
        private async void btnStart_Clicked(object sender, EventArgs e)
        {
            //lblTimer.Text = "00:00";
            #region Old
            //var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            //if (status != PermissionStatus.Granted)
            //{
            //    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
            //    {
            //        await DisplayAlert("Need storage", "", "");
            //    }

            //    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
            //    //Best practice to always check that the key exists
            //    if (results.ContainsKey(Permission.Storage))
            //        status = results[Permission.Storage];
            //}
            #endregion
            if (ddlGroupName.SelectedIndex >= 0)
            {
                //pnlGroupName.IsVisible = false;
                //pnlLottery.IsVisible = true;
            }
            else
            {
                DependencyService.Get<IToastService>()?.Show("نام گروه را انتخاب کنید!");
                return;
            }

            var groupId = (LotteryGroups)ddlGroupName.SelectedItem;
            var isScoreSetting = await _SettingService.FindByKeyAsync
                ($"settings.lottery.peoples.Settings.{groupId.Name.Trim()}.IsScore");

            var isScore = bool.Parse(isScoreSetting.Value);

            var isPeopleCountScore = await _SettingService.FindByKeyAsync
                ($"settings.lottery.peoples.Settings.{groupId.Name.Trim()}.IsPeopleCountScore");

            var isOpenWinner = await _SettingService.FindByKeyAsync
                ("settings.lottery.question.openWinnerPage");

            if (!peoplesItems.Any())
            {
                DependencyService.Get<IToastService>()?.Show("قرعه کشی به اتمام رسیده است.");
                btnStart.Text = "Start";
                return;
            }

            var newList = new List<PeoplesItem>();

            foreach (var items in peoplesItems)
            {
                for (int i = 0; i < items.Score; i++)
                {
                    var newItem = new PeoplesItem
                    {
                        FullName = items.FullName,
                        UserName = items.UserName,
                        ProfilePictureId = items.ProfilePictureId,
                        CommentText = items.CommentText,
                        Score = items.Score,
                        Mobile = items.Mobile,
                        ProfilePicture = items.ProfilePicture,
                        ProfilePictureByte = items.ProfilePictureByte,
                        IsPrivate = items.IsPrivate,
                        Pk = items.Pk,
                        IsVerified = items.IsVerified,
                        Id = items.Id,
                        GroupId = items.GroupId
                    };
                    newList.Add(items);
                }
            }
            if (newList.Count > 0)
            {
                peoplesItems = new List<PeoplesItem>();
                peoplesItems.AddRange(newList);
            }

            btnStart.IsEnabled = false;
            ddlGroupName.IsEnabled = false;
            var random = new Random();
            int RandNumber = random.Next(1, peoplesItems.Count);
            LotteryItem item = new LotteryItem();
            await Task.Run(() =>
            {
                int indexItem = 0;

                var random = new Random();
                //int RandNumber = random.Next(100, 200);
                int RandNumber = random.Next(1, peoplesItems.Count + 1);
                //LotteryItem item = new LotteryItem();
                for (int i = 0; i <= RandNumber; i++)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        int index = random.Next(peoplesItems.Count);
                        item.UserName = peoplesItems[index].UserName;
                        item.FullName = peoplesItems[index].FullName;
                        item.Id = peoplesItems[index].Id;
                        item.Mobile = peoplesItems[index].Mobile;
                        item.DateTime = DateTime.Now;
                        item.GroupId = peoplesItems[index].GroupId;
                        item.PeopleId = peoplesItems[index].Id;
                        if (bool.Parse(isOpenWinner?.Value ?? "false"))
                        {
                            Navigation.PushAsync(new frmWinner(item.FullName, ((LotteryGroups)ddlGroupName.SelectedItem).Id), false);
                        }
                        else
                        {
                            btnStart.Text = peoplesItems[index].FullName;
                        }
                        indexItem = index;
                    });
                    Thread.Sleep(50);
                }
            });
            //item.GroupId = _lotteryGroupsRepository.GetAsync(x => x.Name == "OtherLottery").Result.Id;

            if (bool.Parse(isScoreSetting.Value))
            {
                var thisPeople = peoplesItems.Find(x => x.FullName == item.FullName);
                thisPeople.Score = thisPeople.Score - 1;
                var update = await TbPeople.UpdateAsync(thisPeople);
            }
            var result = await TbLottery.CreateAsync(item);
            var isSendSMS = await _SettingService.FindByKeyAsync
                ("settings.lottery.sendsms");

            var isQuestionSendSMS = await _SettingService.FindByKeyAsync
                ("settings.lottery.question.sendsms");

            if (bool.Parse(isSendSMS.Value))
            {
                if (result > 0)
                {
                    if (bool.Parse(isQuestionSendSMS.Value))
                    {
                        if (await DisplayAlert("پرسش", "آیا تمایل به ارسال پیامک اعلام برنده اطمینان دارید؟", "بله", "خیر"))
                        {
                            var setting_LotteryMessage = await _SettingService.FindByKeyAsync
                                ("settings.lottery.peoples.Notification.Text");

                            var group = await _lotteryGroupsRepository.GetAsync(x => x.Id == item.GroupId);
                            string textMessage = string.Format(setting_LotteryMessage.Value, item.FullName, group.Name);
                            await SendSms(textMessage, item.Mobile);
                        }
                    }
                    else
                    {
                        var setting_LotteryMessage = await _SettingService.FindByKeyAsync
                            ("settings.lottery.peoples.Notification.Text");

                        var group = await _lotteryGroupsRepository.GetAsync(x => x.Id == item.GroupId);
                        string textMessage = string.Format(setting_LotteryMessage.Value, item.FullName, group.Name);
                        await SendSms(textMessage, item.Mobile);
                    }
                }
            }
            //});
            btnStart.IsEnabled = true;
            ddlGroupName.IsEnabled = true;
            //await _chatService.SendMessage(item.FullName);
            await _notifyService.ShowNotification(description: item.FullName);
            //lblTimer.Text =
            await DetailsLottery();
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {

        }

        public async Task SendSms(string messageText, string recipient)
        {
            try
            {
                var message = new SmsMessage(messageText, recipient);
                await Sms.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException ex)
            {
                await DisplayAlert("Failed", "Sms is not supported on this device.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Failed", ex.Message, "OK");
            }
        }

        private async void ddlGroupName_SelectedIndexChanged(object sender, EventArgs e)
        {
            await DetailsLottery();
        }

        private async Task DetailsLottery()
        {
            var groupId = (LotteryGroups)ddlGroupName.SelectedItem;

            var isScoreSetting = await _SettingService.FindByKeyAsync
                 ($"settings.lottery.peoples.Settings.{groupId.Name.Trim()}.IsScore");
            var isScore = bool.Parse(isScoreSetting.Value);

            Expression<Func<PeoplesItem, bool>> predicate = x => x.GroupId == groupId.Id;

            if (isScore)
            {
                predicate = x => x.GroupId == groupId.Id && x.Score > 0;
            }

            peoplesItems = await TbPeople.GetPeopelsAsync(predicate);

            var lottery = await TbLottery.GetAllAsync(x => x.GroupId == groupId.Id);
            lblTotalCount.Text = $"تعداد کل : {peoplesItems.Count}";
            lblWinnerCount.Text = $"تعداد نتایج : {lottery.Count}";

            var isPeopleCountScore = await _SettingService.FindByKeyAsync
                 ($"settings.lottery.peoples.Settings.{groupId.Name.Trim()}.IsPeopleCountScore");
            if (bool.Parse(isPeopleCountScore.Value) && isScore)
                lblNegativeCount.Text = $"تعداد قرعه کشی مانده : {peoplesItems.Sum(x => x.Score)}";
            else
                lblNegativeCount.IsVisible = false;
        }

        [Obsolete]
        protected override bool OnBackButtonPressed()
        {
            if (!btnStart.IsEnabled)
            {
                //await Navigation.PopAsync(); // یا Shell.Current.GoToAsync("..")
                DependencyService.Get<IToastService>().Show("قرعه‌کشی هنوز در حال اجراست،لطفاً تا پایان فرآیند منتظر بمانید");
                return true;
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new MainPage(), false);
            });

            return false;
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (!btnStart.IsEnabled)
            {
                return;
            }

            var groupId = (LotteryGroups)ddlGroupName.SelectedItem;

            if (!(groupId is LotteryGroups))
            {
                DependencyService.Get<IToastService>().Show("لطفاً ابتدا عنوان گروه را انتخاب کنید");
                return;
            }
        }
    }
}