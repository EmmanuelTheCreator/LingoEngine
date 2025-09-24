using System;
using System.Collections.Generic;
using AbstUI.Bitmaps;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;

namespace AbstUI.Components.Graphics;

public abstract class AbstImagePainter<TTexture> : IAbstImagePainter
{
    public readonly record struct DrawAction(bool NeedResize, APoint Position, APoint Size, ARect? SrcRect, ARect? DestRect, Action<TTexture, ARect?, ARect?> Execute);

    protected readonly List<DrawAction> _drawActions = new();
    private AColor? _clearColor;
    protected bool _dirty;
    protected readonly int _maxWidth;
    protected readonly int _maxHeight;
    private int _width;
    private int _height;

    // Grid rendering support is managed by PainterGridTexture when enabled.
    private PainterGridTexture? _gridTexture;

    protected AbstImagePainter(int width, int height, int maxWidth, int maxHeight)
    {
        _maxWidth = maxWidth == 0 ? 2048 : maxWidth;
        _maxHeight = maxHeight == 0 ? 2048 : maxHeight;
        _width = width > 0 ? Math.Min(width, _maxWidth) : 10;
        _height = height > 0 ? Math.Min(height, _maxHeight) : 10;
        _dirty = true;
    }

    public int Width
    {
        get => _width;
        set
        {
            if (_width == value) return;
            _width = value;
            MarkDirty();
        }
    }

    public int Height
    {
        get => _height;
        set
        {
            if (_height == value) return;
            _height = value;
            MarkDirty();
        }
    }

    public bool Pixilated { get; set; }
    public bool AutoResizeWidth { get; set; } = false;
    public bool AutoResizeHeight { get; set; } = true;
    public string Name { get; set; } = string.Empty;

    public bool UseTextureGrid { get; set; }
    public int TileSize { get; set; } = 128;

    protected int OffsetX => _gridTexture?.OffsetX ?? 0;
    protected int OffsetY => _gridTexture?.OffsetY ?? 0;

    protected void AddDrawAction(DrawAction action)
    {
        _drawActions.Add(action);
        _dirty = true;
        if (!UseTextureGrid) return;
        EnsureGrid().TrackDrawAction(action);
    }

    protected void AddTileAction((int X, int Y) tile, DrawAction action)
    {
        if (!UseTextureGrid) return;
        EnsureGrid().AddTileAction(tile, action);
        _dirty = true;
    }

    protected void MarkDirty()
    {
        _dirty = true;
        if (UseTextureGrid)
            EnsureGrid().MarkAllDirty(Width, Height, TileSize);
    }

    protected void MarkDirty(APoint position, APoint size)
    {
        _dirty = true;
        if (!UseTextureGrid) return;
        EnsureGrid().MarkDirty(position, size, TileSize);
    }

    public void Clear(AColor color)
    {
        _drawActions.Clear();
        if (UseTextureGrid)
            EnsureGrid().ClearActions();
        _clearColor = color;
        MarkDirty();
    }

    public void Resize(int width, int height)
    {
        width = Math.Min(width, _maxWidth);
        height = Math.Min(height, _maxHeight);
        if (_width == width && _height == height) return;
        _width = width;
        _height = height;
        MarkDirty();
    }

    public void Render()
    {
        var newWidth = Width;
        var newHeight = Height;
        if (AutoResizeWidth || AutoResizeHeight)
        {
            foreach (var action in _drawActions)
            {
                if (!action.NeedResize) continue;
                var candidateW = (int)MathF.Ceiling(action.Position.X + action.Size.X);
                var candidateH = (int)MathF.Ceiling(action.Position.Y + action.Size.Y);
                if (AutoResizeWidth && candidateW > newWidth)
                    newWidth = candidateW;
                if (AutoResizeHeight && candidateH > newHeight)
                    newHeight = candidateH;
            }
        }

        var targetWidth = AutoResizeWidth ? Math.Max(Width, newWidth) : Width;
        var targetHeight = AutoResizeHeight ? Math.Max(Height, newHeight) : Height;
        targetWidth = Math.Min(targetWidth, _maxWidth);
        targetHeight = Math.Min(targetHeight, _maxHeight);

        if (!UseTextureGrid)
        {
            if (!_dirty && targetWidth == _width && targetHeight == _height)
                return;

            if (targetWidth != Width || targetHeight != Height)
            {
                ResizeTexture(targetWidth, targetHeight);
                _width = targetWidth;
                _height = targetHeight;
            }

            BeginRender(_clearColor ?? AColor.FromRGBA(0, 0, 0, 0));
            foreach (var action in _drawActions)
                action.Execute(Target, action.SrcRect, action.DestRect);
            EndRender();
            _dirty = false;
            return;
        }

        var grid = EnsureGrid();
        if (!_dirty && !grid.HasDirtyTiles && targetWidth == _width && targetHeight == _height)
            return;

        _width = targetWidth;
        _height = targetHeight;
        var clearColor = _clearColor ?? AColor.FromRGBA(0, 0, 0, 0);
        grid.Render(targetWidth, targetHeight, TileSize, clearColor);
        _dirty = false;
    }

    protected abstract void BeginRender(AColor clearColor);
    protected abstract void EndRender();
    protected abstract void ResizeTexture(int width, int height);
    protected virtual TTexture CreateTileTexture(int width, int height) => default!;
    protected virtual void DestroyTileTexture(TTexture texture) { }
    protected virtual AbstBaseTexture2D<TTexture>? CreateTilePixelsHost((int X, int Y) coordinates, TTexture texture, int width, int height)
        => null;
    protected virtual void UseTexture(TTexture texture) { }
    protected virtual byte[] GetTexturePixels(TTexture texture, int width, int height)
        => throw new NotSupportedException($"{GetType().Name} does not support pixel readback for grid textures.");

    protected virtual void SetTextureARGBPixels(TTexture texture, int width, int height, byte[] argbPixels)
        => throw new NotSupportedException($"{GetType().Name} does not support ARGB uploads for grid textures.");

    protected virtual void SetTextureRGBAPixels(TTexture texture, int width, int height, byte[] rgbaPixels)
        => throw new NotSupportedException($"{GetType().Name} does not support RGBA uploads for grid textures.");
    protected abstract TTexture Target { get; }

    protected void DisposeTiles()
    {
        _gridTexture?.DisposeTiles();
        _gridTexture = null;
    }

    private PainterGridTexture EnsureGrid()
    {
        return _gridTexture ??= new PainterGridTexture(this);
    }

    private sealed class PainterGridTexture : AbstBaseGridTexture<TTexture>, ITextureGridOwner<TTexture>
    {
        private readonly AbstImagePainter<TTexture> _owner;
        private readonly Dictionary<(int X, int Y), List<DrawAction>> _tileActions = new();

        public PainterGridTexture(AbstImagePainter<TTexture> owner)
            : base(owner.Name)
        {
            _owner = owner;
        }

        public override int Width => _owner.Width;
        public override int Height => _owner.Height;
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }

        protected override int TileSize => _owner.TileSize;

        public void TrackDrawAction(DrawAction action)
        {
            if (_owner.TileSize <= 0)
                return;

            var range = CalculateTileRange(action.Position, action.Size, _owner.TileSize);
            if (!range.HasTiles)
                return;

            for (int x = range.StartX; x <= range.EndX; x++)
            {
                for (int y = range.StartY; y <= range.EndY; y++)
                    AddTileAction((x, y), action);
            }
        }

        public void AddTileAction((int X, int Y) tile, DrawAction action)
        {
            if (!_tileActions.TryGetValue(tile, out var list))
            {
                list = new List<DrawAction>();
                _tileActions[tile] = list;
            }

            list.Add(action);
            MarkTileDirty(tile);
        }

        public void MarkAllDirty(int width, int height, int tileSize)
        {
            MarkAllTilesDirty(width, height, tileSize);
        }

        public void MarkDirty(APoint position, APoint size, int tileSize)
        {
            MarkRegionDirty(position, size, tileSize);
        }

        public void ClearActions()
        {
            _tileActions.Clear();
            ClearDirtyTiles();
        }

        public void Render(int targetWidth, int targetHeight, int tileSize, AColor clearColor)
        {
            if (tileSize <= 0 || !HasDirtyTiles)
                return;

            ForEachDirtyTile(targetWidth, targetHeight, tileSize, (coords, offsetX, offsetY, _, _, tileBase) =>
            {
                var tile = (TextureGridTile<TTexture>)tileBase;
                OffsetX = offsetX;
                OffsetY = offsetY;
                _owner.UseTexture(tile.Texture);
                _owner.BeginRender(clearColor);
                if (_tileActions.TryGetValue(coords, out var actions))
                {
                    foreach (var action in actions)
                        action.Execute(tile.Texture, action.SrcRect, action.DestRect);
                }
                _owner.EndRender();
            });

            OffsetX = 0;
            OffsetY = 0;
            ClearDirtyTiles();
        }

        public void DisposeTiles()
        {
            DisposeRegisteredTiles();
            _tileActions.Clear();
            OffsetX = 0;
            OffsetY = 0;
        }

        protected override AbstBaseTexture2D<TTexture> CreateTile((int X, int Y) coordinates, int width, int height)
        {
            var texture = _owner.CreateTileTexture(width, height);
            var pixelsHost = _owner.CreateTilePixelsHost(coordinates, texture, width, height);
            return new TextureGridTile<TTexture>(_owner.Name, this, coordinates, width, height, texture, pixelsHost);
        }

        protected override void ResizeTile((int X, int Y) coordinates, AbstBaseTexture2D<TTexture> tile, int width, int height)
        {
            var painterTile = (TextureGridTile<TTexture>)tile;
            var texture = _owner.CreateTileTexture(width, height);
            var pixelsHost = _owner.CreateTilePixelsHost(coordinates, texture, width, height);
            painterTile.UpdateTexture(texture, width, height, pixelsHost);
        }

        protected override void DisposeTexture()
        {
            DisposeTiles();
        }
        public override IAbstTexture2D Clone() => throw new NotSupportedException("Grid textures do not support cloning.");

        void ITextureGridOwner<TTexture>.DestroyTileTexture(TTexture texture)
            => _owner.DestroyTileTexture(texture);

        byte[] ITextureGridOwner<TTexture>.GetTexturePixels(TTexture texture, int width, int height)
            => _owner.GetTexturePixels(texture, width, height);

        void ITextureGridOwner<TTexture>.SetTextureARGBPixels(TTexture texture, int width, int height, byte[] argbPixels)
            => _owner.SetTextureARGBPixels(texture, width, height, argbPixels);

        void ITextureGridOwner<TTexture>.SetTextureRGBAPixels(TTexture texture, int width, int height, byte[] rgbaPixels)
            => _owner.SetTextureRGBAPixels(texture, width, height, rgbaPixels);
    }

    public abstract void SetPixel(APoint point, AColor color);
    public abstract void DrawLine(APoint start, APoint end, AColor color, float width = 1);
    public abstract void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1);
    public abstract void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1);
    public abstract void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1);
    public abstract void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1);
    public abstract void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format);
    public abstract void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position);
    public abstract void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular, int letterSpacing = 0);
    public abstract void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular, int letterSpacing = 0);
    public abstract IAbstTexture2D GetTexture(string? name = null);
    public abstract void Dispose();
}
