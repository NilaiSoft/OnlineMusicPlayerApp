public interface IAudioService
{
    void Play(string url);
    void Pause();
    void Resume();
    void Stop();
    bool IsPlaying();
    double GetDurationSeconds();
    double GetCurrentPositionSeconds();
    void SeekTo(long positionMs);
    void Close();
}
