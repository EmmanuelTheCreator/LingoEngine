using System;
using System.Collections.Generic;
using AbstUI.Bitmaps;
using AbstUI.Primitives;
using FluentAssertions;
using Xunit;

namespace AbstUI.Tests;

public class AbstBaseGridTextureTests
{
    [Fact]
    public void GetPixels_WithFourTiles_ComposesCombinedBuffer()
    {
        var grid = new TestGridTexture(32, 32, tileSize: 16);

        grid.EnsureTile((0, 0)).SetPixels(CreateTileData(16, 16, startValue: 1));
        grid.EnsureTile((1, 0)).SetPixels(CreateTileData(16, 16, startValue: 2));
        grid.EnsureTile((0, 1)).SetPixels(CreateTileData(16, 16, startValue: 3));
        grid.EnsureTile((1, 1)).SetPixels(CreateTileData(16, 16, startValue: 4));

        var expected = CombineTiles(grid.Width, grid.Height, grid.TileLength, grid.Tiles);

        var pixels = grid.GetPixels();

        pixels.Should().Equal(expected);
    }

    [Fact]
    public void SetARGBPixels_WithFourTiles_DistributesBufferAcrossTiles()
    {
        var grid = new TestGridTexture(32, 32, tileSize: 16);
        var pixels = CreateImageBuffer(grid.Width, grid.Height, startValue: 42);

        grid.SetARGBPixels(pixels);

        grid.Tiles.Should().HaveCount(4);
        grid.Tiles.Keys.Should().BeEquivalentTo(new[] { (0, 0), (1, 0), (0, 1), (1, 1) });

        foreach (var (coordinates, tile) in grid.Tiles)
        {
            var expected = ExtractTileRegion(pixels, grid.Width, grid.TileLength, coordinates, tile.Width, tile.Height);
            tile.PixelData.ToArray().Should().Equal(expected);
        }

        grid.GetPixels().Should().Equal(pixels);
    }

    [Fact]
    public void SetRGBAPixels_WithFourTiles_DistributesBufferAcrossTiles()
    {
        var grid = new TestGridTexture(32, 32, tileSize: 16);
        var pixels = CreateImageBuffer(grid.Width, grid.Height, startValue: 200);

        grid.SetRGBAPixels(pixels);

        grid.Tiles.Should().HaveCount(4);
        grid.Tiles.Keys.Should().BeEquivalentTo(new[] { (0, 0), (1, 0), (0, 1), (1, 1) });

        foreach (var (coordinates, tile) in grid.Tiles)
        {
            var expected = ExtractTileRegion(pixels, grid.Width, grid.TileLength, coordinates, tile.Width, tile.Height);
            tile.PixelData.ToArray().Should().Equal(expected);
        }

        grid.GetPixels().Should().Equal(pixels);
    }

    private static byte[] CreateImageBuffer(int width, int height, byte startValue)
    {
        var buffer = new byte[checked(width * height * 4)];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(startValue + i);
        }

        return buffer;
    }

    private static byte[] CreateTileData(int width, int height, byte startValue)
    {
        var buffer = new byte[checked(width * height * 4)];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(startValue + i);
        }

        return buffer;
    }

    private static byte[] CombineTiles(int width, int height, int tileSize, IReadOnlyDictionary<(int X, int Y), TestTileTexture> tiles)
    {
        var combined = new byte[checked(width * height * 4)];
        int destStride = width * 4;

        foreach (var (coordinates, tile) in tiles)
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

    private static byte[] ExtractTileRegion(byte[] source, int imageWidth, int tileSize, (int X, int Y) coordinates, int tileWidth, int tileHeight)
    {
        var region = new byte[checked(tileWidth * tileHeight * 4)];
        int srcStride = imageWidth * 4;
        int destStride = tileWidth * 4;
        int offsetX = coordinates.X * tileSize;
        int offsetY = coordinates.Y * tileSize;

        for (int row = 0; row < tileHeight; row++)
        {
            int srcIndex = ((offsetY + row) * srcStride) + (offsetX * 4);
            Array.Copy(source, srcIndex, region, row * destStride, destStride);
        }

        return region;
    }

    private sealed class TestGridTexture : AbstBaseGridTexture<int>
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

    private sealed class TestTileTexture : AbstBaseTexture2D<int>
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
}
