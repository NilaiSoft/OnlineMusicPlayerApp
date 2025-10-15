using System;
using Xamarin.Forms;
using OnlineMusicPlayerApp.Extensions;
using OnlineMusicPlayerApp.Services;

namespace OnlineMusicPlayerApp.Views
{
    public partial class MiniPlayerView : ContentView
    {
        public MiniPlayerView()
        {
            InitializeComponent();
            LoadLastPlaybackInfo();
        }

        private void LoadLastPlaybackInfo()
        {
            lblMiniTitle.Text = PlaybackCapsule.LoadCurrentTitle();
            imgMiniCover.Source = PlaybackCapsule.LoadCurrentImageTag();

            var audioService = DependencyService.Get<IAudioService>();
            btnMiniPlay.Source = audioService.IsPlaying() ? "icon_pause1" : "icon_play1";
        }

        private void OnMiniPlayClicked(object sender, EventArgs e)
        {
            var audioService = DependencyService.Get<IAudioService>();

            btnMiniPlay.Source = audioService.IsPlaying() ? "icon_pause1" : "icon_play1";

            if (audioService.IsPlaying())
            {
                audioService.Pause();
                return;
            }

            string lastHref = PlaybackCapsule.LoadHref();
            if (string.IsNullOrEmpty(lastHref))
                return;

            double resumePosition = 0;
            double.TryParse(PlaybackCapsule.LoadSeconds(), out resumePosition);

            btnMiniPlay.Source = "icon_pause1";
            audioService.Play(lastHref, resumePosition);

        }

        private void OnMiniNextClicked(object sender, EventArgs e)
        {

        }

        private void OnMiniPrevClicked(object sender, EventArgs e)
        {

        }
    }
}
