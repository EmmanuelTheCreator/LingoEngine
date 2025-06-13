using LingoEngine.Sounds;

namespace LingoEngineSDL2.Sounds;

public class SdlSound : ILingoFrameworkSound
{
    private LingoSound _lingoSound = null!;
    private readonly List<LingoSoundDevice> _devices = new();

    public List<LingoSoundDevice> SoundDeviceList => _devices;
    public int SoundLevel { get; set; }
    public bool SoundEnable { get; set; } = true;

    internal void Init(LingoSound sound)
    {
        _lingoSound = sound;
    }

    public void Beep() { }
    public LingoSoundChannel Channel(int number) => _lingoSound.Channel(number);
    public void UpdateDeviceList() { }
}
