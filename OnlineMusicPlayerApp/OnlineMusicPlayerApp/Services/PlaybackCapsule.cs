using System;
using Xamarin.Essentials;

public static class PlaybackCapsule
{
    public static void SaveSliderPosition(double positionMs)
    {
        Preferences.Set("last_playback_position4", positionMs);
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

    public static string LoadCurrentSeconds()
    {
        return Preferences.Get("last_playback_position4", "");
    }

    public static string LoadCurrentHref()
    {
        return Preferences.Get("last_playback_href", "");
    }

    public static string LoadCurrentTagImage()
    {
        return Preferences.Get("last_playback_TagImage", "");
    }

    public static string LoadCurrentTitle()
    {
        return Preferences.Get("last_playback_title", "");
    }

    public static int LoadCurrentAudioParentId()
    {
        return Preferences.Get("last_playback_Audio_ParentId", 0);
    }

    public static string LoadSeconds() => LoadCurrentSeconds();
    public static string LoadHref() => LoadCurrentHref();
    public static string LoadCurrentImageTag() => LoadCurrentTagImage();
    public static string LoadCurrentTitleMusic() => LoadCurrentTitle();
    public static int LoadCurrentAudioParentIds() => LoadCurrentAudioParentId();
}
