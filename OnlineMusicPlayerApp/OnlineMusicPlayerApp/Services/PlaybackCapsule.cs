using System;
using Xamarin.Essentials;

public static class PlaybackCapsule
{
    public static void SaveSliderPosition(double positionMs)
    {
        Preferences.Set("last_playback_position4", positionMs);
    }

    public static void SaveLastIndex(int index)
    {
        Preferences.Set("last_playback_lastindex", index);
    }

    public static void SaveTitle(string title)
    {
        Preferences.Set("last_playback_title", title);
    }

    public static void SaveCurrentUrl(string href)
    {
        Preferences.Set("last_playback_href", href);
    }

    public static void SaveCurrentTagImage(string tag)
    {
        Preferences.Set("last_playback_TagImage", tag);
    }

    public static void SaveCurrentAudioParentId(int parentId)
    {
        Preferences.Set("last_playback_Audio_ParentId", parentId);
    }

    private static string LoadCurrentSeconds()
    {
        return Preferences.Get("last_playback_position4", "");
    }

    private static string LoadCurrentHref()
    {
        return Preferences.Get("last_playback_href", "");
    }

    private static string LoadCurrentTagImage()
    {
        return Preferences.Get("last_playback_TagImage", "");
    }

    private static string LoadCurrentTitle()
    {
        return Preferences.Get("last_playback_title", "");
    }

    private static int LoadCurrentAudioParentId()
    {
        return Preferences.Get("last_playback_Audio_ParentId", 0);
    }

    private static int LoadLastIndex()
    {
        return Preferences.Get("last_playback_lastindex", 0);
    }

    public static string LoadSeconds() => LoadCurrentSeconds();
    public static string LoadHref() => LoadCurrentHref();
    public static string LoadCurrentImageTag() => LoadCurrentTagImage();
    public static string LoadCurrentTitleMusic() => LoadCurrentTitle();
    public static int LoadCurrentAudioParentIds() => LoadCurrentAudioParentId();
    public static int LoadLastIndexs() => LoadLastIndex();
}
