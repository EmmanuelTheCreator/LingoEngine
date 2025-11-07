namespace BlingoEngine.IO.Legacy.Cast.Data;

public enum BlLegacyTextFraming : byte
{
    Fixed = 0x00,
    Scrolling = 0x01,
    AdjustToFit = 0x02
}
[Flags]
public enum BlLegacyCastInfoFlags : byte
{
    None = 0,
    Unused1 = 1 << 0,
    Unused2 = 1 << 1,
    DtsOff = 1 << 2, // bit 0x04
                     // Add future flags here
}
public enum BlLegacyTextAntiAlias : byte { None = 0, AllText = 1, LargerThan = 2 }
public enum BlLegacyTextKerningMode : byte { None = 0x44, AllText = 0x30, LargerThan = 0x75 }
/// <summary>
/// Describes a single populated entry inside the <c>CAS*</c> table. The slot index represents the
/// position within the table while the resource identifier points to the <c>CASt</c> chunk that
/// contains the cast-member data.
/// </summary>
/// <param name="SlotIndex">Zero-based position of the entry within the table.</param>
/// <param name="ResourceId">Identifier of the <c>CASt</c> resource referenced by the slot.</param>
public readonly record struct BlLegacyCastMemberSlot(int SlotIndex, int ResourceId, BlCastRawMemberItem Member);
