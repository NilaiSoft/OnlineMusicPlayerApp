using OnlineMusicPlayerApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OnlineMusicPlayerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EqualizerPage : ContentPage
    {
        public EqualizerPage()
        {
            InitializeComponent();
            this.BindingContext = new LoginViewModel();
            BuildEqUI();
        }

        void BuildEqUI()
        {
            var bands = DependencyService.Get<IAudioService>().GetBands();
            EqStack.Children.Clear();

            for (int i = 0; i < bands.Count; i++)
            {
                var slider = new Slider
                {
                    Minimum = -1500,
                    Maximum = 1500,
                    Value = 0,
                    AutomationId = $"band_{i}"
                };

                slider.ValueChanged += (s, e) =>
                {
                    int index = int.Parse(((Slider)s).AutomationId.Replace("band_", ""));
                    DependencyService.Get<IAudioService>().SetBandLevel(index, (short)e.NewValue);
                };

                EqStack.Children.Add(new StackLayout
                {
                    Children =
                    {
                        new Label { Text = bands[i], FontSize = 14 },
                        slider
                    }
                });
            }
        }
    }
}