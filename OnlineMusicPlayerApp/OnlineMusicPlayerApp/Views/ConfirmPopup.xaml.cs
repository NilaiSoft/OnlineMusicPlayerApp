using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using System.Threading.Tasks;
using System;
using Rg.Plugins.Popup.Enums;
using Rg.Plugins.Popup.Animations;

namespace OnlineMusicPlayerApp.Views
{
    public partial class ConfirmPopup : PopupPage
    {
        private TaskCompletionSource<bool> _taskCompletionSource;

        public ConfirmPopup(string message)
        {
            InitializeComponent();
            MessageLabel.Text = message;
            _taskCompletionSource = new TaskCompletionSource<bool>();

            Animation = new ScaleAnimation
            {
                PositionIn = MoveAnimationOptions.Center,
                ScaleIn = 1.2,
                DurationIn = 300,
                EasingIn = Easing.CubicIn
            };

            _taskCompletionSource = new TaskCompletionSource<bool>();
        }

        public Task<bool> ShowAsync()
        {
            return _taskCompletionSource.Task;
        }

        private void OnYesClicked(object sender, EventArgs e)
        {
            _taskCompletionSource.TrySetResult(true);
            PopupNavigation.Instance.PopAsync();
        }

        private void OnNoClicked(object sender, EventArgs e)
        {
            _taskCompletionSource.TrySetResult(false);
            PopupNavigation.Instance.PopAsync();
        }
    }
}
