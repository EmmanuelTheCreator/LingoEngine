using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;

namespace BlingoEngine.IO.Legacy.Classic;

/// <summary>
/// Loads classic resource payloads by seeking to the 8-byte chunk headers pointed to by the <c>mmap</c> table. Each payload is
/// validated by comparing the stored four-character tag before reading the declared length.
/// </summary>
internal sealed class BlClassicPayloadLoader
{
    private readonly ReaderContext _context;

    public BlClassicPayloadLoader(ReaderContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Loads the raw chunk bytes for the supplied map entry.
    /// </summary>
    /// <param name="entry">Resource map entry describing the chunk location.</param>
    /// <returns>The raw chunk bytes or an empty array when the payload cannot be resolved.</returns>
    public byte[] Load(BlLegacyResourceEntry entry)
    {
        var reader = _context.Reader;
        var offsets = new[] { entry.MapOffset, _context.RifxOffset + entry.MapOffset };

        foreach (var candidate in offsets)
        {
            if (candidate < 0 || candidate > reader.Length - 8)
                continue;

            var restore = reader.Position;
            try
            {
                reader.Position = candidate;
                var chunkTag = reader.ReadTag();
                if (chunkTag != entry.Tag)
                    continue;

                var length = reader.ReadUInt32();
                if (length == 0)
                    return Array.Empty<byte>();

                if (length > int.MaxValue)
                    continue;

                return reader.ReadBytes((int)length);
            }
            catch (EndOfStreamException)
            {
                return Array.Empty<byte>();
            }
            finally
            {
                reader.Position = restore;
            }
        }

        return Array.Empty<byte>();
    }

    public static byte[]? ReadResource(ReaderContext context, BlTag tag)
    {
        var classicLoader = new BlClassicPayloadLoader(context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (context.AfterburnerState is not null)
            afterburnerLoader = new BlAfterburnerPayloadLoader(context, context.AfterburnerState);
        var vwsc = context.Resources.Entries.FirstOrDefault(e => e.Tag == tag);
        if (vwsc == null) return null;

        var payload = LoadPayload(vwsc, classicLoader, afterburnerLoader);
        return payload;
    }
    public static List<byte[]> ReadResources(ReaderContext context, BlTag tag)
    {
        var returnData = new List<byte[]>();
        var classicLoader = new BlClassicPayloadLoader(context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (context.AfterburnerState is not null)
            afterburnerLoader = new BlAfterburnerPayloadLoader(context, context.AfterburnerState);
        var vwsc = context.Resources.Entries.Where(e => e.Tag == tag);
        foreach (var entry in vwsc)
        {
            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            returnData.Add(payload);
        }
        return returnData;
    }
    private static byte[] LoadPayload(BlLegacyResourceEntry entry, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        return entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
            ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
            : entry.ReadClassicPayload(classicLoader);
    }
}

/// <summary>
/// Convenience helpers that expose classic chunk loading operations on the reader context and entries.
/// </summary>
internal static class BlClassicPayloadLoaderExtensions
{
    public static byte[] ReadClassicPayload(this ReaderContext context, BlLegacyResourceEntry entry)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entry);

        var loader = new BlClassicPayloadLoader(context);
        return loader.Load(entry);
    }

    public static byte[] ReadClassicPayload(this BlLegacyResourceEntry entry, ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(context);

        var loader = new BlClassicPayloadLoader(context);
        return loader.Load(entry);
    }

    public static byte[] ReadClassicPayload(this BlLegacyResourceEntry entry, BlClassicPayloadLoader loader)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(loader);

        return loader.Load(entry);
    }
}
