using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.Primitives;
using LingoEngine.Texts;

namespace LingoEngineSDL2;

public class SdlSprite : ILingoFrameworkSprite, IDisposable
{
    private readonly Action<SdlSprite> _show;
    private readonly Action<SdlSprite> _hide;
    private readonly Action<SdlSprite> _remove;
    private readonly LingoSprite _lingoSprite;
    internal bool IsDirty { get; set; } = true;
    internal bool IsDirtyMember { get; set; } = true;

    public SdlSprite(LingoSprite sprite, Action<SdlSprite> show, Action<SdlSprite> hide, Action<SdlSprite> remove)
    {
        _lingoSprite = sprite;
        _show = show;
        _hide = hide;
        _remove = remove;
        sprite.Init(this);
        ZIndex = sprite.SpriteNum;
    }

    public bool Visibility { get; set; }
    public float Blend { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; private set; }
    public float Height { get; private set; }
    public string Name { get; set; } = string.Empty;
    public LingoPoint RegPoint { get; set; }
    public float SetDesiredHeight { get; set; }
    public float SetDesiredWidth { get; set; }
    public int ZIndex { get; set; }

    public void RemoveMe() { _remove(this); Dispose(); }
    public void Dispose() { }
    public void Show() { _show(this); Update(); }
    public void Hide() { _hide(this); }
    public void SetPosition(LingoPoint point) { X = point.X; Y = point.Y; }

    public void MemberChanged() { IsDirtyMember = true; }

    internal void Update() { }
    public void Resize(float w, float h) { Width = w; Height = h; }
}
