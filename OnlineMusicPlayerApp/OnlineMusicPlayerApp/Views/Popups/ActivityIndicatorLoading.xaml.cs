using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OnlineMusicPlayerApp.Views.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityIndicatorLoading : ContentView
    {
        public ActivityIndicatorLoading()
        {
            InitializeComponent();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            activity.IsEnabled = true;
            activity.IsRunning = true;
            activity.IsVisible = true;
        }
    }
}