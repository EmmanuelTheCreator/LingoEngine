using System;
using System.Collections.Generic;
using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;

namespace AbstUI.Tests.Fakes;

internal sealed class TestGridPainter : AbstImagePainter<TestTexture>
{
    private readonly List<RenderPass> _renderPasses = new();
    private readonly TestTexture _target;
    private TestTexture? _currentTexture;
    private int _tileCounter;

    public TestGridPainter(int width, int height, int tileSize)
        : base(width, height, width, height)
    {
        Name = "TestPainter";
        TileSize = tileSize;
        UseTextureGrid = true;
        _target = new TestTexture("Target", width, height, isTile: false);
    }

    public IReadOnlyList<RenderPass> RenderPasses => _renderPasses;

    protected override void BeginRender(AColor clearColor)
    {
        var texture = _currentTexture ?? _target;
        _renderPasses.Add(new RenderPass(texture, OffsetX, OffsetY, clearColor));
    }

    protected override void EndRender()
    {
        _currentTexture = null;
    }

    protected override void ResizeTexture(int width, int height)
    {
        _target.Width = width;
        _target.Height = height;
    }

    protected override TestTexture CreateTileTexture(int width, int height)
        => new($"Tile_{_tileCounter++}", width, height, isTile: true);

    protected override void DestroyTileTexture(TestTexture texture)
    {
        texture.MarkDestroyed();
    }

    protected override void UseTexture(TestTexture texture)
    {
        _currentTexture = texture;
    }

    protected override byte[] GetTexturePixels(TestTexture texture, int width, int height)
        => throw new NotSupportedException();

    protected override void SetTextureARGBPixels(TestTexture texture, int width, int height, byte[] argbPixels)
        => throw new NotSupportedException();

    protected override void SetTextureRGBAPixels(TestTexture texture, int width, int height, byte[] rgbaPixels)
        => throw new NotSupportedException();

    protected override TestTexture Target => _target;

    public override void SetPixel(APoint point, AColor color)
        => throw new NotSupportedException();

    public override void DrawLine(APoint start, APoint end, AColor color, float width = 1)
    {
        var s = start;
        var e = end;
        int maxX = (int)MathF.Ceiling(MathF.Max(s.X, e.X)) + 1;
        int maxY = (int)MathF.Ceiling(MathF.Max(s.Y, e.Y)) + 1;
        var pos = new APoint(0, 0);
        var size = new APoint(maxX, maxY);
        var action = new DrawAction(false, pos, size, null, null, (texture, _, _) =>
        {
            var localStart = new APoint(s.X - OffsetX, s.Y - OffsetY);
            var localEnd = new APoint(e.X - OffsetX, e.Y - OffsetY);
            texture.RecordLine(localStart, localEnd, OffsetX, OffsetY);
        });
        AddDrawAction(action);
    }

    public override void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
        => throw new NotSupportedException();

    public override void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
        => throw new NotSupportedException();

    public override void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
        => throw new NotSupportedException();

    public override void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
        => throw new NotSupportedException();

    public override void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
        => throw new NotSupportedException();

    public override void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
        => throw new NotSupportedException();

    public override void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular, int letterSpacing = 0)
        => throw new NotSupportedException();

    public override void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular, int letterSpacing = 0)
        => throw new NotSupportedException();

    public override IAbstTexture2D GetTexture(string? name = null)
        => throw new NotSupportedException();

    public override void Dispose()
    {
        DisposeTiles();
    }
}

internal sealed class TestTexture
{
    private readonly List<LineRecord> _lines = new();

    public TestTexture(string name, int width, int height, bool isTile)
    {
        Name = name;
        Width = width;
        Height = height;
        IsTile = isTile;
    }

    public string Name { get; }

    public int Width { get; set; }

    public int Height { get; set; }

    public bool IsTile { get; }

    public bool IsDestroyed { get; private set; }

    public IReadOnlyList<LineRecord> Lines => _lines;

    public void RecordLine(APoint localStart, APoint localEnd, int offsetX, int offsetY)
    {
        _lines.Add(new LineRecord(localStart, localEnd, offsetX, offsetY));
    }

    public void MarkDestroyed()
    {
        IsDestroyed = true;
    }

    public readonly record struct LineRecord(APoint LocalStart, APoint LocalEnd, int OffsetX, int OffsetY)
    {
        public APoint GlobalStart => new(LocalStart.X + OffsetX, LocalStart.Y + OffsetY);

        public APoint GlobalEnd => new(LocalEnd.X + OffsetX, LocalEnd.Y + OffsetY);
    }
}

internal readonly record struct RenderPass(TestTexture Texture, int OffsetX, int OffsetY, AColor ClearColor);
