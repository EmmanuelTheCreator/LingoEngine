using System;
using System.Collections.Generic;
using AbstUI.Primitives;
using FluentAssertions;
using Xunit;
using AbstUI.Tests.Fakes;

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

        var expected = grid.CombineTiles();

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

}
