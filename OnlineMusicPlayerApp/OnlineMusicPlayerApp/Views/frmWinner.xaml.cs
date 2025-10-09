using OnlineMusicPlayerApp.Models;
using System;
using Xamarin.Forms;

namespace OnlineMusicPlayerApp.Views
{
    public partial class frmWinner : ContentPage
    {
        private readonly int _GroupId;
        public frmWinner(string peopleName)
        {
            InitializeComponent();
            RevealWinner(peopleName);
        }

        public frmWinner(string peopleName, int groupId)
        {
            InitializeComponent();
            _GroupId = groupId;
            RevealWinner(peopleName);
        }

        private async void RevealWinner(string name)
        {
            // 🔊 پخش صدای مرموز




            //var assembly = typeof(App).GetTypeInfo().Assembly;
            //Stream audioStream = assembly.GetManifestResourceStream("mp3.mp3");


            //var player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
            //player.Load(audioStream);

            // ✨ افکت نور
            //string winnerText = $"🎉 {name} 🎉";
            string winnerText = $"{name}";
            lblWinner.Text = winnerText;
            lblWinner.Opacity = 0;

            // تخمین اندازه فونت بر اساس طول متن
            int baseFontSize = 36;
            int maxLength = 20; // اگر متن بیشتر از 20 کاراکتر بود، فونت رو کوچیک‌تر کن
            double adjustedFontSize = winnerText.Length > maxLength
                ? baseFontSize * (1.0 - ((winnerText.Length - maxLength) * 0.03))
                : baseFontSize;

            // حداقل فونت رو محدود کن
            adjustedFontSize = Math.Max(adjustedFontSize, 20);

            lblWinner.FontSize = adjustedFontSize;

            // افکت‌ها
            await lblWinner.FadeTo(1, 1200, Easing.CubicIn);
            await lblWinner.ScaleTo(1.2, 500);
            await lblWinner.ScaleTo(1, 300);

        }

        [Obsolete]
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LotteryPage(_GroupId), false);
        }

        [Obsolete]
        protected override bool OnBackButtonPressed()
        {
            Navigation.PushAsync(new LotteryPage(_GroupId), false);
            return true;
        }
    }
}
