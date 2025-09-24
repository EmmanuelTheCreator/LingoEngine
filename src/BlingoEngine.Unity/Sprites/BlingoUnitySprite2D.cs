using System;
using AbstUI.LUnity.Bitmaps;
using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Members;
using BlingoEngine.Sprites;
using BlingoEngine.Texts;
using BlingoEngine.Shapes;
using BlingoEngine.FilmLoops;
using BlingoEngine.Medias;
using BlingoEngine.Unity.Bitmaps;
using BlingoEngine.Unity.Texts;
using BlingoEngine.Unity.Shapes;
using BlingoEngine.Unity.FilmLoops;
using BlingoEngine.Unity.Medias;
using UnityEngine;
using AbstUI.Components;

namespace BlingoEngine.Unity.Sprites;

/// <summary>
/// Unity sprite wrapper used by the engine.
/// Mirrors the behaviour of the Godot implementation.
/// </summary>
public class BlingoUnitySprite2D : IBlingoFrameworkSprite, IBlingoFrameworkSpriteVideo, IDisposable
{
    // Fields
    private readonly GameObject _go;
    private readonly SpriteRenderer _renderer;
    private readonly Action<BlingoUnitySprite2D> _show;
    private readonly Action<BlingoUnitySprite2D> _hide;
    private readonly Action<BlingoUnitySprite2D> _remove;
    private readonly BlingoSprite2D _blingoSprite;

    private UnityTexture2D? _texture;
    private bool _visible;
    private float _blend = 100f;
    private float _x;
    private float _y;
    private string _name = string.Empty;
    private APoint _regPoint;
    private float _desiredWidth;
    private float _desiredHeight;
    private int _zIndex;
    private float _rotation;
    private bool _flipH;
    private bool _flipV;
    private bool _directToStage;
    private int _ink;
    private BlingoFilmLoopPlayer? _filmLoopPlayer;
    private VideoPlayer? _videoPlayer;
    private AMargin _margin = AMargin.Zero;

    internal bool IsDirty { get; set; } = true;
    internal bool IsDirtyMember { get; set; } = true;

    // Properties
    public bool Visibility
    {
        get => _visible;
        set
        {
            if (_visible == value) return;
            _visible = value;
            _go.SetActive(value);
            if (value) _show(this); else _hide(this);
        }
    }

    public float Blend
    {
        get => _blend;
        set { _blend = value; ApplyBlend(); }
    }

    public float X { get => _x; set { _x = value; IsDirty = true; } }
    public float Y { get => _y; set { _y = value; IsDirty = true; } }

    public float Width { get; private set; }
    public float Height { get; private set; }

    public string Name { get => _name; set { _name = value; _go.name = value; } }
    public APoint RegPoint { get => _regPoint; set { _regPoint = value; IsDirty = true; } }
    public float DesiredHeight { get => _desiredHeight; set { _desiredHeight = value; IsDirty = true; } }
    public float DesiredWidth { get => _desiredWidth; set { _desiredWidth = value; IsDirty = true; } }

    public int ZIndex
    {
        get => _zIndex;
        set { _zIndex = value; ApplyZIndex(); }
    }

    public float Rotation { get => _rotation; set { _rotation = value; IsDirty = true; } }
    public float Skew { get; set; }

    public bool FlipH { get => _flipH; set { _flipH = value; IsDirty = true; } }
    public bool FlipV { get => _flipV; set { _flipV = value; IsDirty = true; } }

    public bool DirectToStage
    {
        get => _directToStage;
        set { _directToStage = value; ApplyZIndex(); ApplyBlend(); }
    }

    public int Ink { get => _ink; set { _ink = value; } }

    public AMargin Margin
    {
        get => _margin;
        set
        {
            _margin = value;
            IsDirty = true;
        }
    }

    // Constructor
    public BlingoUnitySprite2D(BlingoSprite2D sprite, Transform parent,
        Action<BlingoUnitySprite2D> show, Action<BlingoUnitySprite2D> hide, Action<BlingoUnitySprite2D> remove)
    {
        _show = show;
        _hide = hide;
        _remove = remove;
        _blingoSprite = sprite;

        _go = new GameObject(sprite.Name);
        _go.transform.parent = parent;
        _renderer = _go.AddComponent<SpriteRenderer>();

        sprite.Init(this);
        _name = sprite.Name;
        _zIndex = sprite.SpriteNum;
        _directToStage = sprite.DirectToStage;
        _ink = sprite.Ink;

        ApplyZIndex();
        ApplyBlend();
    }

    // Methods
    private void ApplyZIndex()
    {
        _renderer.sortingOrder = _directToStage ? 100000 + _zIndex : _zIndex;
    }

    private void ApplyBlend()
    {
        var c = _renderer.color;
        float alpha = Mathf.Clamp(_blend / 100f, 0f, 1f);
        c.a = _directToStage ? 1f : alpha;
        _renderer.color = c;
    }

    public void Resize(float w, float h)
    {
        DesiredWidth = w;
        DesiredHeight = h;
        IsDirty = true;
    }

    public void MemberChanged()
    {
        if (_blingoSprite.Member is { } member)
        {
            var (_, sourceWidth, sourceHeight) = _blingoSprite.GetMemberSourceMetrics();
            if (sourceWidth <= 0)
                sourceWidth = member.Width;
            if (sourceHeight <= 0)
                sourceHeight = member.Height;

            if (Width == 0)
                Width = sourceWidth;
            if (Height == 0)
                Height = sourceHeight;
        }
        IsDirtyMember = true;
    }

    internal void Update()
    {
        if (IsDirtyMember)
            UpdateMember();

        if (!IsDirty) return;
        IsDirty = false;

        var (baseOffset, sourceWidth, sourceHeight) = _blingoSprite.GetMemberSourceMetrics();

        float textureWidth = _texture?.Texture?.width ?? Width;
        float textureHeight = _texture?.Texture?.height ?? Height;

        float targetWidth = _desiredWidth == 0
            ? (sourceWidth > 0 ? sourceWidth : textureWidth)
            : _desiredWidth;
        float targetHeight = _desiredHeight == 0
            ? (sourceHeight > 0 ? sourceHeight : textureHeight)
            : _desiredHeight;

        if (sourceWidth <= 0)
            sourceWidth = targetWidth != 0 ? targetWidth : 1f;
        if (sourceHeight <= 0)
            sourceHeight = targetHeight != 0 ? targetHeight : 1f;

        float scaleMagnitudeX = sourceWidth != 0 ? targetWidth / sourceWidth : 1f;
        float scaleMagnitudeY = sourceHeight != 0 ? targetHeight / sourceHeight : 1f;

        var offset = new Vector3(baseOffset.X * scaleMagnitudeX, baseOffset.Y * scaleMagnitudeY, 0f);
        _go.transform.localPosition = new Vector3(_x - offset.x, _y - offset.y, 0f);
        _go.transform.localEulerAngles = new Vector3(0, 0, _rotation);

        float scaleX = scaleMagnitudeX;
        float scaleY = scaleMagnitudeY;
        if (_flipH) scaleX *= -1f;
        if (_flipV) scaleY *= -1f;
        _go.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        if (targetWidth > 0)
            Width = targetWidth;
        if (targetHeight > 0)
            Height = targetHeight;
    }

    private void UpdateMember()
    {
        IsDirtyMember = false;
        switch (_blingoSprite.Member)
        {
            case BlingoMemberBitmap bmp:
                var unityBmp = bmp.Framework<UnityMemberBitmap>();
                unityBmp.Preload();
                if (unityBmp.TextureBlingo is UnityTexture2D texBmp)
                    SetTexture(texBmp);
                break;
            case BlingoFilmLoopMember flm:
                var unityFl = flm.Framework<UnityFilmLoopMember>();
                unityFl.Preload();
                _filmLoopPlayer = _blingoSprite.GetFilmLoopPlayer();
                var size = _filmLoopPlayer?.GetBoundingBox() ?? flm.GetBoundingBox();
                _desiredWidth = size.Width;
                _desiredHeight = size.Height;
                Width = size.Width;
                Height = size.Height;
                if (_filmLoopPlayer?.Texture is UnityTexture2D texFl)
                    SetTexture(texFl);
                else if (unityFl.TextureBlingo is UnityTexture2D texFl2)
                    SetTexture(texFl2);
                break;
            case BlingoMemberText text:
                var unityText = text.Framework<UnityMemberText>();
                unityText.Preload();
                if (unityText.RenderToTexture(_blingoSprite.InkType, _blingoSprite.BackColor) is UnityTexture2D texText)
                    SetTexture(texText);
                break;
            case BlingoMemberField field:
                var unityField = field.Framework<UnityMemberField>();
                unityField.Preload();
                if (unityField.RenderToTexture(_blingoSprite.InkType, _blingoSprite.BackColor) is UnityTexture2D texField)
                    SetTexture(texField);
                break;
            case BlingoMemberShape shape:
                var unityShape = shape.Framework<UnityMemberShape>();
                unityShape.Preload();
                if (unityShape.TextureBlingo is UnityTexture2D texShape)
                    SetTexture(texShape);
                break;
            case BlingoMemberMedia media:
                var unityMedia = media.Framework<BlingoUnityMemberMedia>();
                unityMedia.Preload();
                UpdateMemberVideo(unityMedia);
                break;
        }
        if (_blingoSprite.Member is { } member)
        {
            if (Width == 0 || Height == 0)
            {
                Width = member.Width;
                Height = member.Height;
            }
        }
        Name = _blingoSprite.GetFullName();
    }

    private void UpdateMemberVideo(BlingoUnityMemberMedia media)
    {
        if (_videoPlayer != null)
        {
            UnityEngine.Object.Destroy(_videoPlayer.gameObject);
            _videoPlayer = null;
        }

        var go = new GameObject("VideoPlayer");
        go.transform.parent = _go.transform;
        var player = go.AddComponent<VideoPlayer>();
        if (media.Url != null)
            player.url = media.Url;
        player.playOnAwake = false;
        _videoPlayer = player;
    }

    /// <inheritdoc/>
    public void Play() => _videoPlayer?.Play();

    /// <inheritdoc/>
    public void Pause() => _videoPlayer?.Pause();

    /// <inheritdoc/>
    public void Stop()
    {
        if (_videoPlayer == null) return;
        _videoPlayer.Stop();
        _videoPlayer.time = 0;
    }

    /// <inheritdoc/>
    public void Seek(int milliseconds)
    {
        if (_videoPlayer == null) return;
        _videoPlayer.time = milliseconds / 1000.0;
    }

    /// <inheritdoc/>
    public int Duration => _videoPlayer?.clip != null ? (int)(_videoPlayer.clip.length * 1000) : 0;

    /// <inheritdoc/>
    public int CurrentTime
    {
        get => (int)(_videoPlayer?.time * 1000 ?? 0);
        set
        {
            if (_videoPlayer != null)
                _videoPlayer.time = value / 1000.0;
        }
    }

    /// <inheritdoc/>
    public BlingoMediaStatus MediaStatus => _videoPlayer switch
    {
        null => BlingoMediaStatus.Closed,
        _ when _videoPlayer.isPlaying => BlingoMediaStatus.Playing,
        _ => BlingoMediaStatus.Paused
    };

    public void RemoveMe()
    {
        _remove(this);
        Dispose();
    }

    public void Show() => Visibility = true;
    public void Hide() => Visibility = false;

    public void SetPosition(APoint point)
    {
        X = point.X;
        Y = point.Y;
    }

    public void ApplyMemberChangesOnStepFrame() { }

    public void SetTexture(IAbstTexture2D texture)
    {
        if (texture is UnityTexture2D ut)
        {
            var tex = ut.Texture;
            if (tex == null)
                return;
            Rect rect;
            if (_blingoSprite.Member is BlingoMemberBitmap && _blingoSprite.MemberSourceRect is { } srcRect)
            {
                float bottom = srcRect.Bottom;
                rect = new Rect(srcRect.Left, tex.height - bottom, srcRect.Width, srcRect.Height);
            }
            else
            {
                rect = new Rect(0, 0, tex.width, tex.height);
            }

            _renderer.sprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f));
            _texture = ut;
            _blingoSprite.FWTextureHasChanged(ut);

            if (_blingoSprite.MemberSourceRect is { } rectSource)
            {
                if (Width == 0)
                    Width = rectSource.Width;
                if (Height == 0)
                    Height = rectSource.Height;
            }
            else
            {
                if (Width == 0)
                    Width = rect.width;
                if (Height == 0)
                    Height = rect.height;
            }

            IsDirtyMember = true;
        }
    }

    public void Dispose()
    {
        if (_videoPlayer != null)
            UnityEngine.Object.Destroy(_videoPlayer.gameObject);
        GameObject.Destroy(_go);
    }

    object IAbstFrameworkNode.FrameworkNode => _go;

    float IAbstFrameworkNode.Width
    {
        get => Width;
        set
        {
            Width = value;
            DesiredWidth = value;
            IsDirty = true;
        }
    }

    float IAbstFrameworkNode.Height
    {
        get => Height;
        set
        {
            Height = value;
            DesiredHeight = value;
            IsDirty = true;
        }
    }

    AMargin IAbstFrameworkNode.Margin
    {
        get => Margin;
        set => Margin = value;
    }

    int IAbstFrameworkNode.ZIndex
    {
        get => ZIndex;
        set => ZIndex = value;
    }
}

