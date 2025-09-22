using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AbstUI.Primitives;
using AbstUI.Texts;
using BlingoEngine.Casts;
using BlingoEngine.Members;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using BlingoEngine.Texts;

namespace BlingoEngine.Tests.Casts;

internal class DummyTextMember : IBlingoMemberTextBase, IBlingoMemberTextBaseInteral
{
    // fields
    private readonly IBlingoCast _cast;
    private readonly DummyFrameworkMember _framework = new();

    // properties
    public IBlingoFrameworkMember FrameworkObj => _framework;
    public string Name { get; set; } = string.Empty;
    public int Number => 0;
    public DateTime CreationDate => DateTime.Now;
    public DateTime ModifiedDate => DateTime.Now;
    public bool Hilite => false;
    public int CastLibNum => 0;
    public int NumberInCast => 0;
    public int PurgePriority { get; set; }
    public APoint RegPoint { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long Size { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string FileName { get; set; }
    public BlingoMemberType Type { get; set; } = BlingoMemberType.Text;
    public string CastName => string.Empty;
    public IBlingoCast Cast => _cast;
    public string Text { get; set; } = string.Empty;
    public BlingoLines Line => new(() => { });
    public BlingoWords Word => new(Text);
    public BlingoChars Char => new();
    public bool Editable { get; set; }
    public bool WordWrap { get; set; }
    public int ScrollTop { get; set; }
    public string Font { get; set; } = string.Empty;
    public int FontSize { get; set; }
    public BlingoTextStyle FontStyle { get; set; }
    public AColor Color { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public AbstTextAlignment Alignment { get; set; }
    public int Margin { get; set; }
    public bool Preloaded { get; private set; }
    public bool LoadFileCalled { get; private set; }
    public AbstMarkdownData? MarkdownData { get; private set; }

    // constructors
    public DummyTextMember(IBlingoCast cast) => _cast = cast;

    // methods
    public void CopyToClipBoard() { }
    public void Erase() { }
    public void ImportFileInto() { }
    public void Move() { }
    public void PasteClipBoardInto() { }
    public void Preload() { Preloaded = true; }
    public Task PreloadAsync()
    {
        Preload();
        return Task.CompletedTask;
    }
    public void Unload() { }
    public IBlingoMember Duplicate(int? newNumber = null) => this;
    public IBlingoMember? GetMemberInCastByOffset(int numberOffset) => null;
    public bool IsPixelTransparent(int x, int y) => false;
    public void Dispose() { }
    public IAbstTexture2D? TextureBlingo => null;
    public IAbstTexture2D? RenderToTexture(BlingoInkType ink, AColor transparentColor) => null;
    public void SetTextMD(AbstMarkdownData data)
    {
        Text = data.Markdown;
        MarkdownData = data;
    }
    public void Clear() { Text = string.Empty; }
    public void Copy() { }
    public void Cut() { }
    public void Paste() { }
    public void InsertText(string text) { }
    public void ReplaceSelection(string replacement) { }
    public void SetSelection(int start, int end) { }
    public void RequireRedraw() { }
    public void InitDefaults() { }
    public IAbstTexture2D? GetTexture() => null;
    public void ChangesHasBeenApplied() { }
    public void LoadFile() { LoadFileCalled = true; }
    public void SetFileName(string name) => _fileName = name;

    public string GetTextMDString()
    {
        return "";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private class DummyFrameworkMember : IBlingoFrameworkMember
    {
        public bool IsLoaded => true;
        public void CopyToClipboard() { }
        public void Erase() { }
        public void ImportFileInto() { }
        public void PasteClipboardInto() { }
        public void Preload() { }
        public Task PreloadAsync() => Task.CompletedTask;
        public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }
        public void Unload() { }
        public bool IsPixelTransparent(int x, int y) => false;
    }
}


