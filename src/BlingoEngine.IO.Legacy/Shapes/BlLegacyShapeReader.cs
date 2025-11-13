using System;
using System.Buffers.Binary;
using System.Collections.Generic;

using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Shapes;

/// <summary>
/// Reads shape cast-member records from the resource table. The reader resolves each <c>CASt</c>
/// entry, inflates compressed bodies when necessary, and slices out the 17-byte QuickDraw payload
/// described in src/BlingoEngine.IO.Legacy/docs/LegacyShapeRecords.md so higher layers can inspect the ink and colour
/// flags.
/// </summary>
internal sealed class BlLegacyShapeReader
{
    private const int ShapeRecordLength = 17;
    private const int ModernHeaderLength = 12;
    private const int TransitionalHeaderLength = 7;

    private static readonly BlTag CastTag = BlTag.Cast;
    private static readonly BlTag LegacyCastTag = BlTag.Get("CASt");

    private readonly ReaderContext _context;

    public BlLegacyShapeReader(ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <summary>
    /// Reads every shape record referenced by the resource tables, returning the raw bytes alongside
    /// a best-effort format classification that explains how to interpret the colour components.
    /// </summary>
    public IReadOnlyList<BlLegacyShape> Read()
    {
        var shapes = new List<BlLegacyShape>();
        if (_context.Resources.Entries.Count == 0)
        {
            return shapes;
        }

        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (_context.AfterburnerState is not null)
        {
            afterburnerLoader = new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);
        }

        var movieFormat = _context.DataBlock?.Format;
        bool isBigEndian = movieFormat?.IsBigEndian ?? true;

        foreach (var entry in _context.Resources.Entries)
        {
            if (entry.Tag != CastTag && entry.Tag != LegacyCastTag)
            {
                continue;
            }

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            if (payload.Length == 0)
            {
                continue;
            }

            if (!TryExtractShape(payload, isBigEndian, out var parsedFormat, out var bytes))
            {
                continue;
            }

            var format = BlLegacyShapeFormat.Resolve(movieFormat, parsedFormat);
            shapes.Add(new BlLegacyShape(entry.Id, format, bytes));
        }

        return shapes;
    }

    private static byte[] LoadPayload(BlLegacyResourceEntry entry, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        return entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
            ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
            : entry.ReadClassicPayload(classicLoader);
    }

    private static bool TryExtractShape(byte[] payload, bool isBigEndian, out BlLegacyShapeFormatKind format, out byte[] bytes)
    {
        var span = payload.AsSpan();

        int offset;
        int length;

        if (TryParseModern(span, isBigEndian: true, out offset, out length) &&
            TryExtractShapeRecord(span.Slice(offset, length), isBigEndian, out bytes))
        {
            format = BlLegacyShapeFormatKind.Director4To10UnsignedColors;
            return true;
        }

        if (TryParseModern(span, isBigEndian: false, out offset, out length) &&
            TryExtractShapeRecord(span.Slice(offset, length), isBigEndian, out bytes))
        {
            format = BlLegacyShapeFormatKind.Director4To10UnsignedColors;
            return true;
        }

        if (TryParseTransitional(span, isBigEndian: true, out offset, out length) &&
            TryExtractShapeRecord(span.Slice(offset, length), isBigEndian, out bytes))
        {
            format = BlLegacyShapeFormatKind.Director4To10UnsignedColors;
            return true;
        }

        if (TryParseTransitional(span, isBigEndian: false, out offset, out length) &&
            TryExtractShapeRecord(span.Slice(offset, length), isBigEndian, out bytes))
        {
            format = BlLegacyShapeFormatKind.Director4To10UnsignedColors;
            return true;
        }

        if (TryParseVintage(span, out offset, out length) &&
            TryExtractShapeRecord(span.Slice(offset, length), isBigEndian, out bytes))
        {
            format = BlLegacyShapeFormatKind.Director2To3SignedColors;
            return true;
        }

        if (TryExtractShapeRecord(span, isBigEndian, out bytes))
        {
            format = BlLegacyShapeFormatKind.Unknown;
            return true;
        }

        format = BlLegacyShapeFormatKind.Unknown;
        bytes = Array.Empty<byte>();
        return false;
    }

    private static bool TryParseModern(ReadOnlySpan<byte> payload, bool isBigEndian, out int offset, out int length)
    {
        offset = 0;
        length = 0;

        if (payload.Length < ModernHeaderLength)
        {
            return false;
        }

        uint castType = ReadUInt32(payload, isBigEndian);
        if (castType != 8)
        {
            return false;
        }

        uint infoSize = ReadUInt32(payload.Slice(4, 4), isBigEndian);
        uint dataSize = ReadUInt32(payload.Slice(8, 4), isBigEndian);
        if (dataSize == 0)
        {
            return false;
        }

        if (infoSize > (uint)(payload.Length - ModernHeaderLength))
        {
            return false;
        }

        int dataOffset = ModernHeaderLength + (int)infoSize;
        if (dataOffset < 0 || dataOffset > payload.Length)
        {
            return false;
        }

        int available = payload.Length - dataOffset;
        if (available <= 0)
        {
            return false;
        }

        if (dataSize > (uint)available)
        {
            dataSize = (uint)available;
        }

        if (dataSize < ShapeRecordLength)
        {
            return false;
        }

        offset = dataOffset;
        length = (int)dataSize;
        return true;
    }

    private static bool TryParseTransitional(ReadOnlySpan<byte> payload, bool isBigEndian, out int offset, out int length)
    {
        offset = 0;
        length = 0;

        if (payload.Length < TransitionalHeaderLength)
        {
            return false;
        }

        ushort castDataSize = ReadUInt16(payload, isBigEndian);
        uint infoSize = ReadUInt32(payload.Slice(2, 4), isBigEndian);
        byte castType = payload[6];
        if (castType != 8)
        {
            return false;
        }

        if (infoSize > (uint)(payload.Length - TransitionalHeaderLength))
        {
            return false;
        }

        int afterHeader = payload.Length - TransitionalHeaderLength;
        int castDataAreaLength = afterHeader - (int)infoSize;
        if (castDataAreaLength <= 0)
        {
            return false;
        }

        int declaredAreaLength = Math.Max(0, castDataSize - 1);
        int availableAreaLength = Math.Min(declaredAreaLength, castDataAreaLength);
        if (availableAreaLength < ShapeRecordLength)
        {
            return false;
        }

        int dataAreaOffset = TransitionalHeaderLength + (castDataAreaLength - availableAreaLength);
        if (dataAreaOffset < TransitionalHeaderLength)
        {
            return false;
        }

        int endOffset = dataAreaOffset + availableAreaLength;
        if (endOffset > payload.Length)
        {
            return false;
        }

        offset = dataAreaOffset;
        length = availableAreaLength;
        return true;
    }

    private static bool TryParseVintage(ReadOnlySpan<byte> payload, out int offset, out int length)
    {
        offset = 0;
        length = 0;

        if (payload.Length < 2)
        {
            return false;
        }

        byte entrySize = payload[0];
        if (entrySize < 2)
        {
            return false;
        }

        byte castType = payload[1];
        if (castType != 8)
        {
            return false;
        }

        int actualAfterType = Math.Max(0, payload.Length - 2);
        int declaredAreaLength = Math.Max(0, entrySize - 1);
        int dataAreaLength = Math.Min(declaredAreaLength, actualAfterType);
        if (dataAreaLength < ShapeRecordLength)
        {
            return false;
        }

        int dataAreaOffset = 2 + (actualAfterType - dataAreaLength);
        if (dataAreaOffset < 2 || dataAreaOffset + dataAreaLength > payload.Length)
        {
            return false;
        }

        offset = dataAreaOffset;
        length = dataAreaLength;
        return true;
    }

    private static bool TryExtractShapeRecord(ReadOnlySpan<byte> data, bool isBigEndian, out byte[] bytes)
    {
        if (data.Length < ShapeRecordLength)
        {
            bytes = Array.Empty<byte>();
            return false;
        }

        if (data.Length == ShapeRecordLength)
        {
            bytes = data.ToArray();
            return true;
        }

        int bestOffset = -1;
        int bestScore = int.MinValue;
        int searchLimit = data.Length - ShapeRecordLength;

        for (int offset = 0; offset <= searchLimit; offset++)
        {
            var window = data.Slice(offset, ShapeRecordLength);
            int score = ScoreWindow(window, isBigEndian);
            if (score > bestScore)
            {
                bestScore = score;
                bestOffset = offset;
            }
        }

        if (bestOffset >= 0 && bestScore >= 0)
        {
            bytes = data.Slice(bestOffset, ShapeRecordLength).ToArray();
            return true;
        }

        // Fallback: preserve the trailing bytes so callers can inspect the raw payload even if
        // heuristics could not locate a confident record boundary.
        bytes = data.Slice(data.Length - ShapeRecordLength, ShapeRecordLength).ToArray();
        return true;
    }

    private static int ScoreWindow(ReadOnlySpan<byte> window, bool isBigEndian)
    {
        if (window.Length != ShapeRecordLength)
        {
            return int.MinValue;
        }

        bool matchesNative = HasValidRect(window, isBigEndian);
        bool matchesOpposite = HasValidRect(window, !isBigEndian);

        if (!matchesNative && !matchesOpposite)
        {
            return -1;
        }

        int score = matchesNative ? 4 : 1;

        byte fill = window[14];
        if (fill <= 0x7F)
        {
            score++;
        }

        byte thickness = window[15];
        if (thickness == 0 || thickness <= 0x40)
        {
            score++;
        }

        byte direction = window[16];
        if (direction <= 0x0F)
        {
            score++;
        }

        return score;
    }

    private static bool HasValidRect(ReadOnlySpan<byte> window, bool isBigEndian)
    {
        if (window.Length < 10)
        {
            return false;
        }

        short top = ReadInt16(window.Slice(2, 2), isBigEndian);
        short left = ReadInt16(window.Slice(4, 2), isBigEndian);
        short bottom = ReadInt16(window.Slice(6, 2), isBigEndian);
        short right = ReadInt16(window.Slice(8, 2), isBigEndian);

        return bottom >= top && right >= left;
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> data, bool isBigEndian)
        => isBigEndian ? BinaryPrimitives.ReadUInt32BigEndian(data) : BinaryPrimitives.ReadUInt32LittleEndian(data);

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, bool isBigEndian)
        => isBigEndian ? BinaryPrimitives.ReadUInt16BigEndian(data) : BinaryPrimitives.ReadUInt16LittleEndian(data);

    private static short ReadInt16(ReadOnlySpan<byte> data, bool isBigEndian)
        => isBigEndian ? BinaryPrimitives.ReadInt16BigEndian(data) : BinaryPrimitives.ReadInt16LittleEndian(data);
}

