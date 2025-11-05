
using BlingoEngine.IO.Legacy.Cast.Data;

namespace BlingoEngine.IO.Legacy.Cast;

/// <summary>
/// Represents the table of cast-member resource identifiers stored inside a <c>CAS*</c> chunk.
/// Each slot mirrors the four-byte entries Director wrote to reference individual <c>CASt</c>
/// members that belong to the owning cast library.
/// </summary>
public sealed class BlLegacyCastLibrary
{
    public BlLegacyCastLibrary(int resourceId, int? libraryId, int entryCount)
    {
        ResourceId = resourceId;
        LibraryId = libraryId;
        EntryCount = entryCount;
    }

    /// <summary>
    /// Gets the resource identifier assigned to the <c>CAS*</c> table in the map.
    /// </summary>
    public int ResourceId { get; }

    /// <summary>
    /// Gets the parent resource identifier recorded in the <c>KEY*</c> table. When present this
    /// value identifies the cast library that owns the <c>CAS*</c> table.
    /// </summary>
    public int? LibraryId { get; }

    /// <summary>
    /// Gets the number of four-byte slots stored in the <c>CAS*</c> payload (including empty slots).
    /// </summary>
    public int EntryCount { get; }
    public CastPreload Preload { get; set;  }

    public string? CastPath { get; set; }
    public string? RowWidth { get; set; }
    public int VisibleColumnsFlags { get; set; }
    public int NumberOfVisibleMembers { get; set; }
    public bool ShowAsThumbList { get; set; }

    /// <summary>
    /// Gets the list of populated cast-member slots. Empty slots are omitted but their original
    /// index is preserved so consumers can reconstruct member numbering.
    /// </summary>
    public List<BlLegacyCastMemberSlot> MemberSlots { get; } = new();
    public string Name { get; internal set; } = "";
    public bool IsInternal { get; internal set; }

    public enum CastPreload
    {
        WhenNeeded = 0,
        AfterFrameOne = 1,
        BeforeFrameOne = 2,
    }
}
