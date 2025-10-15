using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;

namespace BlingoEngine.IO.Legacy.Cast;

/// <summary>
/// Reads <c>CAS*</c> resources and exposes the cast-member tables they contain. Each table stores a
/// packed list of 32-bit identifiers that point to individual <c>CASt</c> resources. The loader keeps
/// the slot index for every populated entry so higher layers can reconstruct cast numbering.
/// </summary>
internal sealed class BlLegacyCastReader
{
    private readonly ReaderContext _context;
    
   

    public BlLegacyCastReader(ReaderContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Iterates over all registered <c>CAS*</c> resources, loading their payload bytes and decoding the
    /// cast-member tables contained within. The reader respects both classic chunk storage and
    /// Afterburner inline segments, inflating compressed data when necessary.
    /// </summary>
    public IReadOnlyList<BlLegacyCastLibrary> Read()
    {
        var libraries = new List<BlLegacyCastLibrary>();
        if (_context.Resources.Entries.Count == 0)
            return libraries;

        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (_context.AfterburnerState is not null)
        {
            afterburnerLoader = new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);
        }

        foreach (var entry in _context.Resources.Entries)
        {
            if (entry.Tag != BlTag.CasStar)
                continue;

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            var library = ParseLibrary(entry, payload, classicLoader, afterburnerLoader);
            libraries.Add(library);
        }

        return libraries;
    }

    private static byte[] LoadPayload(BlLegacyResourceEntry entry, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        return entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
            ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
            : entry.ReadClassicPayload(classicLoader);
    }

    private BlLegacyCastLibrary ParseLibrary(BlLegacyResourceEntry entry, byte[] payload, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        int? parentId = null;
        if (_context.Resources.ParentByChild.TryGetValue(entry.Id, out var link))
            parentId = link.ParentId;

        var entryCount = payload.Length / 4;
        var library = new BlLegacyCastLibrary(entry.Id, parentId, entryCount);
        if (entryCount == 0)
        {
            return library;
        }

        var reader = _context.CreateMemoryReader(payload, BlEndianness.BigEndian);
        for (var slot = 0; slot < entryCount; slot++)
        {
            var castResourceId = unchecked((int)reader.ReadUInt32());
            if (castResourceId == 0)
                continue;

            library.MemberSlots.Add(CreateMember(slot, castResourceId, classicLoader, afterburnerLoader));
        }

        return library;
    }

    private BlLegacyCastMemberSlot CreateMember(int slot, int resourceId, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        CastMemberInfo memberInfo = new();
        if (_context.Resources.TryGetEntry(resourceId, out var memberEntry))
        {
            var memberPayload = LoadPayload(memberEntry, classicLoader, afterburnerLoader);
            if (memberPayload.Length > 0 && TryParseMemberChunk(memberPayload, out var memberInfo1))
                memberInfo = memberInfo1!;
        }

        return new BlLegacyCastMemberSlot(slot, resourceId, memberInfo!.MemberType, memberInfo.Name, memberInfo.Flags, memberInfo.TextFraming, memberInfo.AntiAlias, memberInfo.AntiAliasThreashold, memberInfo.Kerning, memberInfo.KerningThreashold, memberInfo.Ink, memberInfo.UseHyperlinkStyles, memberInfo.IsEditable);
    }

    private static bool TryParseMemberChunk(byte[] payload, out CastMemberInfo? castMemberInfo)
    {
        castMemberInfo = null;
        if (payload.Length < 12)
            return false;

        using var memory = new MemoryStream(payload, writable: false);
        var reader = new BlStreamReader(memory)
        {
            Endianness = BlEndianness.BigEndian
        };

        var typeValue = reader.ReadUInt32();
        var memberType = BlLegacyCastMemberTypeHelpers.MapMemberType(typeValue);

        var infoLength = reader.ReadUInt32();
        var specificLength = reader.ReadUInt32();

        var infoBytesAvailable = payload.Length - (int)reader.Position;
        if (infoBytesAvailable <= 0)
            return true;

        if (infoLength > (uint)infoBytesAvailable)
            infoLength = (uint)infoBytesAvailable;

        var infoData = infoLength > 0 ? reader.ReadBytes((int)infoLength) : Array.Empty<byte>();
        var flags = ReadCastInfoFlags(infoData);
        var textFraming = ReadTextFraming(infoData);
        var (antiAlias, antiAliasThreashold) = ReadAntiAlias(infoData);
        var (kerning, kerningThreshold) = ReadKerning(infoData);
        var ink = ReadInk(infoData);
        var useHyperlinkStyles = ReadUseHyperlinkStyles(infoData);
        var isEditable = ReadIsEditable(infoData);
        if (specificLength > 0)
        {
            var skip = Math.Min((int)specificLength, payload.Length - (int)reader.Position);
            if (skip > 0)
                reader.Skip(skip);
        }

        var name = ReadMemberName(infoData);
        castMemberInfo = new CastMemberInfo(memberType, name, flags, textFraming, antiAlias, antiAliasThreashold, kerning, kerningThreshold, ink, useHyperlinkStyles, isEditable);
        return true;
    }

    private static string ReadMemberName(byte[] infoData)
    {
        if (infoData.Length == 0)
            return string.Empty;
        var extracted = infoData.ExtractName();
        return !string.IsNullOrEmpty(extracted) ? extracted : string.Empty;
    }
    /// <summary>Reads DTS (Default Text Style) flag from Cinf.</summary>
    private static BlLegacyCastInfoFlags ReadCastInfoFlags(byte[] info) => (BlLegacyCastInfoFlags)Get(info, 0x44);

    private static BlLegacyTextFraming ReadTextFraming(byte[] info) => (BlLegacyTextFraming)Get(info, 0x46);

    private static (BlLegacyTextAntiAlias Mode, byte ThresholdPt) ReadAntiAlias(byte[] info)
    {
        var raw = Get(info, 0x8E);
        var thr = Get(info, 0xBA);
        var mode = (raw & 0x0F) switch { 0x06 => BlLegacyTextAntiAlias.None, 0x04 => BlLegacyTextAntiAlias.AllText, 0x02 => BlLegacyTextAntiAlias.LargerThan, _ => BlLegacyTextAntiAlias.None };
        return (mode, thr);
    }

    // ReadKerning
    private static (BlLegacyTextKerningMode Mode, byte ThresholdPt) ReadKerning(byte[] info)
    {
        var raw = Get(info, 0x8E);
        var thr = Get(info, 0xD2);
        var enabled = (Get(info, 0xCE) & 0x01) != 0;
        var mode = (raw & 0xF0) switch { 0x40 => BlLegacyTextKerningMode.None, 0x30 => BlLegacyTextKerningMode.AllText, 0x70 => BlLegacyTextKerningMode.LargerThan, _ => BlLegacyTextKerningMode.None };
        return (enabled ? mode : BlLegacyTextKerningMode.None, thr);
    }

    // ReadUseHyperlinkStyles
    public static bool ReadUseHyperlinkStyles(byte[] info) => Get(info, 0xD6) != 0;

    // ReadInk
    public static byte ReadInk(byte[] info) => Get(info, 0xE6);

    // ReadIsEditable
    private static bool ReadIsEditable(byte[] info) => Get(info, 0xA2) != 0;
    private static byte Get(byte[] a, int off, byte def = 0) => (a != null && a.Length > off) ? a[off] : def;
    private record CastMemberInfo(BlLegacyCastMemberType MemberType = BlLegacyCastMemberType.Null, string Name = "", BlLegacyCastInfoFlags Flags = BlLegacyCastInfoFlags.None, BlLegacyTextFraming TextFraming = BlLegacyTextFraming.Fixed, BlLegacyTextAntiAlias AntiAlias = BlLegacyTextAntiAlias.None, byte AntiAliasThreashold = 0, BlLegacyTextKerningMode Kerning = BlLegacyTextKerningMode.None, byte KerningThreashold = 0, byte Ink = 0, bool UseHyperlinkStyles = true, bool IsEditable = false);
}

/// <summary>
/// Extension helpers that expose the <see cref="BlLegacyCastReader"/> through the shared
/// <see cref="ReaderContext"/> type used by the legacy pipeline.
/// </summary>
internal static class BlLegacyCastReaderExtensions
{
    public static IReadOnlyList<BlLegacyCastLibrary> ReadCastLibraries(this ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var reader = new BlLegacyCastReader(context);
        return reader.Read();
    }
}
