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
        private readonly IAudioService _audio;
        private readonly IAudioPreferences _prefs;
        public EqualizerPage()
        {
            InitializeComponent();
            this.BindingContext = new LoginViewModel();

            _audio = DependencyService.Get<IAudioService>();
            _prefs = DependencyService.Get<IAudioPreferences>();

            BuildEqUI();
        }

        void BuildEqUI()
        {
            var bands = _audio.GetBands();
            EqStack.Children.Clear();

            for (int i = 0; i < bands.Count; i++)
            {
                // مقدار ذخیره‌شده
                short savedLevel = _prefs.GetBandLevel(i);

                var slider = new Slider
                {
                    Minimum = -1500,
                    Maximum = 1500,
                    Value = savedLevel,
                    AutomationId = $"band_{i}"
                };

                // اعمال مقدار ذخیره‌شده
                _audio.SetBandLevel(i, savedLevel);

                slider.ValueChanged += (s, e) =>
                {
                    int index = int.Parse(((Slider)s).AutomationId.Replace("band_", ""));
                    short newLevel = (short)e.NewValue;

                    _audio.SetBandLevel(index, newLevel);

                    // ذخیره در حافظه
                    _prefs.SaveBandLevel(index, newLevel);
                };

                // UI
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