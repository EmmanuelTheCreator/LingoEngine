using System.IO;
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
    public void Copy(string text) => SdlClipboard.SetText(text);
    public string PasteClipboard() => SdlClipboard.GetText();
    public string ReadText() => File.Exists(_lingoMemberText.FileName) ? File.ReadAllText(_lingoMemberText.FileName) : string.Empty;
    public string ReadTextRtf()
    {
        var rtf = Path.ChangeExtension(_lingoMemberText.FileName, ".rtf");
        return File.Exists(rtf) ? File.ReadAllText(rtf) : string.Empty;
    }
    public void CopyToClipboard() => SdlClipboard.SetText(Text);
    public void Erase() { Unload(); }
    public void ImportFileInto() { }
    public void PasteClipboardInto() => _lingoMemberText.Text = SdlClipboard.GetText();
    public void Preload() { IsLoaded = true; }
    public void Unload() { IsLoaded = false; }
}
