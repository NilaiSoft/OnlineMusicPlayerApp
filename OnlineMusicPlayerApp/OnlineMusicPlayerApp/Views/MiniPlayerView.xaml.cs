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
            lblMiniTitle.Text = "Ehsan";//PlaybackCapsule.LoadCurrentTitle();

            var audioService = DependencyService.Get<IAudioService>();
            btnMiniPlay.ImageSource = audioService.IsPlaying() ? "icon_pause1" : "icon_play1";
        }

        private void OnMiniPlayClicked(object sender, EventArgs e)
        {
            var audioService = DependencyService.Get<IAudioService>();

            if (audioService.IsPlaying())
            {
                btnMiniPlay.ImageSource = "icon_play1";
                audioService.Pause();
                return;
            }

            string lastHref = PlaybackCapsule.LoadHref();
            if (string.IsNullOrEmpty(lastHref))
                return;

            double resumePosition = 0;
            double.TryParse(PlaybackCapsule.LoadSeconds(), out resumePosition);

            btnMiniPlay.ImageSource = "icon_pause1";
            audioService.Play(lastHref, resumePosition);
        }
    }
}
