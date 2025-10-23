using System;
using System.Collections.Generic;
using System.IO;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

using KeyTableEntry = BlingoEngine.IO.Legacy.Cast.BlLegacyCastLibraryBuilderHelpers.KeyTableEntry;

namespace BlingoEngine.IO.Legacy.Bitmaps;

/// <summary>
/// Encodes synthetic bitmap resources for test scenarios. The writer mirrors the classic
/// eight-byte chunk prefix described in <c>src/BlingoEngine.IO.Legacy/docs/LegacyBitmapLoading.md</c> — a four-character tag
/// followed by a big-endian payload length — so <see cref="BlClassicPayloadLoader"/> can resolve the
/// bytes using the legacy resource map across every Director generation.
/// </summary>
internal sealed class BlLegacyBitmapWriter
{
    private static readonly BlTag BitdTag = BlTag.Get("BITD");
    private static readonly BlTag DibTag = BlTag.Get("DIB ");
    private static readonly BlTag PictTag = BlTag.Get("PICT");
    private static readonly BlTag EditorTag = BlTag.Get("ediM");
    private static readonly BlTag AlphaTag = BlTag.Get("ALFA");
    private static readonly BlTag ThumbTag = BlTag.Get("Thum");

    private readonly BlStreamWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyBitmapWriter"/> class.
    /// </summary>
    /// <param name="stream">Destination stream that receives the bitmap chunks.</param>
    public BlLegacyBitmapWriter(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _writer = new BlStreamWriter(stream)
        {
            Endianness = BlEndianness.BigEndian
        };
    }

    /// <summary>
    /// Writes a Macintosh BITD bitmap chunk, which older Director (≤ 3) projectors stored in the
    /// resource map. The payload should contain the run-length encoded pixel rows documented in the
    /// legacy byte tables.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw BITD bytes (control stream followed by literal/repeat data).</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteBitd(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, BitdTag, payload);

    /// <summary>
    /// Writes a Windows <c>DIB </c> resource that Director 4/5 emitted alongside the classic BITD
    /// chunk. The payload should begin with one of the BITMAPINFOHEADER sizes (0x0C, 0x28, 0x40,
    /// 0x6C, or 0x7C) so the loader can classify the format correctly.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw DIB bytes including the palette + BITMAPINFOHEADER layout.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteDib(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, DibTag, payload);

    /// <summary>
    /// Writes a QuickDraw <c>PICT</c> chunk used by classic Macintosh projectors when the member
    /// stored a drawing instead of a bitmap.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw PICT bytes copied from the resource stream.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WritePict(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, PictTag, payload);

    /// <summary>
    /// Writes an authoring metadata (<c>ediM</c>) chunk that can embed PNG/JPEG/DIB bytes in modern
    /// movies. Tests can feed either valid image signatures or arbitrary metadata to exercise the
    /// fallback paths documented for Director 6+.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw metadata bytes, typically starting with a modern image signature.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteEditorMetadata(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, EditorTag, payload);

    /// <summary>
    /// Writes an <c>ALFA</c> chunk representing the standalone alpha mask that Director 5+ stored next
    /// to bitmap members.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw alpha bytes recorded in the resource stream.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteAlphaMask(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, AlphaTag, payload);

    /// <summary>
    /// Writes the <c>Thum</c> thumbnail payload produced by newer authoring tools alongside the
    /// primary bitmap.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="payload">Raw thumbnail bytes to persist inside the chunk.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteThumbnail(int resourceId, ReadOnlySpan<byte> payload)
        => WriteResource(resourceId, ThumbTag, payload);

    /// <summary>
    /// Writes a resource chunk using the standard 8-byte prefix (tag + big-endian length) followed
    /// by the raw payload bytes. The returned entry can be registered in a
    /// <see cref="ReaderContext"/> so unit tests mimic the memory-map layout used by Director.
    /// </summary>
    /// <param name="resourceId">Identifier assigned to the resource entry.</param>
    /// <param name="tag">Four-character code associated with the payload.</param>
    /// <param name="payload">Raw payload bytes to write after the chunk header.</param>
    /// <returns>A <see cref="BlLegacyResourceEntry"/> that points at the encoded chunk.</returns>
    public BlLegacyResourceEntry WriteResource(int resourceId, BlTag tag, ReadOnlySpan<byte> payload)
    {
        if (resourceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(resourceId), "Resource identifiers must be positive.");

        var payloadLength = payload.Length;
        var declaredSize = checked((uint)payloadLength);

        var offset = _writer.Position;
        if (offset < 0 || offset > uint.MaxValue)
            throw new InvalidOperationException("Chunk offset exceeds the range supported by legacy maps.");

        _writer.WriteTag(tag);
        _writer.WriteUInt32(declaredSize);
        _writer.WriteBytes(payload);

        return new BlLegacyResourceEntry(resourceId, tag, declaredSize, (uint)offset, 0, 0, 0);
    }
}

/// <summary>
/// Provides helpers for synthesising bitmap cast libraries backed by classic <c>BITD</c>, <c>DIB </c>,
/// or modern <c>ediM</c> metadata streams. The generated container mirrors the <c>KEY*</c>,
/// <c>CAS*</c>, and <c>CASt</c> resources emitted by Director so the resulting archive can be consumed
/// by the existing reader pipeline.
/// </summary>
public static class BlLegacyBitmapLibraryBuilder
{
    private const uint KeyResourceId = 1;
    private const uint CastTableResourceId = 2;
    private const uint CastMemberResourceId = 3;
    private const uint FirstBitmapResourceId = 4;

    private static readonly BlTag EditorTag = BlTag.Get("ediM");
    private static readonly BlTag BitdTag = BlTag.Get("BITD");
    private static readonly BlTag DibTag = BlTag.Get("DIB ");
    private static readonly BlTag PictTag = BlTag.Get("PICT");
    private static readonly BlTag AlphaTag = BlTag.Get("ALFA");
    private static readonly BlTag ThumbTag = BlTag.Get("Thum");

    /// <summary>
    /// Builds a <see cref="DirFilesContainerDTO"/> containing a single bitmap cast member. Callers
    /// can provide any combination of the legacy payloads – authoring metadata, BITD, DIB, PICT,
    /// alpha, or thumbnail streams – and the builder will register each as a child resource under the
    /// generated <c>CASt</c> record.
    /// </summary>
    /// <param name="memberName">Display name stored in the <c>CASt</c> metadata.</param>
    /// <param name="metadataBytes">Optional <c>ediM</c> payload (PNG/JPEG/DIB) exposed to newer Director releases.</param>
    /// <param name="bitdBytes">Optional classic <c>BITD</c> payload for Macintosh projectors.</param>
    /// <param name="dibBytes">Optional Windows <c>DIB </c> payload paired with the classic bitmap.</param>
    /// <param name="pictBytes">Optional QuickDraw <c>PICT</c> bytes stored alongside drawings.</param>
    /// <param name="alphaMaskBytes">Optional <c>ALFA</c> payload that carries the standalone alpha mask.</param>
    /// <param name="thumbnailBytes">Optional <c>Thum</c> payload exported by newer authoring tools.</param>
    /// <returns>A container populated with the resources necessary to emit a cast library.</returns>
    /// <exception cref="ArgumentException">Thrown when no bitmap payloads are provided.</exception>
    public static DirFilesContainerDTO BuildSingleMemberBitmapLibrary(
        string? memberName,
        byte[]? metadataBytes = null,
        byte[]? bitdBytes = null,
        byte[]? dibBytes = null,
        byte[]? pictBytes = null,
        byte[]? alphaMaskBytes = null,
        byte[]? thumbnailBytes = null)
    {
        if (IsEmpty(metadataBytes)
            && IsEmpty(bitdBytes)
            && IsEmpty(dibBytes)
            && IsEmpty(pictBytes)
            && IsEmpty(alphaMaskBytes)
            && IsEmpty(thumbnailBytes))
        {
            throw new ArgumentException("At least one bitmap payload must be provided.", nameof(bitdBytes));
        }

        var container = new DirFilesContainerDTO();
        var keyEntries = new List<KeyTableEntry>();
        var childResources = new List<DirFileResourceDTO>();
        uint nextResourceId = FirstBitmapResourceId;

        AddResource(childResources, keyEntries, ref nextResourceId, metadataBytes, EditorTag);
        AddResource(childResources, keyEntries, ref nextResourceId, bitdBytes, BitdTag);
        AddResource(childResources, keyEntries, ref nextResourceId, dibBytes, DibTag);
        AddResource(childResources, keyEntries, ref nextResourceId, pictBytes, PictTag);
        AddResource(childResources, keyEntries, ref nextResourceId, alphaMaskBytes, AlphaTag);
        AddResource(childResources, keyEntries, ref nextResourceId, thumbnailBytes, ThumbTag);

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{BlTag.KeyStar.Value}_{KeyResourceId:D4}.bin",
            Bytes = BlLegacyCastLibraryBuilderHelpers.BuildKeyTable(keyEntries)
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{BlTag.CasStar.Value}_{CastTableResourceId:D4}.bin",
            Bytes = BlLegacyCastLibraryBuilderHelpers.BuildCastTable(CastMemberResourceId)
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"CASt_{CastMemberResourceId:D4}.bin",
            Bytes = BuildCastMember(memberName)
        });

        foreach (var resource in childResources)
        {
            container.Files.Add(resource);
        }

        return container;
    }

    private static byte[] BuildCastMember(string? memberName)
        => BlLegacyCastLibraryBuilderHelpers.BuildModernCastMetadata(
            BlLegacyCastMemberType.Bitmap,
            memberName,
            dataLength: 0u);

    private static void AddResource(
        List<DirFileResourceDTO> resources,
        List<KeyTableEntry> keyEntries,
        ref uint nextResourceId,
        byte[]? payload,
        BlTag tag)
    {
        if (IsEmpty(payload))
        {
            return;
        }

        var resourceId = nextResourceId++;
        resources.Add(new DirFileResourceDTO
        {
            FileName = $"{tag.Value}_{resourceId:D4}.bin",
            Bytes = payload!
        });

        keyEntries.Add(new KeyTableEntry(resourceId, CastMemberResourceId, tag));
    }

    private static bool IsEmpty(byte[]? payload) => payload is null || payload.Length == 0;
}
