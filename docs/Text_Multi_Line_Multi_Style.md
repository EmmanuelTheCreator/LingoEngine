# Text_Multi_Line_Multi_Style.cst

This note collects known offsets for the `Text_Multi_Line_Multi_Style.cst` sample. The file contains two copies of the XMED data as explained in Anthony Kleine's memory-map documentation.  The second copy begins at `0x20E4` and is the one referenced by the memory map.

## Byte blocks

| Byte Address | Δ prev | Bytes Length | Bit | Description | Notes |
|-------------:|-------:|-------------:|----|-------------|-------|
| 0x110C | — | 4 | | DEMX header | obsolete copy |
| 0x120A | 0x0FE | ~0x12A | | text string (first copy) | lines of styled text |
| 0x1334 | 0x12A | 120 | | style map table | six entries |
| 0x16A8 | 0x374 | 48 | | style descriptor 0008 | Arial red centered |
| 0x18C4 | 0x21C | 48 | | style descriptor 0006 | yellow left Tahoma |
| 0x196E | 0x0AA | 48 | | style descriptor 000B | Terminal green |
| 0x1A30 | 0x0C2 | 48 | | style descriptor 0003 | orange left |
| 0x21E1 | 0x8B1 | ~0x12A | | text string (second copy) | used version |
| 0x230C | 0x12B | 120 | | style map table (repeat) | six entries |
| 0x2680 | 0x374 | 48 | | style descriptor 0008 | duplicate of 0008 |
| 0x26A8 | 0x28 | 48 | | style descriptor 0005 | duplicate Arial |
| 0x289C | 0x1F4 | 48 | | style descriptor 0006 | duplicate Tahoma |
| 0x2946 | 0x0AA | 48 | | style descriptor 000B | duplicate Terminal |

## Style block details

### Style 0008 (Arial)
Offset `0x16A8` stores the first descriptor. The header bytes `30 82` encode bold and italic flags. The short `40,` token before the font name holds the size `12px`. The final byte `05` selects color index five.

### Style 0006 (Tahoma)
At `0x18C4` another block repeats the layout with the font `Tahoma` and color index `06`. The flag byte `02` denotes left alignment.

### Style 000B (Terminal)
The block at `0x196E` references `Terminal` with index `0B` and the same alignment bytes as the Tahoma style.

### Style 0003 (Tahoma copy)
Offset `0x1A30` mirrors the previous blocks but links to style ID `0003`. The alignment bytes match those of the yellow line.

The later descriptors starting at `0x2680` repeat these structures verbatim. Their offsets correspond to the second XMED copy.
