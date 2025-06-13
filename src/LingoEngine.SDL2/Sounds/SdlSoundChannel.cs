using LingoEngine.Sounds;

namespace LingoEngineSDL2.Sounds;

public class SdlSoundChannel : ILingoFrameworkSoundChannel, IDisposable
{
    private LingoSoundChannel _channel = null!;
    public int ChannelNumber { get; }
    public int SampleRate => 44100;
    public int Volume { get; set; }
    public int Pan { get; set; }
    public float CurrentTime { get; set; }
    public bool IsPlaying { get; private set; }
    public int ChannelCount => 2;
    public int SampleCount => 0;
    public float ElapsedTime => CurrentTime;
    public float StartTime { get; set; }
    public float EndTime { get; set; }

    public SdlSoundChannel(int number)
    {
        ChannelNumber = number;
    }
    internal void Init(LingoSoundChannel channel)
    {
        _channel = channel;
    }

    public void PlayFile(string stringFilePath) { IsPlaying = true; }
    public void PlayNow(LingoMemberSound member) { IsPlaying = true; }
    public void Stop() { IsPlaying = false; }
    public void Rewind() { CurrentTime = 0; }
    public void Pause() { IsPlaying = !IsPlaying; }
    public void Repeat() { CurrentTime = StartTime; IsPlaying = true; }
    public void Dispose() { }
}
