namespace BlingoEngine.IO.Legacy.Cast.Data;
[Flags]
public enum BlRawCastInfoFlags : byte
{
    None = 0,
    Unused1 = 1 << 0,
    Unused2 = 1 << 1,
    DtsOff = 1 << 2, // bit 0x04
                     // Add future flags here
}

/// <summary>
/// Describes a single populated entry inside the <c>CAS*</c> table. The slot index represents the
/// position within the table while the resource identifier points to the <c>CASt</c> chunk that
/// contains the cast-member data.
/// </summary>
/// <param name="SlotIndex">Zero-based position of the entry within the table.</param>
/// <param name="ResourceId">Identifier of the <c>CASt</c> resource referenced by the slot.</param>
public readonly record struct BlCastRawMemberSlot(int SlotIndex, int ResourceId, BlCastRawMemberItem Member);
