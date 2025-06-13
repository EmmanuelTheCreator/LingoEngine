using System;
using System.Runtime.InteropServices;
using LingoEngine.Sounds;
using LingoEngine.SDL2.SDLL;

namespace LingoEngine.SDL2.Sounds;

public class SdlSound : ILingoFrameworkSound
{
    private LingoSound _lingoSound = null!;
    private readonly List<LingoSoundDevice> _devices = new();
    private bool _initialized;

    public List<LingoSoundDevice> SoundDeviceList => _devices;
    public int SoundLevel { get; set; }
    public bool SoundEnable { get; set; } = true;

    internal void Init(LingoSound sound)
    {
        _lingoSound = sound;
        if (!_initialized)
        {
            SDL_mixer.Mix_OpenAudio(44100, SDL.AUDIO_S16LSB, 2, 2048);
            _initialized = true;
        }
    }

    public void Beep() => Console.Beep();
    public LingoSoundChannel Channel(int number) => _lingoSound.Channel(number);
    public void UpdateDeviceList()
    {
        _devices.Clear();
        int count = SDL.SDL_GetNumAudioDevices(0);
        for (int i = 0; i < count; i++)
        {
            var name = SDL.SDL_GetAudioDeviceName(i, 0);
            _devices.Add(new LingoSoundDevice(i, name ?? string.Empty));
        }
    }
}
