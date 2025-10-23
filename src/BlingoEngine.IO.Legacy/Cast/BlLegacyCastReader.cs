using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Cast.Data;
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
        BlCastMemberItem memberInfo = new();
        if (_context.Resources.TryGetEntry(resourceId, out var memberEntry))
        {
            var memberPayload = LoadPayload(memberEntry, classicLoader, afterburnerLoader);
            if (memberPayload.Length > 0 && TryParseMemberChunk(memberPayload, out var memberInfo1))
                memberInfo = memberInfo1!;
        }

        return new BlLegacyCastMemberSlot(slot, resourceId, memberInfo);
    }

    private static bool TryParseMemberChunk(byte[] payload, out BlCastMemberItem? castMemberInfo)
    {
        var memberData = new BlLegacyCastItemReader().ReadItem(string.Empty, payload);
        castMemberInfo = memberData.MemberItem;

        return true;
    }

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
