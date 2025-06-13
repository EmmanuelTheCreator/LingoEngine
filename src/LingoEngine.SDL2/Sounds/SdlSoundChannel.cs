using LingoEngine.SDL2.SDLL;
using LingoEngine.Sounds;
using System.Runtime.InteropServices;

namespace LingoEngine.SDL2.Sounds;

public class SdlSoundChannel : ILingoFrameworkSoundChannel, IDisposable
{
    private LingoSoundChannel _channel = null!;
    private nint _chunk = nint.Zero;
    private string? _currentFile;
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

    public void PlayFile(string stringFilePath)
    {
        Stop();
        _currentFile = stringFilePath;
        _chunk = SDL_mixer.Mix_LoadWAV(stringFilePath);
        if (_chunk == nint.Zero)
            return;
        SDL_mixer.Mix_PlayChannel(ChannelNumber, _chunk, 0);
        IsPlaying = true;
        CurrentTime = StartTime;
    }

    public void PlayNow(LingoMemberSound member)
    {
        Stop();
        _currentFile = member.FileName;
        _chunk = SDL_mixer.Mix_LoadWAV(_currentFile);
        if (_chunk == nint.Zero)
            return;
        SDL_mixer.Mix_PlayChannel(ChannelNumber, _chunk, 0);
        IsPlaying = true;
        CurrentTime = StartTime;
    }

    public void Stop()
    {
        SDL_mixer.Mix_HaltChannel(ChannelNumber);
        if (_chunk != nint.Zero)
        {
            SDL_mixer.Mix_FreeChunk(_chunk);
            _chunk = nint.Zero;
        }
        IsPlaying = false;
        CurrentTime = 0;
    }

    public void Rewind()
    {
        SDL_mixer.Mix_HaltChannel(ChannelNumber);
        CurrentTime = 0;
    }

    public void Pause()
    {
        if (IsPlaying)
            SDL_mixer.Mix_Pause(ChannelNumber);
        else
            SDL_mixer.Mix_Resume(ChannelNumber);
        IsPlaying = !IsPlaying;
    }

    public void Repeat()
    {
        if (_chunk != nint.Zero)
        {
            SDL_mixer.Mix_HaltChannel(ChannelNumber);
            SDL_mixer.Mix_PlayChannel(ChannelNumber, _chunk, -1);
            IsPlaying = true;
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
