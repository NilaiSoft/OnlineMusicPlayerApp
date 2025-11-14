using Xamarin.Essentials;

public class AudioPreferences : IAudioPreferences
{
    const string KEY = "EQ_BAND_";

    public void SaveBandLevel(int index, short level)
    {
        Preferences.Set(KEY + index, level);
    }

    public short GetBandLevel(int index)
    {
        return (short)Preferences.Get(KEY + index, 0);
    }
}
