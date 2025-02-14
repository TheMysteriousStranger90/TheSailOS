namespace TheSailOSProject.Audio;

public interface IAudioManager
{
    bool IsAudioEnabled { get; }
    void Initialize();
    void Play(byte[] audioData);
    void Stop();
    void SetGain(float gain);
}