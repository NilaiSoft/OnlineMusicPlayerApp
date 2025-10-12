using Xamarin.Essentials;

public static class PlaybackCapsule
{
    public static void SaveSliderPosition(long positionMs)
    {
        Preferences.Set("last_playback_position", positionMs);
    }

    public static void SaveCurrentUrl(string href)
    {
        Preferences.Set("last_playback_href", href);
    }

    public static void SaveCurrentTagImage(string tag)
    {
        Preferences.Set("last_playback_TagImage", tag);
    }

    public static long LoadCurrentSeconds()
    {
        return Preferences.Get("last_playback_position", 0);
    }

    public static string LoadCurrentHref()
    {
        return Preferences.Get("last_playback_href", "");
    }

    public static string LoadCurrentTagImage()
    {
        return Preferences.Get("last_playback_TagImage", "");
    }

    public static double LoadSeconds() => LoadCurrentSeconds() / 1000.0;
    public static string LoadHref() => LoadCurrentHref();
    public static string LoadCurrentImageTag() => LoadCurrentTagImage();
}
