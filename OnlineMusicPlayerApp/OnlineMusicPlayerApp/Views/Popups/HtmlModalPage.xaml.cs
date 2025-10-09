using OnlineMusicPlayerApp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Xamarin.Forms.Xaml;

namespace OnlineMusicPlayerApp.Views.Popups
{
    public partial class HtmlModalPage : ContentPage
    {
        private TaskCompletionSource<string> _resultSource = new TaskCompletionSource<string>();
        public Task<string> Result => _resultSource.Task;

        public HtmlModalPage(string url)
        {
            InitializeComponent();

            btnConfirm.IsVisible = false;
            btnClose.IsVisible = false;
            HtmlWebView.Source = url;
        }

        private HtmlModalPage(string htmlContent, string btnConfirmText = "", string btnCloseText = "")
        {
            InitializeComponent();

            HtmlWebView.Source = new HtmlWebViewSource { Html = htmlContent };
            btnConfirm.Text = btnConfirmText;
            btnClose.Text = btnCloseText;

            btnConfirm.IsVisible = !string.IsNullOrEmpty(btnConfirmText);
            btnClose.IsVisible = !string.IsNullOrEmpty(btnCloseText);
        }

        public static async Task<HtmlModalPage> CreateAsync(string text, string imageSrc, string btnConfirmText = "", string btnCloseText = "")
        {
            if (string.IsNullOrEmpty(imageSrc))
            {
                await SettingsManager.LoadAsync();
                text = SettingsManager.CurrentSettings.BuildNumberMessage ?? "";
                imageSrc = SettingsManager.CurrentSettings.BuildNumberImage ?? "";
            }

            string html = "";
            string htmlTemplate;
            var assembly = typeof(App).GetTypeInfo().Assembly;

            if (imageSrc.StartsWith("https://drive.google.com/"))
            {
                imageSrc = await DependencyService.Get<IGoogleDriveServices>().DownloadGoogleDriveFileAsync(imageSrc, "myfile.jpg");
                if (string.IsNullOrEmpty(imageSrc))
                {
                    using (var stream = assembly.GetManifestResourceStream("OnlineMusicPlayerApp.Resources.Html.OflinePage.html"))
                    using (var reader = new StreamReader(stream))
                    {
                        htmlTemplate = reader.ReadToEnd();
                    }
                    return new HtmlModalPage(htmlTemplate, btnConfirmText, btnCloseText);
                }

                byte[] imageBytes = File.ReadAllBytes(imageSrc);
                string base64Image = Convert.ToBase64String(imageBytes);
                html = $"<img src='data:image/jpeg;base64,{base64Image}' />";
            }


            if (!string.IsNullOrEmpty(imageSrc))
            {
                html = string.IsNullOrEmpty(html)
                    ? $"<img src=\"{imageSrc}\" alt=\"عکس زیبا\" />"
                    : html;

                string overlayHtml = string.IsNullOrWhiteSpace(text)
                    ? ""
                    : $"<div class='overlay'><h2>{text}</h2></div>";

                using (var stream = assembly.GetManifestResourceStream("OnlineMusicPlayerApp.Resources.Html.ImageText.html"))
                using (var reader = new StreamReader(stream))
                {
                    htmlTemplate = reader.ReadToEnd();
                }

                htmlTemplate = htmlTemplate.Replace("{{image}}", html)
                                           .Replace("{{overlay}}", overlayHtml);

                return new HtmlModalPage(htmlTemplate, btnConfirmText, btnCloseText);
            }

            using (var stream = assembly.GetManifestResourceStream("OnlineMusicPlayerApp.Resources.Html.OflinePage.html"))
            using (var reader = new StreamReader(stream))
            {
                htmlTemplate = reader.ReadToEnd();
            }
            return new HtmlModalPage(htmlTemplate, btnConfirmText, btnCloseText);
        }

        private async void btnConfirm_Clicked(object sender, EventArgs e)
        {
            _resultSource.SetResult("Confirm");
            await Navigation.PopModalAsync();
        }

        private async void btnClose_Clicked(object sender, EventArgs e)
        {
            _resultSource.SetResult("Close");
            await Navigation.PopModalAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            DependencyService.Get<ISecureScreen>()?.EnableSecure();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DependencyService.Get<ISecureScreen>()?.DisableSecure();
        }

    }
}