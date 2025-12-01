using Android.Media.Audiofx;
using Xamarin.Forms;
using OnlineMusicPlayerApp.Droid.Services;
using OnlineMusicPlayerApp.Services;
using Java.Util.Prefs;
using System.Collections.Generic;
using Android.Content;

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

        bassBoost = bassBoost ?? new BassBoost(0, session);
        bassBoost.SetStrength(800);
        bassBoost.SetEnabled(true);

        virtualizer = virtualizer ?? new Virtualizer(1, session);
        virtualizer.SetStrength(900);
        virtualizer.SetEnabled(true);
    }

    public void InitAudioEffects2()
    {
        //    if (MusicService.player == null)
        //        return;

        //    int session = MusicService.player.AudioSessionId;
        //    if (session <= 0)
        //        return;

        //    // 1️⃣ Equalizer اصلی (برای Sliderها)
        //    equalizer = new Equalizer(0, session);
        //    eq.SetEnabled(true);

        //    // 2️⃣ اعمال تنظیمات ذخیره شده یوزر
        //    var bands = new List<string>();
        //    short bandCount = equalizer.NumberOfBands;

        //    for (short i = 0; i < bandCount; i++)
        //    {
        //        int freq = equalizer.GetCenterFreq(i) / 1000;

        //        // مقدار ذخیره شده را به صورت string می‌گیریم
        //        ISharedPreferences prefs = Android.App.Application.Context
        //.GetSharedPreferences("eqprefs", FileCreationMode.Private);

        //        string savedStr = prefs.GetString($"band_{i}", "0");
        //        short savedValue = short.TryParse(savedStr, out var v) ? v : (short)0;

        //        // تبدیل رشته به عدد
        //        savedValue = short.TryParse(savedStr, out v) ? v : (short)0;

        //        // اعمال مقدار روی اکولایزر
        //        equalizer.SetBandLevel(i, savedValue);

        //        // نمایش 
        //        bands.Add($"{freq} Hz : {savedValue}");
        //    }

        //    // 3️⃣ Crystalizer — تقویت باس، Treble و Virtualizer
        //    ApplyCrystalizerPreset();
    }

    //private void ApplyCrystalizerPreset()
    //{
    //    if (equalizer == null) return;

    //    short bands = equalizer.NumberOfBands;

    //    for (short i = 0; i < bands; i++)
    //    {
    //        int freq = equalizer.GetCenterFreq(i) / 1000;

    //        // Bass Crystalizer
    //        if (freq < 250)
    //        {
    //            short level = (short)(equalizer.GetBandLevel(i) + 900);
    //            equalizer.SetBandLevel(i, level);
    //        }

    //        // Mid Boost
    //        else if (freq >= 1000 && freq < 4000)
    //        {
    //            short level = (short)(equalizer.GetBandLevel(i) + 500);
    //            equalizer.SetBandLevel(i, level);
    //        }

    //        // Treble Crystalizer
    //        else if (freq >= 6000)
    //        {
    //            short level = (short)(equalizer.GetBandLevel(i) + 1200);
    //            equalizer.SetBandLevel(i, level);
    //        }
    //    }

    //    // Bass Boost
    //    bassBoost = new BassBoost(0, MusicService.player.AudioSessionId);
    //    bassBoost.SetStrength(900);
    //    bassBoost.SetEnabled(true);

    //    // Virtualizer
    //    virtualizer = new Virtualizer(1, MusicService.player.AudioSessionId);
    //    virtualizer.SetStrength(900);
    //    virtualizer.SetEnabled(true);
    //}
}
