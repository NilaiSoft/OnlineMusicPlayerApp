public interface IAudioPreferences
{
    void SaveBandLevel(int index, short level);
    short GetBandLevel(int index);
}
