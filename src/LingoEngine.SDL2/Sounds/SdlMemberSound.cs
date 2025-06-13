using LingoEngine.Sounds;

namespace LingoEngineSDL2.Sounds;

public class SdlMemberSound : ILingoFrameworkMemberSound, IDisposable
{
    private LingoMemberSound _member = null!;
    public bool Stereo { get; private set; }
    public double Length { get; private set; }
    internal void Init(LingoMemberSound member)
    {
        _member = member;
    }
    public void Dispose() { }
    public void CopyToClipboard() { }
    public void Erase() { }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void Preload() { }
    public void Unload() { }
    public bool IsLoaded => true;
}
