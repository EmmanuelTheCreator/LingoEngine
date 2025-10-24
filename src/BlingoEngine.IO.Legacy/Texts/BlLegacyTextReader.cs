using System;
using System.Collections.Generic;

using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Texts.Data;

namespace BlingoEngine.IO.Legacy.Texts;

/// <summary>
/// Reads text payloads referenced by <c>CASt</c> entries. Director stores styled text in <c>XMED</c>
/// chunks and continues to emit plain <c>STXT</c> resources for early projector versions, so the
/// reader resolves those resources through the key table, inflates them when necessary, and
/// surfaces the raw bytes alongside the detected format. Byte layouts for both resource types are
/// summarized in <c>BlingoEngine.IO.Legacy/docs/LegacyTextFieldMembers.md</c> to help downstream decoders interpret the
/// <see cref="BlLegacyText.Bytes"/> buffer.
/// </summary>
internal sealed class BlLegacyTextReader
{
    private static readonly BlTag _xmedTag = BlTag.Get("XMED");
    private static readonly BlTag _stxtTag = BlTag.Get("STXT");

    private static readonly BlTag[] _candidateTags =
    {
        _xmedTag,
        _stxtTag
    };

    private static readonly HashSet<BlTag> _childTags = new(_candidateTags);

    private static readonly HashSet<BlTag> _standaloneTags = new(_candidateTags);

    private readonly ReaderContext _context;

    public BlLegacyTextReader(ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public IReadOnlyList<BlLegacyText> Read()
    {
        var texts = new List<BlLegacyText>();
        if (_context.Resources.Entries.Count == 0)
            return texts;

        var classicLoader = new BlClassicPayloadLoader(_context);
        BlAfterburnerPayloadLoader? afterburnerLoader = null;
        if (_context.AfterburnerState is not null)
            afterburnerLoader = new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);

        var processed = new HashSet<int>();
        var entriesById = _context.Resources.EntriesById;

        foreach (var pair in _context.Resources.ChildrenByParent)
        {
            foreach (var link in pair.Value)
            {
                if (!_childTags.Contains(link.Tag))
                    continue;

                if (!entriesById.TryGetValue(link.ChildId, out var child))
                    continue;

                if (!processed.Add(child.Id))
                    continue;

                var payload = LoadPayload(child, classicLoader, afterburnerLoader);
                if (payload.Length == 0)
                {
                    processed.Remove(child.Id);
                    continue;
                }

                var format = DetectFormat(child.Tag);
                texts.Add(new BlLegacyText(child.Id, format, payload));
            }
        }

        foreach (var entry in _context.Resources.Entries)
        {
            if (!_standaloneTags.Contains(entry.Tag))
                continue;

            if (!processed.Add(entry.Id))
                continue;

            var payload = LoadPayload(entry, classicLoader, afterburnerLoader);
            if (payload.Length == 0)
            {
                processed.Remove(entry.Id);
                continue;
            }

            var format = DetectFormat(entry.Tag);
            texts.Add(new BlLegacyText(entry.Id, format, payload));
        }

        return texts;
    }

    private static byte[] LoadPayload(
        BlLegacyResourceEntry entry,
        BlClassicPayloadLoader classicLoader,
        BlAfterburnerPayloadLoader? afterburnerLoader)
    {
        return entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
            ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
            : entry.ReadClassicPayload(classicLoader);
    }

    private static BlLegacyTextFormatKind DetectFormat(BlTag tag)
    {
        if (tag == _xmedTag)
            return BlLegacyTextFormatKind.Xmed;

        if (tag == _stxtTag)
            return BlLegacyTextFormatKind.Stxt;

        return BlLegacyTextFormatKind.Unknown;
    }
}

internal static class BlLegacyTextReaderExtensions
{
    public static IReadOnlyList<BlLegacyText> ReadTexts(this ReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var reader = new BlLegacyTextReader(context);
        return reader.Read();
    }
}
