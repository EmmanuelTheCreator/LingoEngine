using System;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Data;

using KeyTableEntry = BlingoEngine.IO.Legacy.Cast.BlLegacyCastLibraryBuilderHelpers.KeyTableEntry;

namespace BlingoEngine.IO.Legacy.Sounds;

/// <summary>
/// Provides helpers for synthesising minimal cast libraries that embed MP3 sound members. The
/// generated resources mirror the <c>KEY*</c>, <c>CAS*</c>, <c>CASt</c>, and <c>ediM</c> layout used by
/// modern Director authoring movies so the resulting container can be consumed by the existing
/// reader pipeline.
/// </summary>
public static class BlLegacySoundLibraryBuilder
{
    private const uint KeyResourceId = 1;
    private const uint CastTableResourceId = 2;
    private const uint CastMemberResourceId = 3;
    private const uint EditorResourceId = 4;

    private static readonly BlTag EditorTag = BlTag.Get("ediM");

    /// <summary>
    /// Builds a <see cref="DirFilesContainerDTO"/> that contains a single sound cast member backed by
    /// the supplied MP3 payload. The generated container is ready to be persisted through the
    /// <see cref="BlCstFileWriter"/>.
    /// </summary>
    /// <param name="memberName">Display name stored in the <c>CASt</c> metadata.</param>
    /// <param name="mp3Bytes">Raw MP3 bytes that populate the <c>ediM</c> chunk.</param>
    /// <returns>A container that exposes the resources necessary to emit a cast library.</returns>
    public static DirFilesContainerDTO BuildSingleMemberMp3Library(string? memberName, ReadOnlySpan<byte> mp3Bytes)
    {
        if (mp3Bytes.IsEmpty)
        {
            throw new ArgumentException("MP3 payload must contain at least one byte.", nameof(mp3Bytes));
        }

        if (mp3Bytes.Length > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(mp3Bytes), "MP3 payload exceeds the supported size for legacy archives.");
        }

        var container = new DirFilesContainerDTO();

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{BlTag.KeyStar.Value}_{KeyResourceId:D4}.bin",
            Bytes = BuildKeyTable()
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"{BlTag.CasStar.Value}_{CastTableResourceId:D4}.bin",
            Bytes = BuildCastTable()
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"CASt_{CastMemberResourceId:D4}.bin",
            Bytes = BuildCastMember(memberName)
        });

        container.Files.Add(new DirFileResourceDTO
        {
            FileName = $"ediM_{EditorResourceId:D4}.bin",
            Bytes = mp3Bytes.ToArray()
        });

        return container;
    }

    private static byte[] BuildKeyTable()
    {
        var entries = new[]
        {
            new KeyTableEntry(EditorResourceId, CastMemberResourceId, EditorTag)
        };

        return BlLegacyCastLibraryBuilderHelpers.BuildKeyTable(entries);
    }

    private static byte[] BuildCastTable()
        => BlLegacyCastLibraryBuilderHelpers.BuildCastTable(CastMemberResourceId);

    private static byte[] BuildCastMember(string? memberName)
    {
        return BlLegacyCastLibraryBuilderHelpers.BuildModernCastMetadata(
            BlLegacyCastMemberType.Sound,
            memberName,
            dataLength: 0u);
    }
}
