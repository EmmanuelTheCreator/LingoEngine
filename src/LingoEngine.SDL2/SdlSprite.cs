using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.Primitives;
using LingoEngine.Texts;
using LingoEngineSDL2.Pictures;
using LingoEngineSDL2.Texts;

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

    internal void Update()
    {
        if (IsDirtyMember)
            UpdateMember();
        if (IsDirty)
        {
            if (SetDesiredWidth != 0) Width = SetDesiredWidth;
            if (SetDesiredHeight != 0) Height = SetDesiredHeight;
            IsDirty = false;
        }
    }

    private void UpdateMember()
    {
        IsDirtyMember = false;
        switch (_lingoSprite.Member)
        {
            case LingoMemberPicture pic:
                var p = pic.Framework<SdlMemberPicture>();
                p.Preload();
                Width = p.Width;
                Height = p.Height;
                break;
            case LingoMemberText text:
                text.Framework<SdlMemberText>().Preload();
                break;
            case LingoMemberField field:
                field.Framework<SdlMemberField>().Preload();
                break;
        }
    }

    public void Resize(float w, float h) { Width = w; Height = h; }
}
