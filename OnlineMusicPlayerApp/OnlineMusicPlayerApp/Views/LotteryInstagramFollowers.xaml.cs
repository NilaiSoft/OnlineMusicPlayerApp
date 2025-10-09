using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Logger;
using LotteryApp.BLL;
using LotteryApp.InstagramServices;
using LotteryApp.Model;
using LotteryApp.Models;
using NilaiSoft.Api.Samples;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Toast;
using Plugin.Toast.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LotteryApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LotteryInstagramFollowers : ContentPage
    {
        static BLL.Lottery TbLottery;
        static BLL.UserName _UserNameRepository;
        public LotteryInstagramFollowers()
        {
            InitializeComponent();
            Title = "قرعه کشی اعضای اینستاگرام";
            TbLottery = new Lottery(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dbLottery.db3"));
            _UserNameRepository = new UserName(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dbLottery.db3"));
        }

        private static IInstaApi _instaApi;
        const string StateFile = "state.bin";

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
        void SaveSession()
        {
            if (_instaApi == null)
                return;
            var state = _instaApi.GetStateDataAsStream();

            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StateFile);

            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                state.Seek(0, System.IO.SeekOrigin.Begin);
                state.CopyTo(fileStream);
            }
        }
        public async Task<bool> Login()
        {
            var currentUser = await _UserNameRepository.GetCurrentUserAsync();
            if (currentUser == null)
            {
                await Navigation.PushAsync(new InstaLoginPage());
                return false;
            }

            var userSession = new UserSessionData
            {
                UserName = currentUser.UserName,
                Password = currentUser.Password
            };

            _instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.All))
                .SetRequestDelay(RequestDelay.FromSeconds(0, 1))
                .Build();
            LoadSession();

            if (!_instaApi.IsUserAuthenticated)
            {
                var logInResult = await _instaApi.LoginAsync();
                Debug.WriteLine(logInResult.Value);
                if (logInResult.Succeeded)
                {
                    // Save session 
                    SaveSession();
                    return true;
                }
                else
                {
                    // two factor is required
                    if (logInResult.Value == InstaLoginResult.TwoFactorRequired)
                    {
                        string _TwoFactor = System.IO.File.ReadAllText(@"TwoFactor.txt", Encoding.UTF8);
                        var logInResultTwoFactor = await _instaApi.TwoFactorLoginAsync(_TwoFactor);
                        SaveSession();
                        // open a box so user can send two factor code
                    }
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        [Obsolete]
        private async void btnStart_Clicked(object sender, EventArgs e)
        {
            if (!Utility.IsConnectedToInternet())
            {
                CrossToastPopUp.Current.ShowToastMessage("Connection Error");
                return;
            }
            //imgLoading.IsVisible = true;
            if (!string.IsNullOrEmpty(txtUrlPost.Text) && imgPost.Source == null)
            {
                CrossToastPopUp.Current.ShowToastMessage("لینک پست بارگذاری نشده است.");
                return;
            }
            if (!string.IsNullOrEmpty(txtUrlPost.Text))
            {
                FollowerComents();
            }
            else
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                    {
                        await DisplayAlert("Need storage", "", "");
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                    //Best practice to always check that the key exists
                    if (results.ContainsKey(Permission.Storage))
                        status = results[Permission.Storage];
                }

                btnStart.IsEnabled = false;

                await Login();
                var instaResult = await new Followers(_instaApi).GetFollowersAsync();
                var followerList = instaResult.Item1.Value;
                List<PeoplesItem> list = new List<PeoplesItem>();
                int id = 0;
                await Task.Run(() =>
                {
                    int indexItem = 0;
                    foreach (var followerItems in followerList)
                    {
                        id++;
                        list.Add(new PeoplesItem()
                        {
                            Id = id,
                            FullName = $"{followerItems.FullName.Trim()}",
                            Mobile = $"{followerItems.FullName.Trim()}{Environment.NewLine}نام کاربری : {followerItems.UserName.Trim()}",
                            UserName = followerItems.UserName,
                            PictureProfile = followerItems.ProfilePicture
                        });
                    }

                    var random = new Random();
                    int RandNumber = random.Next(500, 1000);
                    Models.LotteryItem item = new LotteryItem();
                    for (int i = 0; i <= RandNumber; i++)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            int index = random.Next(list.Count);
                            item.FullName = list[index].FullName;
                            item.Id = list[index].Id;
                            item.Mobile = list[index].Mobile;
                            item.DateTime = DateTime.Now;
                            lblFullName.Text = list[index].FullName;
                            lblUserName.Text = list[index].UserName;
                            indexItem = index;
                        });
                        Thread.Sleep(10);
                    }
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        imgAvatar.Source = list[indexItem].PictureProfile;
                    });
                    var result = TbLottery.SaveNoteAsync(item);
                    //imgLoading.IsVisible = false;
                });
                btnStart.IsEnabled = true;
            }
        }

        [Obsolete]
        private async void FollowerComents()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            if (status != PermissionStatus.Granted)
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                {
                    await DisplayAlert("Need storage", "", "");
                }

                var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                //Best practice to always check that the key exists
                if (results.ContainsKey(Permission.Storage))
                    status = results[Permission.Storage];
            }

            btnStart.IsEnabled = false;

            await Login();
            var instaResult = await new MediaComments(_instaApi).GetMediaCommentInfoFromUrlAsync(txtUrlPost.Text);
            var commentList = instaResult.Value.Comments;
            List<PeoplesItem> list = new List<PeoplesItem>();
            if (commentList.Count < 1)
            {
                CrossToastPopUp.Current.ShowToastMessage("هیچ کامنتی ثبت نشده است.");
                btnStart.IsEnabled = true;
                //imgLoading.IsVisible = false;
                return;
            }
            int id = 0;
            foreach (var followerItems in commentList)
            {
                id++;
                list.Add(new PeoplesItem()
                {
                    Id = id,
                    FullName = $"{followerItems.User.FullName.Trim()}",
                    Mobile = $"{followerItems.User.FullName.Trim()}{Environment.NewLine}نام کاربری : {followerItems.User.UserName.Trim()}",
                    UserName = followerItems.User.UserName,
                    PictureProfile = followerItems.User.ProfilePicture
                });
            }

            var random = new Random();
            int RandNumber = random.Next(500, 1000);
            Models.LotteryItem item = new LotteryItem();
            await Task.Run(() =>
            {
                int indexItem = 0;
                for (int i = 0; i <= RandNumber; i++)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        int index = random.Next(list.Count);
                        item.FullName = list[index].FullName;
                        item.Id = list[index].Id;
                        item.Mobile = list[index].Mobile;
                        item.DateTime = DateTime.Now;
                        lblFullName.Text = list[index].FullName;
                        lblUserName.Text = list[index].UserName;
                        indexItem = index;
                    });
                    Thread.Sleep(10);
                }
                Device.BeginInvokeOnMainThread(() =>
                {
                    imgAvatar.Source = list[indexItem].PictureProfile;
                });
                var result = TbLottery.SaveNoteAsync(item);
            });
            btnStart.IsEnabled = true;
            //imgLoading.IsVisible = false;
        }

        private async void btnLoadPost_Clicked(object sender, EventArgs e)
        {
            if (btnLoadPost.Text == "Clear")
            {
                txtUrlPost.Text = string.Empty;
                btnLoadPost.Text = "Load Post";
                txtUrlPost.IsReadOnly = false;
                imgPost.Source = null;
                btnStart.Text = "شروع";
                return;
            }
            txtUrlPost.Text = await Xamarin.Essentials.Clipboard.GetTextAsync();
            if (!Utility.IsConnectedToInternet())
            {
                CrossToastPopUp.Current.ShowToastMessage("Connection Error");
                return;
            }

            if (string.IsNullOrEmpty(txtUrlPost.Text))
            {
                CrossToastPopUp.Current.ShowToastMessage("فیلد آدرس خالیست");
                return;
            }
            await Login();
            var instaResult = await new MediaComments(_instaApi).GetMediaFromUrlAsync(txtUrlPost.Text);
            foreach (var item in instaResult.Value.Images.Take(1))
            {
                imgPost.Source = item.URI;
            }
            btnLoadPost.Text = "Clear";
            txtUrlPost.IsReadOnly = true;
        }

        private async void btnStartFollowers_Clicked(object sender, EventArgs e)
        {
            if (!Utility.IsConnectedToInternet())
            {
                CrossToastPopUp.Current.ShowToastMessage("Connection Error");
                return;
            }
            //imgLoading.IsVisible = true;
            if (!string.IsNullOrEmpty(txtUrlPost.Text) && imgPost.Source == null)
            {
                CrossToastPopUp.Current.ShowToastMessage("لینک پست بارگذاری نشده است.");
                return;
            }
            await Login();
            var instaResult = await new Followers(_instaApi).GetFollowersAsync();
            var followerList = instaResult.Item1.Value;
            List<long> userIds = new List<long>();
            foreach (var item in followerList)
            {
                userIds.Add(item.Pk);
            }
            var instaResult1 = await new Followers(_instaApi).FollowerAddUser(userIds);
        }
    }
}