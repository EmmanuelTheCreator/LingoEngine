using System;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
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
    public float SkewX { get; set; }
    public float SkewY { get; set; }

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

        var baseX = X + offset.X;
        var baseY = Y + offset.Y;
        if (Math.Abs(SkewX) < 0.0001f && Math.Abs(SkewY) < 0.0001f)
        {
            SDL.SDL_FRect dst = new SDL.SDL_FRect
            {
                x = baseX,
                y = baseY,
                w = Width,
                h = Height
            };
            SDL.SDL_FPoint center = new SDL.SDL_FPoint { x = Width / 2, y = Height / 2 };
            SDL.SDL_RenderCopyExF(renderer, _texture, nint.Zero, ref dst, Rotation, ref center, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
        }
        else
        {
            float radRot = Rotation * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(radRot);
            float sin = (float)Math.Sin(radRot);
            float tanX = (float)Math.Tan(SkewX * Math.PI / 180f);
            float tanY = (float)Math.Tan(SkewY * Math.PI / 180f);

            SDL.SDL_Vertex[] verts = new SDL.SDL_Vertex[4];
            float[] xs = new float[] { 0, Width, Width, 0 };
            float[] ys = new float[] { 0, 0, Height, Height };
            float cx = Width / 2f;
            float cy = Height / 2f;
            for (int i = 0; i < 4; i++)
            {
                float vx = xs[i] - cx;
                float vy = ys[i] - cy;
                float sx = vx + vy * tanX;
                float sy = vy + vx * tanY;
                float rx = sx * cos - sy * sin;
                float ry = sx * sin + sy * cos;

                verts[i].position = new SDL.SDL_FPoint { x = baseX + cx + rx, y = baseY + cy + ry };
                verts[i].tex_coord = new SDL.SDL_FPoint { x = xs[i] / Width, y = ys[i] / Height };
                verts[i].color = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };
            }
            int[] indices = new int[] { 0, 1, 2, 0, 2, 3 };
            SDL.SDL_RenderGeometry(renderer, _texture, verts, verts.Length, indices, indices.Length);
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
