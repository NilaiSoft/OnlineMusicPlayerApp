using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using OnlineMusicPlayerApp.Services;
using OnlineMusicPlayerApp.Views;
using Microsoft.AspNetCore.SignalR.Client;
using Plugin.Toast;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace OnlineMusicPlayerApp
{
    public partial class App : Xamarin.Forms.Application
    {
        //TODO: Replace with *.azurewebsites.net url after deploying backend to Azure
        //To debug on Android emulators run the web backend against .NET Core not IIS
        //If using other emulators besides stock Google images you may need to adjust the IP address
        public static string AzureBackendUrl =
            DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5000" : "http://localhost:5000";
        public static bool UseMockDataStore = true;
        public static HubConnection hubConnection;
        INotifyService _notifyService;
        public App()
        {
            Xamarin.Forms.Application.Current.On<Xamarin.Forms.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
            InitializeComponent();

            if (UseMockDataStore)
                DependencyService.Register<MockDataStore>();
            else
                DependencyService.Register<AzureDataStore>();
            MainPage = new MainPage();
            _notifyService = DependencyService.Get<INotifyService>();
            Device.SetFlags(new[] { "MediaElement_Experimental" });
        }

        protected async override void OnStart()
        {
            #region OldMethod
            //            hubConnection = new HubConnectionBuilder().WithUrl("https://linqe.ir/movehub").Build();

            //#if DEBUG
            //            hubConnection = new HubConnectionBuilder().WithUrl("http://192.168.43.14/movehub").Build();
            //#else

            //#endif
            #endregion
            await SettingsManager.LoadAsync();

            //hubConnection = new HubConnectionBuilder()
            ////.WithUrl($"https://Linqe.ir/movehub", (opts) =>
            //.WithUrl($"http://192.168.43.14/movehub", (opts) =>
            //{
            //    opts.HttpMessageHandlerFactory = (message) =>
            //    {
            //        if (message is System.Net.Http.HttpClientHandler clientHandler)
            //            // bypass SSL certificate
            //            clientHandler.ServerCertificateCustomValidationCallback +=
            //                (sender, certificate, chain, sslPolicyErrors) => { return true; };
            //        return message;
            //    };
            //}).Build();

            // Handle when your app starts
            //#if DEBUG
            //            hubConnection = new HubConnectionBuilder()
            //                        .WithUrl($"https://192.168.43.14/movehub", (opts) =>
            //                        {
            //                            opts.HttpMessageHandlerFactory = (message) =>
            //                            {
            //                                if (message is System.Net.Http.HttpClientHandler clientHandler)
            //                                    // bypass SSL certificate
            //                                    clientHandler.ServerCertificateCustomValidationCallback +=
            //                                                    (sender, certificate, chain, sslPolicyErrors) => { return true; };
            //                                return message;
            //                            };
            //                        }).Build();
            //#endif

            //hubConnection.On<string>("ReceiveNewPosition", (message) =>
            //{
            //    //DependencyService.Get<IToastService>()?.Show($"{message}");
            //    _notifyService.ShowNotification(description: message);
            //});

            //if (hubConnection.State == HubConnectionState.Disconnected)
            //{
            //    try
            //    {
            //        await hubConnection.StartAsync();
            //        DependencyService.Get<IToastService>()?.Show("Connected!");
            //    }
            //    catch (System.Exception ex) { }
            //}

            //if (hubConnection.State == HubConnectionState.Connected)
            //{
            //    await hubConnection.SendAsync("MoveFromServer", "Ehsan Nozari");
            //}
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
