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

    Equalizer equalizer;
    BassBoost bassBoost;
    Virtualizer virtualizer;

    public void InitAudioEffects()
    {
        if (MusicService.player == null)
            return;

        int session = MusicService.player.AudioSessionId;
        if (session <= 0)
            return;

        eq = new Equalizer(0, session);
        eq.SetEnabled(true);

        short bandCount = eq.NumberOfBands;

        for (short i = 0; i < bandCount; i++)
        {
            int freq = eq.GetCenterFreq(i) / 1000;

            if (freq >= 6000)
                eq.SetBandLevel(i, 1200);

            else if (freq >= 1000 && freq < 6000)
                eq.SetBandLevel(i, 600);

            else if (freq < 250)
                eq.SetBandLevel(i, 800);
        }

        bassBoost = new BassBoost(0, session);
        bassBoost.SetStrength(800);
        bassBoost.SetEnabled(true);

        virtualizer = new Virtualizer(1, session);
        virtualizer.SetStrength(900);
        virtualizer.SetEnabled(true);
    }
}
