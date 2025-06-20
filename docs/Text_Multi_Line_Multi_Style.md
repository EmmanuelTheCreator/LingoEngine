# Text_Multi_Line_Multi_Style.cst

This note collects known offsets for the `Text_Multi_Line_Multi_Style.cst` sample. The file contains two copies of the XMED data as explained in Anthony Kleine's memory-map documentation.  The second copy begins at `0x20E4` and is the one referenced by the memory map.
This cast comes from Director MX 2004; the structure matches the 8.5 notes.
See [XMED_Offsets.md](XMED_Offsets.md) and [XMED_FileComparisons.md](XMED_FileComparisons.md) for additional details.

## HEADER

| Byte Address | Δ prev | Bytes Length | Bit | Description | Notes |
|-------------:|-------:|-------------:|----|-------------|-------|
| 0x110C | — | 4 | | DEMX header | obsolete copy |

### Default style block

The file begins with a short descriptor that defines the base font and several
flags used when no other style is applied.  Only a handful of values are clear
so far:

| Byte Address | Δ prev | Bytes Length | Bit | Description | Notes |
|-------------:|-------:|-------------:|----|-------------|-------|
| 0x0018 | —        | 4  |    | width value `0x075E` | twips |
| 0x001C | +0x04   | 1  |    | style flags `0x44` | underline/tabbed |
| 0x001D | +0x01   | 1  |    | alignment byte `0x07` | meaning unknown |
| 0x001E | +0x01   | 2  |    | zeros | reserved |
| 0x0020 | +0x02   | 12 |    | zero padding | |
| 0x002C | +0x0C   | 4  |    | ASCII `*YEK` | header token |
| 0x0030 | +0x04   | 4  |    | possible FG colour bytes `FC000000` | pairs with next row |
| 0x0034 | +0x04   | 4  |    | digits `0C000C00` (likely BG colour) | |
| 0x0038 | +0x04   | 4  |    | digits `14000000` | may store margin data |
|           |         |    |    | *uses color table near 0x1114* |
| 0x003C | +0x04   | 4  |    | line spacing `0x00000006` | |
| 0x0040 | +0x04   | 4  |    | font size `0x0E` (14px) | |
| 0x0044 | +0x04   | 4  |    | unknown `0x00000004` | |
| 0x0048 | +0x04   | 4  |    | ASCII `muhT` | token |
| 0x004C | +0x04   | 4  |    | text length `0x0000000D` | |
| 0x0050 | +0x04   | 4  |    | unknown `0x00000004` | |
| 0x0054 | +0x04   | 4  |    | ASCII `DEMX` | marker |
| 0x0058 | +0x04   | 4  |    | value `09000000` | |
| 0x005C | +0x04   | 4  |    | value `00040000` | |
| 0x0060 | +0x04   | 4  |    | ASCII `*SAC` | |
| 0x0064 | +0x04   | 4  |    | value `0B000000` | |
| 0x0068 | +0x04   | 4  |    | value `00040000` | |
| 0x006C | +0x04   | 4  |    | ASCII `fniC` | |

## unknown
| Byte Address | Δ prev | Bytes Length | Bit | Description | Notes |
|-------------:|-------:|-------------:|----|-------------|-------|

| 0x120A | 0x0FE | ~0x12A | | text string (first copy) | lines of styled text |

## Stylemap table
| Byte Address | Δ prev | Bytes Length | Bit | Description | Notes |
|-------------:|-------:|-------------:|----|-------------|-------|
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

### Style 0008 (Arial)
Offset `0x16A8` stores the first descriptor. The header bytes `30 82` encode bold and italic flags. The short `40,` token before the font name holds the size `12px`. The final byte `05` selects color index five.
| Byte Address | Δ prev | Bytes Length | Bit | Description | Notes |
|-------------:|-------:|-------------:|----|-------------|-------|
| 0x16A8 | — | 1 | | style flags `0x30` | matches field header |
| 0x16A9 | +0x01 | 1 | | alignment flags `0x82` | italic + editable |
| 0x16AA | +0x01 | 5 | | unknown numbers | header values |
| 0x16AF | +0x05 | 20 | | "00080000036600000005" | fields: StyleId=0008, TextLength=0366, BaseStyleId=0005 |
| 0x16C3 | +0x14 | 1 | | null separator | |
| 0x16C4 | +0x01 | 3 | | ASCII `40,` | font size token (12px) |
| 0x16C7 | +0x03 | 1 | | color index `05` (see color table) | |
| 0x16C8 | +0x01 | 5 | | font name "Arial" | |
| 0x16CD | +0x05 | 11 | | zero padding | |

#### Inheritance check
BaseStyleId `0005` links this descriptor to an earlier style record.  When
building the final appearance start from the default block, apply the style with
ID `0005`, then override its fields with those from style `0008`.  This chain
renders Arial 12px centered text in red, matching the first line.
### Style 0006 (Tahoma)
At `0x18C4` another block repeats the layout with the font `Tahoma` and color index `06` (see color table). The flag byte `02` denotes left alignment.
| Byte Address | Δ prev | Bytes Length | Bit | Description | Notes |
|-------------:|-------:|-------------:|----|-------------|-------|
| 0x18C4 | — | 1 | | style flags `0x02` | left alignment |
| 0x18C5 | +0x01 | 1 | | alignment byte `0x30` | |
| 0x18C6 | +0x01 | 1 | | unknown value `0x82` | |
| 0x18C7 | +0x01 | 1 | | unknown value `0x02` | |
| 0x18C8 | +0x01 | 4 | | digits "3101" | header numbers |
| 0x18CC | +0x04 | 1 | | `0x30` | separator |
| 0x18CD | +0x01 | 1 | | null | |
| 0x18CE | +0x01 | 3 | | ASCII `40,` | font size token (12px) |
| 0x18D1 | +0x03 | 1 | | color index `06` (see color table) | |
| 0x18D2 | +0x01 | 6 | | font name "Tahoma" | |
| 0x18D8 | +0x06 | 24 | | padding zeros | |
#### Inheritance check
BaseStyleId `000B` indicates this style builds on the Terminal descriptor.  The
chain starts with the default block, then applies style `000B` before this
record.  The result is yellow Tahoma 9px left aligned text as shown in line two.

### Style 000B (Terminal)
The block at `0x196E` references `Terminal` with index `0B` and the same alignment bytes as the Tahoma style.
| Byte Address | Δ prev | Bytes Length | Bit | Description | Notes |
|-------------:|-------:|-------------:|----|-------------|-------|
| 0x196E | — | 1 | | style flags `0x30` | inherits previous |
| 0x196F | +0x01 | 1 | | alignment byte `0x30` | |
| 0x1970 | +0x01 | 1 | | unknown `0x30` | |
| 0x1971 | +0x01 | 1 | | unknown `0x38` | |
| 0x1972 | +0x01 | 1 | | unknown `0x02` | |
| 0x1973 | +0x01 | 3 | | bytes `30 82 02` | header | 
| 0x1976 | +0x03 | 4 | | digits "3101" | |
| 0x197A | +0x04 | 1 | | `0x30` | separator |
| 0x197B | +0x01 | 1 | | null | |
| 0x197C | +0x01 | 3 | | ASCII `40,` | size token |
| 0x197F | +0x03 | 1 | | color index `08` (see color table) | |
| 0x1980 | +0x01 | 8 | | font name "Terminal" | |
| 0x1988 | +0x08 | 16 | | padding zeros | |
#### Inheritance check
BaseStyleId `0002` ties this descriptor back to the default style block.  Using
the default values and then applying these bytes yields Terminal 18px left
aligned text in green, matching the third line.

### Style 0003 (Tahoma copy)
Offset `0x1A30` mirrors the previous blocks but links to style ID `0003`. The alignment bytes match those of the yellow line.

#### Inheritance check
This copied Tahoma style lists BaseStyleId `0002`, so the final line inherits
from the default block and then overrides with this descriptor to display orange
text.
The later descriptors starting at `0x2680` repeat these structures verbatim. Their offsets correspond to the second XMED copy.

### Fonts near `0x2390`

Starting at byte `0x2390` a new block lists the fonts used by each style. Each entry begins with the size token `40,` followed by the ASCII font name and a one‑byte color index.  The fonts appear in the same order as their descriptor blocks.

| Byte Address | Δ prev | Bytes Length | Bit | Description | Notes |
|-------------:|-------:|-------------:|----|-------------|-------|
| 0x269C | 0x30C | 5 | | font `Arial` index 5 | copy of style 0008 |
| 0x274A | 0x0AE | 8 | | font `Arcade *` index 8 | |
| 0x27F8 | 0x0AE | 5 | | font `arial` index 5 | lowercase repeat |
| 0x28A6 | 0x0AE | 6 | | font `Tahoma` index 6 | |
| 0x2954 | 0x0AE | 8 | | font `Terminal` index 8 | |
### Color table

The sequence `FFFF0000000600040001` begins at byte offset `0x1114` right after the obsolete DEMX header. It defines five RGB entries used by the style descriptors.

### Line style entries

The table below decodes the 20‑digit records found at `0x1334`. Field five in each row references the descriptor blocks above. The same values repeat at `0x230C` for the second copy.

| Offset | StyleId | F2 | TextLength | F4 | BaseStyleId | Notes |
|------:|-------:|---:|-----------:|---:|-----------:|------|
| 0x1334 | 0004 | 0000 | 0029 | 0000 | 0008 | line 1: red, Arial, centered |
| 0x1371 | 0005 | 0000 | 001F | 0000 | 0006 | line 2: yellow, Tahoma |
| 0x13A4 | 0006 | 0000 | 0273 | 0000 | 000B | line 3: green, Terminal |
| 0x162B | 0007 | 0000 | 0070 | 0000 | 0003 | line 4: orange, Tahoma |
| 0x16AF | 0008 | 0000 | 0366 | 0000 | 0005 | line 5: red again |
| 0x1A29 | 0009 | 0000 | 0015 | 0000 | 0002 | table terminator |

### Style override chain

The BaseStyleId column forms a hierarchy where each descriptor overrides the one
before it.  Starting from the default block (ID `0002`), later styles layer on
additional attributes.

| Style ID | BaseStyleId | Notes |
|--------:|------------:|-------|
| 000B | 0002 | Terminal inherits from the defaults |
| 0006 | 000B | Tahoma builds on Terminal |
| 0005 | 0006 | map entry only, no separate descriptor |
| 0008 | 0005 | final Arial style used for lines 1 and 5 |
| 0003 | 0002 | orange Tahoma style |

## Trailing block

Unknown bytes follow the final style descriptor. The segment may hold font or
color indices reused by later Director versions.

| Byte Address | Δ prev | Bytes Length | Bit | Description | Notes |
|-------------:|-------:|-------------:|----|-------------|-------|
| 0x29D0 | 0x707 | 16 | | bytes `00 00 00 00 00 00 00 00` | start of numeric table |
| 0x29E0 | +0x10 | 16 | | digits `01 34 01 30 81 01 36 30` | unknown meaning |
| 0x29F0 | +0x10 | 16 | | digits `82 02 31 02 46 46 02 30` | continuation |
| 0x2A20 | +0x30 | 32 | | digits `30 82 02 31 35 32 ...` | numeric table |
