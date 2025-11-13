using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Cast;

/// <summary>
/// Shared helpers for synthesising minimal cast-library resources. The builder utilities centralise
/// the byte layouts for <c>KEY*</c> tables, <c>CAS*</c> member lists, and <c>CASt</c> metadata so
/// higher-level factories can focus on populating the media payloads.
/// </summary>
internal static class BlLegacyCastLibraryBuilderHelpers
{
    public readonly struct KeyTableEntry
    {
        public KeyTableEntry(uint childId, uint parentId, BlTag tag)
        {
            ChildId = childId;
            ParentId = parentId;
            Tag = tag;
        }

        public uint ChildId { get; }

        public uint ParentId { get; }

        public BlTag Tag { get; }
    }

    public static byte[] BuildKeyTable(IReadOnlyList<KeyTableEntry> entries)
    {
        if (entries is null)
        {
            throw new ArgumentNullException(nameof(entries));
        }

        int count = entries.Count;
        var payload = new byte[12 + (count * 12)];
        var span = payload.AsSpan();

        BinaryPrimitives.WriteUInt16LittleEndian(span[0..2], 12);
        BinaryPrimitives.WriteUInt16LittleEndian(span[2..4], 12);
        BinaryPrimitives.WriteUInt32LittleEndian(span[4..8], (uint)count);
        BinaryPrimitives.WriteUInt32LittleEndian(span[8..12], (uint)count);

        for (int i = 0; i < count; i++)
        {
            var entry = entries[i];
            int offset = 12 + (i * 12);
            BinaryPrimitives.WriteUInt32LittleEndian(span[offset..(offset + 4)], entry.ChildId);
            BinaryPrimitives.WriteUInt32LittleEndian(span[(offset + 4)..(offset + 8)], entry.ParentId);
            BinaryPrimitives.WriteUInt32LittleEndian(span[(offset + 8)..(offset + 12)], entry.Tag.ToUInt32BigEndian());
        }

        return payload;
    }

    public static byte[] BuildCastTable(uint castMemberResourceId)
    {
        var payload = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(payload, castMemberResourceId);
        return payload;
    }

    public static byte[] BuildNameInfoBytes(string? memberName)
    {
        var name = memberName ?? string.Empty;
        var bytes = Encoding.UTF8.GetBytes(name);
        if (bytes.Length > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(memberName), "Cast-member names must fit in a single length byte.");
        }

        var info = new byte[1 + bytes.Length];
        info[0] = (byte)bytes.Length;
        if (bytes.Length > 0)
        {
            bytes.CopyTo(info.AsSpan(1));
        }

        return info;
    }

    public static byte[] BuildModernCastMetadata(BlCastRawMemberType type, string? memberName, uint dataLength)
    {
        var info = BuildNameInfoBytes(memberName);
        var payload = new byte[12 + info.Length];

        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(0, 4), (uint)type);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(4, 4), (uint)info.Length);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(8, 4), dataLength);

        info.CopyTo(payload.AsSpan(12));
        return payload;
    }
}
