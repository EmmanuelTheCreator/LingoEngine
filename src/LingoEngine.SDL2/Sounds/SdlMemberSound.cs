using System.IO;
using System.Runtime.InteropServices;
using LingoEngine.Sounds;
using LingoEngine.SDL2.SDLL;

namespace LingoEngine.SDL2.Sounds;

public class SdlMemberSound : ILingoFrameworkMemberSound, IDisposable
{
    private LingoMemberSound _member = null!;
    private nint _chunk = nint.Zero;
    public bool Stereo { get; private set; }
    public double Length { get; private set; }
    public bool IsLoaded { get; private set; }

    internal void Init(LingoMemberSound member)
    {
        _member = member;
    }
    public void Dispose() { Unload(); }
    public void CopyToClipboard() { }
    public void Erase() { Unload(); }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void Preload()
    {
        if (IsLoaded) return;
        if (!File.Exists(_member.FileName))
            return;

        _chunk = SDL_mixer.Mix_LoadWAV(_member.FileName);
        if (_chunk == nint.Zero)
            return;

        var fi = new FileInfo(_member.FileName);
        _member.Size = fi.Length;
        Length = fi.Length / 44100.0;
        Stereo = true;
        IsLoaded = true;
    }
    public void Unload()
    {
        if (_chunk != nint.Zero)
        {
            SDL_mixer.Mix_FreeChunk(_chunk);
            _chunk = nint.Zero;
        }
        IsLoaded = false;
    }
}
