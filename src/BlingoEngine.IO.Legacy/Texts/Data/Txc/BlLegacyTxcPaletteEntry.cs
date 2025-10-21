using BlingoEngine.IO.Legacy.Core;

namespace BlingoEngine.IO.Legacy.Texts.Data.Txc;

/// <summary>
/// Represents a single entry in the QuickDraw color table embedded inside a legacy
/// <c>TXc</c> resource. The <see cref="Value"/> field stores the palette index emitted
/// by Director, while <see cref="Color"/> exposes the converted RGB triple.
/// </summary>
public readonly record struct BlLegacyTxcPaletteEntry(ushort Value, BlLegacyColor Color);
