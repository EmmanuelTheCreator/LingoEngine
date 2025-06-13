using LingoEngine.Pictures;
using LingoEngine.Pictures.LingoEngine;

namespace LingoEngineSDL2.Pictures;

public class SdlMemberPicture : ILingoFrameworkMemberPicture, IDisposable
{
    private LingoMemberPicture _member = null!;
    public byte[]? ImageData { get; private set; }
    public bool IsLoaded { get; private set; }
    public string Format { get; private set; } = "image/unknown";
    public int Width { get; private set; }
    public int Height { get; private set; }

    internal void Init(LingoMemberPicture member)
    {
        _member = member;
    }

    public void Preload() { IsLoaded = true; }
    public void Unload() { IsLoaded = false; }
    public void Erase() { Unload(); ImageData = null; }
    public void Dispose() { }
    public void CopyToClipboard() { }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
}
