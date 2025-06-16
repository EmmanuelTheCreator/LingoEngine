using System;
using LingoEngine.Pictures;
using LingoEngine.SDL2;

namespace LingoEngine.SDL2.Pictures;

public class SdlMemberFilmLoop : ILingoFrameworkMemberFilmLoop, IDisposable
{
    private LingoMemberFilmLoop _member = null!;
    public bool IsLoaded { get; private set; }
    public byte[]? Media { get; set; }
    public LingoFilmLoopFraming Framing { get; set; } = LingoFilmLoopFraming.Auto;
    public bool Loop { get; set; } = true;

    internal void Init(LingoMemberFilmLoop member)
    {
        _member = member;
    }

    public void Preload()
    {
        IsLoaded = true;
    }

    public void Unload()
    {
        IsLoaded = false;
    }

    public void Erase()
    {
        Media = null;
        Unload();
    }

    public void ImportFileInto()
    {
        // not implemented
    }

    public void CopyToClipboard()
    {
        if (Media == null) return;
        var base64 = Convert.ToBase64String(Media);
        SdlClipboard.SetText(base64);
    }

    public void PasteClipboardInto()
    {
        var data = SdlClipboard.GetText();
        if (string.IsNullOrEmpty(data)) return;
        try
        {
            Media = Convert.FromBase64String(data);
        }
        catch
        {
        }
    }

    public void Dispose() { Unload(); }
}
