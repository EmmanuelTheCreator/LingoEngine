using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Bitmaps;
using AbstUI.Primitives;

namespace AbstUI.Tests.Fakes;

internal sealed class TestGridTexture : AbstBaseGridTexture<int>
{
    private readonly Dictionary<(int X, int Y), TestTileTexture> _tiles = new();
    private readonly int _tileSize;
    private int _width;
    private int _height;

    public override int Width => _width;
    public override int Height => _height;
    public int TileLength => _tileSize;
    public IReadOnlyDictionary<(int X, int Y), TestTileTexture> Tiles => _tiles;

    public TestGridTexture(int width, int height, int tileSize)
        : base("TestGrid")
    {
        _width = width;
        _height = height;
        _tileSize = tileSize;
    }

    public TestTileTexture EnsureTile((int X, int Y) coordinates)
    {
        if (!TryComputeTileBounds(coordinates, TileLength, Width, Height, out _, out _, out var tileWidth, out var tileHeight))
            throw new ArgumentOutOfRangeException(nameof(coordinates));

        return (TestTileTexture)GetOrCreateTile(coordinates, tileWidth, tileHeight);
    }

    protected override int TileSize => _tileSize;

    protected override AbstBaseTexture2D<int> CreateTile((int X, int Y) coordinates, int width, int height)
    {
        var tile = new TestTileTexture(coordinates, width, height);
        _tiles[coordinates] = tile;
        return tile;
    }

    protected override void ResizeTile((int X, int Y) coordinates, AbstBaseTexture2D<int> tile, int width, int height)
    {
        ((TestTileTexture)tile).Resize(width, height);
    }

    protected override void OnTileRemoved((int X, int Y) coordinates, AbstBaseTexture2D<int> tile)
    {
        base.OnTileRemoved(coordinates, tile);
        _tiles.Remove(coordinates);
    }

    protected override void DisposeTexture()
    {
        DisposeRegisteredTiles();
        _tiles.Clear();
    }

    public override IAbstTexture2D Clone()
        => throw new NotSupportedException("Test grid texture does not support cloning.");
}

internal sealed class TestTileTexture : AbstBaseTexture2D<int>
{
    private byte[] _pixels;
    private int _width;
    private int _height;

    public override int Width => _width;
    public override int Height => _height;
    public ReadOnlyMemory<byte> PixelData => _pixels;

    public TestTileTexture((int X, int Y) coordinates, int width, int height)
        : base($"Tile_{coordinates.X}_{coordinates.Y}")
    {
        _width = width;
        _height = height;
        _pixels = new byte[checked(width * height * 4)];
    }

    public void SetPixels(byte[] pixels)
    {
        if (pixels.Length != Width * Height * 4)
            throw new ArgumentException("Tile pixel data length does not match tile dimensions.", nameof(pixels));

        _pixels = pixels.ToArray();
    }

    public void Resize(int width, int height)
    {
        _width = width;
        _height = height;
        _pixels = new byte[checked(width * height * 4)];
    }

    protected override void DisposeTexture()
    {
        _pixels = Array.Empty<byte>();
    }

    public override byte[] GetPixels()
        => _pixels.ToArray();

    public override void SetARGBPixels(byte[] argbPixels)
    {
        if (argbPixels.Length != Width * Height * 4)
            throw new ArgumentException("ARGB buffer length does not match tile dimensions.", nameof(argbPixels));

        _pixels = argbPixels.ToArray();
    }

    public override void SetRGBAPixels(byte[] rgbaPixels)
    {
        if (rgbaPixels.Length != Width * Height * 4)
            throw new ArgumentException("RGBA buffer length does not match tile dimensions.", nameof(rgbaPixels));

        _pixels = rgbaPixels.ToArray();
    }

    public override IAbstTexture2D Clone()
        => throw new NotSupportedException("Test tile texture does not support cloning.");
}

internal static class TestGridTextureExtensions
{
    public static byte[] CombineTiles(this TestGridTexture grid)
    {
        int width = grid.Width;
        int height = grid.Height;
        int tileSize = grid.TileLength;
        var combined = new byte[checked(width * height * 4)];
        int destStride = width * 4;

        foreach (var (coordinates, tile) in grid.Tiles)
        {
            int offsetX = coordinates.X * tileSize;
            int offsetY = coordinates.Y * tileSize;
            int tileWidth = tile.Width;
            int tileHeight = tile.Height;
            int tileStride = tileWidth * 4;
            var tilePixels = tile.PixelData.Span;

            for (int row = 0; row < tileHeight; row++)
            {
                int destIndex = ((offsetY + row) * destStride) + (offsetX * 4);
                tilePixels.Slice(row * tileStride, tileStride).CopyTo(combined.AsSpan(destIndex, tileStride));
            }
        }

        return combined;
    }
}
