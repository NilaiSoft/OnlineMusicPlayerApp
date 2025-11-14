using Android.Media.Audiofx;
using Xamarin.Forms;
using OnlineMusicPlayerApp.Droid.Services;
using OnlineMusicPlayerApp.Services;

[assembly: Dependency(typeof(EqualizerServices))]
public class EqualizerServices : IEqualizerService
{
    private Equalizer eq;
    IAudioService _audioService;
    private IAudioService _audio;
    private IAudioPreferences _prefs;

    // بعد از Play صدا را فعال می‌کند
    public void Init()
    {
        _audioService = DependencyService.Get<IAudioService>();
        _audio = DependencyService.Get<IAudioService>();
        _prefs = DependencyService.Get<IAudioPreferences>();
        void BuildEqUI()
        {
            var bands = _audio.GetBands();

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
            }
        }
        BuildEqUI();
    }

    public void SetBand(short band, short level)
    {
        if (eq == null) return;
        eq.SetBandLevel(band, level);
    }

    public short GetBandCount()
    {
        return eq?.NumberOfBands ?? (short)0;
    }

    public int GetBandFreq(short band)
    {
        return eq.GetCenterFreq(band) / 1000;
    }
}
