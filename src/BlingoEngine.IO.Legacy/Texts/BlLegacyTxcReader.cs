using System;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.IO;

using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Texts.Data;

namespace BlingoEngine.IO.Legacy.Texts;

/// <summary>
/// Reads legacy Director <c>TXc</c> resources that store pre-rendered text bitmaps.
/// The reader validates the QuickDraw pixmap header, extracts the embedded color table,
/// and decodes the first pixel plane using the simple run-length schemes that Director
/// employed (pair encoding and PackBits).
/// </summary>
public static class BlLegacyTxcReader
{
    private static readonly byte[] Signature = { (byte)'T', (byte)'X', (byte)'c' };

    /// <summary>Parses a <c>TXc</c> buffer and returns the decoded image metadata.</summary>
    public static BlLegacyTxcImage Read(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        return Read(buffer.AsSpan());
    }

    /// <summary>Parses a <c>TXc</c> buffer and returns the decoded image metadata.</summary>
    public static BlLegacyTxcImage Read(ReadOnlySpan<byte> buffer)
    {
        if (buffer.IsEmpty)
            throw new InvalidDataException("TXc payload is empty.");

        var signatureIndex = buffer.IndexOf(Signature);
        if (signatureIndex < 0 || signatureIndex + 4 > buffer.Length)
            throw new InvalidDataException("TXc signature not found.");

        var variant = (char)buffer[signatureIndex + 3];
        var payload = buffer[(signatureIndex + 4)..];
        if (payload.Length < 0x38)
            throw new InvalidDataException("TXc header truncated.");

        var width = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(0x34, 2));
        var height = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(0x36, 2));

        var paletteOffset = LocatePaletteOffset(payload, out var paletteLength, out var paletteEntries);
        if (paletteOffset < 0)
            throw new InvalidDataException("TXc color table not found.");

        var palette = Array.AsReadOnly(paletteEntries);

        var pixelOffset = paletteOffset + paletteLength;
        if (pixelOffset > payload.Length)
            throw new InvalidDataException("TXc pixel payload truncated.");

        var pixelSource = payload[pixelOffset..];
        var (decodedPixels, consumed, compression) = DecodePixelData(pixelSource, width, height);

        var encodedPixels = consumed > 0 ? pixelSource[..consumed].ToArray() : Array.Empty<byte>();
        var remainder = consumed >= pixelSource.Length
            ? Array.Empty<byte>()
            : pixelSource[consumed..].ToArray();

        var bitsPerPixel = decodedPixels.Length == width * height && width > 0 && height > 0
            ? (byte)8
            : (byte)0;

        return new BlLegacyTxcImage(
            variant,
            width,
            height,
            bitsPerPixel,
            compression,
            signatureIndex + 4 + paletteOffset,
            signatureIndex + 4 + pixelOffset,
            palette,
            encodedPixels,
            decodedPixels,
            remainder);
    }

    private static int LocatePaletteOffset(
        ReadOnlySpan<byte> payload,
        out int paletteLength,
        out BlLegacyTxcPaletteEntry[] entries)
    {
        paletteLength = 0;
        entries = Array.Empty<BlLegacyTxcPaletteEntry>();

        if (TryReadPalette(payload, 0x4C, out paletteLength, out entries))
            return 0x4C;

        for (var candidate = 0x20; candidate <= payload.Length - 8; candidate += 2)
        {
            if (TryReadPalette(payload, candidate, out paletteLength, out entries))
                return candidate;
        }

        paletteLength = 0;
        entries = Array.Empty<BlLegacyTxcPaletteEntry>();
        return -1;
    }

    private static bool TryReadPalette(
        ReadOnlySpan<byte> payload,
        int offset,
        out int paletteLength,
        out BlLegacyTxcPaletteEntry[] entries)
    {
        paletteLength = 0;
        entries = Array.Empty<BlLegacyTxcPaletteEntry>();

        if (offset < 0 || offset + 8 > payload.Length)
            return false;

        var ctSize = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(offset + 6, 2));
        var entryCount = ctSize + 1;
        if (entryCount <= 0 || entryCount > 1024)
            return false;

        var totalLength = 8L + entryCount * 8L;
        if (offset + totalLength > payload.Length)
            return false;

        paletteLength = (int)totalLength;
        entries = new BlLegacyTxcPaletteEntry[entryCount];

        var entryOffset = offset + 8;
        for (var i = 0; i < entryCount; i++)
        {
            var value = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(entryOffset, 2));
            var red16 = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(entryOffset + 2, 2));
            var green16 = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(entryOffset + 4, 2));
            var blue16 = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(entryOffset + 6, 2));

            var color = new BlLegacyColor(
                ConvertComponent(red16),
                ConvertComponent(green16),
                ConvertComponent(blue16));

            entries[i] = new BlLegacyTxcPaletteEntry(value, color);
            entryOffset += 8;
        }

        return true;
    }

    private static byte ConvertComponent(ushort component) => (byte)(component / 257);

    private static (byte[] Pixels, int Used, BlLegacyTxcCompressionKind Compression) DecodePixelData(
        ReadOnlySpan<byte> source,
        int width,
        int height)
    {
        if (width <= 0 || height <= 0 || source.IsEmpty)
            return (Array.Empty<byte>(), 0, BlLegacyTxcCompressionKind.Unknown);

        var destination = new byte[width * height];
        if (TryPairs(source, destination.AsSpan(), width, height, out var used))
            return (destination, used, BlLegacyTxcCompressionKind.RlePairs);

        destination = new byte[width * height];
        if (TryPackBits(source, destination.AsSpan(), width, height, out used))
            return (destination, used, BlLegacyTxcCompressionKind.PackBits);

        if (source.Length >= destination.Length)
        {
            var copy = source[..destination.Length].ToArray();
            return (copy, destination.Length, BlLegacyTxcCompressionKind.None);
        }

        return (Array.Empty<byte>(), 0, BlLegacyTxcCompressionKind.Unknown);
    }

    private static bool TryPairs(
        ReadOnlySpan<byte> source,
        Span<byte> destination,
        int width,
        int height,
        out int used)
    {
        used = 0;
        var destIndex = 0;

        for (var row = 0; row < height; row++)
        {
            var column = 0;
            while (column < width)
            {
                if (used + 2 > source.Length)
                    return false;

                var runLength = source[used++];
                var value = source[used++];

                for (var i = 0; i < runLength && column < width; i++)
                {
                    if (destIndex >= destination.Length)
                        return false;

                    destination[destIndex++] = value;
                    column++;
                }
            }

            if (column != width)
                return false;
        }

        return destIndex == destination.Length;
    }

    private static bool TryPackBits(
        ReadOnlySpan<byte> source,
        Span<byte> destination,
        int width,
        int height,
        out int used)
    {
        used = 0;
        var destIndex = 0;

        for (var row = 0; row < height; row++)
        {
            var column = 0;
            while (column < width && used < source.Length)
            {
                var n = (source[used++] + 256) % 256;

                if (n <= 127)
                {
                    var count = n + 1;
                    if (used + count > source.Length || destIndex + count > destination.Length)
                        return false;

                    source.Slice(used, count).CopyTo(destination.Slice(destIndex));
                    used += count;
                    destIndex += count;
                    column += count;
                }
                else if (n >= 129)
                {
                    var count = 257 - n;
                    if (used >= source.Length)
                        return false;

                    var value = source[used++];
                    for (var i = 0; i < count && column < width; i++)
                    {
                        if (destIndex >= destination.Length)
                            return false;

                        destination[destIndex++] = value;
                        column++;
                    }
                }
                // n == 128 is a NOP
            }

            if (column != width)
                return false;
        }

        return destIndex == destination.Length;
    }
}
