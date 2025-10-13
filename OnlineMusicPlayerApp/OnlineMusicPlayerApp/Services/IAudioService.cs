public interface IAudioService
{
    void Play(string url);
    void Play(string url, double startSeconds); // نسخه با موقعیت شروع
    void Pause();
    void Resume();
    void Stop();
    bool IsPlaying();
    double GetDurationSeconds();
    double GetCurrentPositionSeconds();
    void SeekTo(long positionMs);
    void Close();
}
