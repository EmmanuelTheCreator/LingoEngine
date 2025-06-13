using LingoEngine.FrameworkCommunication.Events;
using LingoEngine.Primitives;
using LingoEngine.Texts;

namespace LingoEngineSDL2.Texts;

public abstract class SdlMemberTextBase<TText> : ILingoFrameworkMemberTextBase, IDisposable where TText : ILingoMemberTextBase
{
    protected TText _lingoMemberText = default!;
    protected string _text = string.Empty;
    protected ILingoFontManager _fontManager;

    protected SdlMemberTextBase(ILingoFontManager fontManager)
    {
        _fontManager = fontManager;
    }

    internal void Init(TText member)
    {
        _lingoMemberText = member;
    }

    public string Text { get => _text; set => _text = value; }
    public bool WordWrap { get; set; }
    public int ScrollTop { get; set; }
    public string FontName { get; set; } = string.Empty;
    public int FontSize { get; set; }
    public LingoTextStyle FontStyle { get; set; }
    public LingoColor TextColor { get; set; } = LingoColor.FromRGB(0,0,0);
    public LingoTextAlignment Alignment { get; set; }
    public int Margin { get; set; }
    public bool IsLoaded { get; private set; }

    public void Dispose() { }
    public void Copy(string text) { }
    public string PasteClipboard() => string.Empty;
    public string ReadText() => string.Empty;
    public string ReadTextRtf() => string.Empty;
    public void CopyToClipboard() { }
    public void Erase() { }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void Preload() { IsLoaded = true; }
    public void Unload() { IsLoaded = false; }
}
