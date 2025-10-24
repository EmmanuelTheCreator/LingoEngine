using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using BlingoEngine.IO.Legacy.Core;

namespace BlingoEngine.IO.Legacy.Texts.Data.Txc;

public static class BlLegacyTxcImageExtensions
{
    private static readonly byte[] PngSignature =
    {
        0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A
    };

    private static readonly uint[] CrcTable = CreateCrcTable();

    public static byte[] ToPngBytes(this BlLegacyTxcImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (image.Width <= 0 || image.Height <= 0)
            throw new InvalidOperationException("Image dimensions must be positive.");

        if (image.BitsPerPixel != 8)
            throw new NotSupportedException("Only 8-bit indexed TXc images can be exported to PNG.");

        if (image.Pixels.Length < image.Width * image.Height)
            throw new InvalidOperationException("Decoded pixel payload does not match the declared dimensions.");

        if (image.Palette.Count == 0)
            throw new InvalidOperationException("TXc image does not expose a color palette.");

        var (palette, paletteEntryCount) = BuildPalette(image);
        var scanlines = BuildScanlines(image.Pixels, image.Width, image.Height);
        var compressed = Compress(scanlines);

        using var stream = new MemoryStream();
        stream.Write(PngSignature);

        var ihdr = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(0, 4), image.Width);
        BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(4, 4), image.Height);
        ihdr[8] = 8;  // bit depth
        ihdr[9] = 3;  // indexed-color
        ihdr[10] = 0; // compression method
        ihdr[11] = 0; // filter method
        ihdr[12] = 0; // interlace method
        WriteChunk(stream, "IHDR"u8, ihdr);

        WriteChunk(stream, "PLTE"u8, palette.AsSpan(0, paletteEntryCount * 3));
        WriteChunk(stream, "IDAT"u8, compressed);
        WriteChunk(stream, "IEND"u8, ReadOnlySpan<byte>.Empty);

        return stream.ToArray();
    }

    private static (byte[] Palette, int Count) BuildPalette(BlLegacyTxcImage image)
    {
        var maxPixel = 0;
        foreach (var value in image.Pixels)
            if (value > maxPixel)
                maxPixel = value;

        var paletteCount = image.Palette.Count;
        if (paletteCount == 0)
            throw new InvalidOperationException("TXc image does not expose a color palette.");

        var sorted = new BlLegacyTxcPaletteEntry[paletteCount];
        for (var i = 0; i < paletteCount; i++)
            sorted[i] = image.Palette[i];

        Array.Sort(sorted, static (left, right) => left.Value.CompareTo(right.Value));

        var size = maxPixel + 1;
        var palette = new byte[size * 3];
        var entryIndex = 0;

        for (var i = 0; i <= maxPixel; i++)
        {
            var threshold = i * 257;
            while (entryIndex + 1 < sorted.Length && sorted[entryIndex + 1].Value <= threshold)
                entryIndex++;

            var color = sorted[entryIndex].Color;
            var offset = i * 3;
            palette[offset] = color.R;
            palette[offset + 1] = color.G;
            palette[offset + 2] = color.B;
        }

        return (palette, size);
    }

    private static byte[] BuildScanlines(byte[] pixels, int width, int height)
    {
        var scanlineWidth = width + 1;
        var output = new byte[height * scanlineWidth];
        var sourceIndex = 0;

        for (var row = 0; row < height; row++)
        {
            var destinationIndex = row * scanlineWidth;
            output[destinationIndex] = 0; // No filter
            Buffer.BlockCopy(pixels, sourceIndex, output, destinationIndex + 1, width);
            sourceIndex += width;
        }

        return output;
    }

    private static byte[] Compress(byte[] data)
    {
        using var output = new MemoryStream();
        using (var zlib = new ZLibStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            zlib.Write(data);
        }

        return output.ToArray();
    }

    private static void WriteChunk(Stream stream, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        if (type.Length != 4)
            throw new ArgumentException("PNG chunk types must contain exactly four bytes.", nameof(type));

        Span<byte> lengthBuffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(lengthBuffer, (uint)data.Length);
        stream.Write(lengthBuffer);
        stream.Write(type);
        if (!data.IsEmpty)
            stream.Write(data);

        Span<byte> crcBuffer = stackalloc byte[4];
        var crc = ComputeCrc(type, data);
        BinaryPrimitives.WriteUInt32BigEndian(crcBuffer, crc);
        stream.Write(crcBuffer);
    }

    private static uint ComputeCrc(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        var crc = 0xFFFF_FFFFu;
        for (var i = 0; i < type.Length; i++)
            crc = CrcTable[(crc ^ type[i]) & 0xFF] ^ crc >> 8;

        for (var i = 0; i < data.Length; i++)
            crc = CrcTable[(crc ^ data[i]) & 0xFF] ^ crc >> 8;

        return crc ^ 0xFFFF_FFFFu;
    }

    private static uint[] CreateCrcTable()
    {
        var table = new uint[256];

        for (uint n = 0; n < table.Length; n++)
        {
            var c = n;
            for (var k = 0; k < 8; k++)
            {
                if ((c & 1) != 0)
                    c = 0xEDB8_8320u ^ c >> 1;
                else
                    c >>= 1;
            }

            table[n] = c;
        }

        return table;
    }
}
