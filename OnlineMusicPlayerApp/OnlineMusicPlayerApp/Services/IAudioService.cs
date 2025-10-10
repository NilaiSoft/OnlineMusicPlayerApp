public interface IAudioService
{
    void Play(string url);
    void Pause();
    void Resume();
    void Stop();
    bool IsPlaying();
}
