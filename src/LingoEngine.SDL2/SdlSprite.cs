using LingoEngine.FrameworkCommunication;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Pictures;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Pictures;
using LingoEngine.SDL2.SDLL;
using LingoEngine.SDL2.Texts;
using LingoEngine.Texts;

namespace LingoEngine.SDL2;

public class SdlSprite : ILingoFrameworkSprite, IDisposable
{
    private readonly Action<SdlSprite> _show;
    private readonly Action<SdlSprite> _hide;
    private readonly Action<SdlSprite> _remove;
    private readonly LingoSprite _lingoSprite;
    internal bool IsDirty { get; set; } = true;
    internal bool IsDirtyMember { get; set; } = true;

    private readonly nint _renderer;
    private nint _texture = nint.Zero;

    public SdlSprite(LingoSprite sprite, nint renderer, Action<SdlSprite> show, Action<SdlSprite> hide, Action<SdlSprite> remove)
    {
        _lingoSprite = sprite;
        _renderer = renderer;
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
    public float Rotation { get; set; }
    public float Skew { get; set; }

    public void RemoveMe() { _remove(this); Dispose(); }
    public void Dispose()
    {
        if (_texture != nint.Zero)
        {
            SDL.SDL_DestroyTexture(_texture);
            _texture = nint.Zero;
        }
    }
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

    internal void Render(nint renderer)
    {
        // todo : rotation
        // todo : skew
        if (_texture == nint.Zero) return;
        var offset = new LingoPoint();
        if (_lingoSprite.Member is { } member)
        {
            var baseOffset = member.CenterOffsetFromRegPoint();
            if (member.Width != 0 && member.Height != 0)
            {
                float scaleX = Width / member.Width;
                float scaleY = Height / member.Height;
                offset = new LingoPoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);
            }
            else
            {
                offset = baseOffset;
            }
        }

        SDL.SDL_Rect dst = new SDL.SDL_Rect
        {
            x = (int)(X + offset.X),
            y = (int)(Y + offset.Y),
            w = (int)Width,
            h = (int)Height
        };
        SDL.SDL_RenderCopy(renderer, _texture, nint.Zero, ref dst);
    }

    private void UpdateMember()
    {
        IsDirtyMember = false;
        switch (_lingoSprite.Member)
        {
            case LingoMemberPicture pic:
                var p = pic.Framework<SdlMemberPicture>();
                p.Preload();
                if (_texture != nint.Zero)
                {
                    SDL.SDL_DestroyTexture(_texture);
                    _texture = nint.Zero;
                }

                if (p.Surface != nint.Zero)
                {
                    _texture = SDL.SDL_CreateTextureFromSurface(_renderer, p.Surface);
                    if (_texture != nint.Zero)
                    {
                        Width = p.Width;
                        Height = p.Height;
                    }
                }
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
