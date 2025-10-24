using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Text;

using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Tests.Helpers;

using FluentAssertions;

using Xunit;
using BlingoEngine.IO.Legacy.Texts.Data.Txc;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

public sealed class BlLegacyTxcReaderTests
{
    [Theory]
    [InlineData("Text_PreRender_CopyInk_SaveBitmap.cst", 'p', 1390, 65)]
    [InlineData("Text_PreRender_OtherInk_SaveBitmap.cst", 'l', 2010, 1093)]
    public void Read_PreRenderedTextBitmap_ReturnsDecodedImage(
        string assetName,
        char variant,
        int encodedLength,
        int remainderLength)
    {
        var path = TestContextHarness.GetAssetPath($"Texts_Fields/MemberTests/{assetName}");
        var bytes = File.ReadAllBytes(path);

        var image = BlLegacyTxcReader.Read(bytes);

        image.Should().NotBeNull();
        image.Variant.Should().Be(variant);
        image.Width.Should().Be((ushort)500);
        image.Height.Should().Be((ushort)154);
        image.BitsPerPixel.Should().Be((byte)8);
        image.Compression.Should().Be(BlLegacyTxcCompressionKind.RlePairs);

        image.Palette.Should().HaveCount(73);
        image.EncodedPixels.Length.Should().Be(encodedLength);
        image.Pixels.Length.Should().Be(image.Width * image.Height);
        image.RemainingData.Length.Should().Be(remainderLength);

        var signatureIndex = bytes.AsSpan().IndexOf("TXc"u8);
        signatureIndex.Should().BeGreaterThan(-1);
        image.PaletteOffset.Should().Be(signatureIndex + 4 + 0x4C);
        var expectedPixelOffset = image.PaletteOffset + 8 + image.Palette.Count * 8;
        image.PixelDataOffset.Should().Be(expectedPixelOffset);
    }

    [Theory]
    [InlineData("Text_PreRender_CopyInk_SaveBitmap.cst")]
    [InlineData("Text_PreRender_OtherInk_SaveBitmap.cst")]
    public void ToPngBytes_EmitsIndexedPngWithMatchingPixels(string assetName)
    {
        var path = TestContextHarness.GetAssetPath($"Texts_Fields/MemberTests/{assetName}");
        var bytes = File.ReadAllBytes(path);

        var image = BlLegacyTxcReader.Read(bytes);
        var png = image.ToPngBytes();

        png.Should().NotBeNull();
        png.Length.Should().BeGreaterThan(100);

        var signature = new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A };
        png.AsSpan(0, signature.Length).SequenceEqual(signature).Should().BeTrue();

        var offset = signature.Length;
        var idatBuffer = new MemoryStream();
        var paletteLength = 0;
        var width = image.Width;
        var height = image.Height;

        while (offset < png.Length)
        {
            var length = BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(offset, 4));
            offset += 4;
            var typeBytes = png.AsSpan(offset, 4);
            offset += 4;
            var chunkType = Encoding.ASCII.GetString(typeBytes);
            var data = png.AsSpan(offset, (int)length);
            offset += (int)length;
            offset += 4; // Skip CRC

            if (chunkType == "IHDR")
            {
                BinaryPrimitives.ReadUInt32BigEndian(data.Slice(0, 4)).Should().Be(width);
                BinaryPrimitives.ReadUInt32BigEndian(data.Slice(4, 4)).Should().Be(height);
                data[8].Should().Be(8);
                data[9].Should().Be(3);
            }
            else if (chunkType == "PLTE")
            {
                paletteLength = data.Length;
            }
            else if (chunkType == "IDAT")
            {
                idatBuffer.Write(data);
            }
            else if (chunkType == "IEND")
            {
                break;
            }
        }

        paletteLength.Should().BeGreaterThan(0);
        var maxPixel = 0;
        foreach (var value in image.Pixels)
            if (value > maxPixel)
                maxPixel = value;

        var expectedPaletteEntries = maxPixel + 1;
        paletteLength.Should().Be(expectedPaletteEntries * 3);

        using var compressedStream = new MemoryStream(idatBuffer.ToArray());
        using var zlib = new ZLibStream(compressedStream, CompressionMode.Decompress);
        using var raw = new MemoryStream();
        zlib.CopyTo(raw);

        var scanlines = raw.ToArray();
        scanlines.Length.Should().Be(height * (width + 1));

        var pixelIndex = 0;
        for (var row = 0; row < height; row++)
        {
            var rowOffset = row * (width + 1);
            scanlines[rowOffset].Should().Be(0);

            for (var column = 0; column < width; column++)
            {
                scanlines[rowOffset + 1 + column].Should().Be(image.Pixels[pixelIndex]);
                pixelIndex++;
            }
        }
    }
}
