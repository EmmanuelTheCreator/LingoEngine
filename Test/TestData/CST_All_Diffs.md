# CST Byte Differences

Compared with `Text_Hallo.cst`. Showing first 5 differing bytes and length difference if any.



| File | Length diff | First diffs |
|------|-------------|-------------|
| ImgCast.cst | length 4139 vs 3213 | 0x0004:86->24, 0x0005:0C->10, 0x003C:0F->11, 0x0050:86->24, 0x0051:0C->10 |
| Text_12chars.cst | length 7296 vs 3213 | 0x0004:86->78, 0x0005:0C->1C, 0x0018:2C->7A, 0x0019:00->16, 0x002C:70->2A |
| Text_Hallo2.cst | length 6664 vs 3213 | 0x0004:86->00, 0x0005:0C->1A, 0x0018:2C->90, 0x0019:00->17 |
| Text_Hallo_2line_linespace_16.cst | length 7478 vs 3213 | 0x0004:86->F0, 0x0005:0C->1B, 0x0018:2C->B2, 0x0019:00->15, 0x002C:70->2A |
| Text_Hallo_2line_linespace_30.cst | length 7654 vs 3213 | 0x0004:86->DE, 0x0005:0C->1D, 0x0018:2C->DE, 0x0019:00->05, 0x002C:70->2A |
| Text_Hallo_2line_linespace_Default.cst | length 7478 vs 3213 | 0x0004:86->F0, 0x0005:0C->1B, 0x0018:2C->80, 0x0019:00->19, 0x002C:70->2A |
| Text_Hallo_NoBold.cst | length 6704 vs 3213 | 0x0004:86->28, 0x0005:0C->1A, 0x0018:2C->B8, 0x0019:00->17 |
| Text_Hallo_editable_true.cst | length 7518 vs 3213 | 0x0004:86->56, 0x0005:0C->1D, 0x0018:2C->E6, 0x0019:00->1A, 0x002C:70->2A |
| Text_Hallo_font_Vivaldi.cst | length 6886 vs 3213 | 0x0004:86->DE, 0x0005:0C->1A, 0x0018:2C->6E, 0x0019:00->18 |
| Text_Hallo_fontsize14.cst | length 7336 vs 3213 | 0x0004:86->58, 0x0005:0C->1C, 0x0018:2C->E8, 0x0019:00->19, 0x002C:70->2A |
| Text_Hallo_italic.cst | length 7336 vs 3213 | 0x0004:86->A0, 0x0005:0C->1C, 0x0018:2C->30, 0x0019:00->1A, 0x002C:70->2A |
| Text_Hallo_letterSpace_6.cst | length 6718 vs 3213 | 0x0004:86->36, 0x0005:0C->1A, 0x0018:2C->C6, 0x0019:00->17 |
| Text_Hallo_multiLine.cst | length 6846 vs 3213 | 0x0004:86->B6, 0x0005:0C->1A, 0x0018:2C->46, 0x0019:00->18 |
| Text_Hallo_tab_true.cst | length 7518 vs 3213 | 0x0004:86->56, 0x0005:0C->1D, 0x0018:2C->46, 0x0019:00->10, 0x002C:70->2A |
| Text_Hallo_textAlignLeft.cst | length 7478 vs 3213 | 0x0004:86->2E, 0x0005:0C->1D, 0x0018:2C->BE, 0x0019:00->1A, 0x002C:70->2A |
| Text_Hallo_textAlignRight.cst | length 7478 vs 3213 | 0x0004:86->2E, 0x0005:0C->1D, 0x0018:2C->D0, 0x0019:00->15, 0x002C:70->2A |
| Text_Hallo_underline.cst | length 7336 vs 3213 | 0x0004:86->A0, 0x0005:0C->1C, 0x0018:2C->9C, 0x0019:00->16, 0x002C:70->2A |
| Text_Hallo_wrap_off.cst | length 7518 vs 3213 | 0x0004:86->64, 0x0005:0C->1C, 0x0018:2C->F4, 0x0019:00->19, 0x002C:70->2A |

## Extracted text and font (heuristic)

| File | Text Runs | Last Font |
|------|-----------|-----------|
| Text_Hallo.cst | Hallo | Arcade * |
| Text_Hallo2.cst | Hallo | Arcade * |
| Text_Hallo_NoBold.cst | Hallo \| Hallo | Arcade * |
| Text_Hallo_italic.cst | Hallo \| Hallo | Arcade * |
| Text_Hallo_underline.cst | Hallo \| Hallo | Arcade * |
| Text_Hallo_font_Vivaldi.cst | Hallo \| Hallo | Vivaldi |
| Text_Hallo_fontsize14.cst | Hallo \| Hallo | Arcade * |
| Text_Hallo_letterSpace_6.cst | Hallo \| Hallo | Arcade * |
| Text_Hallo_multiLine.cst | Hallo \| Hallo\rmulti line\ris longer\rYES! | Arcade * |
| Text_Hallo_editable_true.cst | Hallo \| Hallo | Vivaldi |
| Text_Hallo_tab_true.cst | Hallo | Vivaldi |
| Text_Hallo_textAlignLeft.cst | Hallo \| Hallo\rmulti line\ris longer\rYES! | Arcade * |
| Text_Hallo_textAlignRight.cst | Hallo \| Hallo | Arcade * |
| Text_Hallo_wrap_off.cst | Hallo \| Hallo | Vivaldi |
# CST Text Blocks

| File | Marker | Text | Before(hex) | After(hex) |
|------|--------|------|------------|-----------|
| Text_Hallo.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo2.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_2line_linespace_Default.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_NoBold.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_NoBold.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_editable_true.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_editable_true.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_font_Vivaldi.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_font_Vivaldi.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_fontsize14.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_fontsize14.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_italic.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_italic.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_letterSpace_6.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_letterSpace_6.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_multiLine.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_multiLine.cst | 1F, | Hallomulti lineis longerYES! | 3030303030303030 | 30303034303030303030304130303030 |
| Text_Hallo_tab_true.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_textAlignLeft.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_textAlignLeft.cst | 1F, | Hallomulti lineis longerYES! | 3030303030303030 | 30303034303030303030304130303030 |
| Text_Hallo_textAlignRight.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_textAlignRight.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_underline.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_underline.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_wrap_off.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |
| Text_Hallo_wrap_off.cst | 5, | Hallo | 3030303030303030 | 30303034303030303030303930303030 |

# Alignment byte comparison

The byte at offset `0x0019` stores the text alignment mode. The table below lists the raw value for each alignment variant compared with the default center aligned `Text_Hallo.cst`.

| Offset | Center | Left | Right |
|-------:|------:|-----:|------:|
| 0x0018 | 0x2C | 0xBE | 0xD0 |
| **0x0019** | **0x00** | **0x1A** | **0x15** |
| 0x002C | 0x70 | 0x2A | 0x2A |
| 0x002D | 0x61 | 0x59 | 0x59 |
| 0x002E | 0x6D | 0x45 | 0x45 |
| 0x002F | 0x6D | 0x4B | 0x4B |

# Differences: Text_Hallo_fontsize14.cst vs Text_Hallo.cst

The table below lists the first few bytes that differ when comparing `Text_Hallo_fontsize14.cst` against `Text_Hallo.cst`. The offset is shown in hexadecimal.

| Offset | Base value | Font Size 14 value |
|-------:|-----------:|-------------------:|
| 0x0004 | 0x86 | 0x58 |
| 0x0005 | 0x0C | 0x1C |
| 0x0018 | 0x2C | 0xE8 |
| 0x0019 | 0x00 | 0x19 |
| 0x002C | 0x70 | 0x2A |
| 0x002D | 0x61 | 0x59 |
| 0x002E | 0x6D | 0x45 |
| 0x002F | 0x6D | 0x4B |

# Differences: Text_Hallo_letterSpace_6.cst vs Text_Hallo.cst

The table below lists the first few bytes that differ when comparing `Text_Hallo_letterSpace_6.cst` against `Text_Hallo.cst`. The offset is shown in hexadecimal.

| Offset | Base value | LetterSpace value |
|-------:|-----------:|------------------:|
| 0x0004 | 0x86 | 0x36 |
| 0x0005 | 0x0C | 0x1A |
| 0x0018 | 0x2C | 0xC6 |
| 0x0019 | 0x00 | 0x17 |

# Differences: Text_Hallo_tab_true.cst vs Text_Hallo.cst

The table below lists the first few bytes that differ when comparing `Text_Hallo_tab_true.cst` against `Text_Hallo.cst`. The offset is shown in hexadecimal.

| Offset | Base value | Tab True value |
|-------:|-----------:|---------------:|
| 0x0004 | 0x86 | 0x56 |
| 0x0005 | 0x0C | 0x1D |
| 0x0018 | 0x2C | 0x46 |
| 0x0019 | 0x00 | 0x10 |
| 0x002C | 0x70 | 0x2A |
| 0x002D | 0x61 | 0x59 |
| 0x002E | 0x6D | 0x45 |
| 0x002F | 0x6D | 0x4B |

The byte at offset `0x0019` changes from `0x00` in the base file to `0x10` when tabs are enabled. This sets bit 4 (value `0x10`).

# Differences: Text_Hallo_NoBold.cst vs Text_Hallo.cst

The table below lists the first few bytes that differ when comparing `Text_Hallo_NoBold.cst` against `Text_Hallo.cst`.

| Offset | Base value | NoBold value |
|-------:|-----------:|-------------:|
| 0x0004 | 0x86 | 0x28 |
| 0x0005 | 0x0C | 0x1A |
| 0x0018 | 0x2C | 0xB8 |
| 0x0019 | 0x00 | 0x17 |

# Differences: Text_Hallo_italic.cst vs Text_Hallo.cst

The table below lists the first few bytes that differ when comparing `Text_Hallo_italic.cst` against `Text_Hallo.cst`.

| Offset | Base value | Italic value |
|-------:|-----------:|-------------:|
| 0x0004 | 0x86 | 0xA0 |
| 0x0005 | 0x0C | 0x1C |
| 0x0018 | 0x2C | 0x30 |
| 0x0019 | 0x00 | 0x1A |
| 0x002C | 0x70 | 0x2A |
| 0x002D | 0x61 | 0x59 |
| 0x002E | 0x6D | 0x45 |
| 0x002F | 0x6D | 0x4B |

# Differences: Text_Hallo_underline.cst vs Text_Hallo.cst

The table below lists the first few bytes that differ when comparing `Text_Hallo_underline.cst` against `Text_Hallo.cst`.

| Offset | Base value | Underline value |
|-------:|-----------:|----------------:|
| 0x0004 | 0x86 | 0xA0 |
| 0x0005 | 0x0C | 0x1C |
| 0x0018 | 0x2C | 0x9C |
| 0x0019 | 0x00 | 0x16 |
| 0x002C | 0x70 | 0x2A |
| 0x002D | 0x61 | 0x59 |
| 0x002E | 0x6D | 0x45 |
| 0x002F | 0x6D | 0x4B |

# Differences: Text_Hallo_wrap_off.cst vs Text_Hallo.cst

The table below lists the first few bytes that differ when comparing `Text_Hallo_wrap_off.cst` against `Text_Hallo.cst`.

| Offset | Base value | Wrap Off value |
|-------:|-----------:|---------------:|
| 0x0004 | 0x86 | 0x64 |
| 0x0005 | 0x0C | 0x1C |
| 0x0018 | 0x2C | 0xF4 |
| 0x0019 | 0x00 | 0x19 |
| 0x002C | 0x70 | 0x2A |
| 0x002D | 0x61 | 0x59 |
| 0x002E | 0x6D | 0x45 |
| 0x002F | 0x6D | 0x4B |

## Field CST Byte Differences

Compared with `Field_Hallo.cst`. Showing first 5 differing bytes.

| File | Length diff | First diffs |
|------|-------------|-------------|
| Field_Hallo2.cst | same | 0x0004:BE->4C, 0x0005:14->0E, 0x0018:4E->F0, 0x0019:12->06, 0x004C:08->0D |
| Field_Hallo_3lines.cst | same | 0x0004:BE->4C, 0x0005:14->0E, 0x0018:4E->DC, 0x0019:12->0B, 0x004C:08->09 |
| Field_Hallo_align_center.cst | same | 0x0004:BE->A4, 0x0005:14->0B, 0x0018:4E->34, 0x0019:12->09, 0x004C:08->04 |
| Field_Hallo_align_right.cst | same | 0x0004:BE->A4, 0x0005:14->0B, 0x0018:4E->BC, 0x0019:12->06, 0x004C:08->04 |
| Field_Hallo_bold.cst | same | |
| Field_Hallo_font_candera.cst | same | |
| Field_Hallo_fontsize_24.cst | same | 0x0004:BE->A4, 0x0005:14->0B, 0x0018:4E->BC, 0x0019:12->06, 0x004C:08->18 |
| Field_Hallo_italic.cst | same | 0x0018:4E->BC, 0x0019:12->06, 0x0378:6B->2A, 0x0379:6E->59, 0x037A:75->45 |
| Field_Hallo_underline.cst | same | 0x0004:BE->A4, 0x0005:14->0B, 0x0018:4E->34, 0x0019:12->09, 0x004C:08->18 |

## Text vs Field Offsets

The table below compares some bytes from the baseline `Text_Hallo.cst` and `Field_Hallo.cst` files. Offsets are in hexadecimal. Values highlight where common properties appear to be stored.

| Offset | Text_Hallo | Field_Hallo | Notes |
|------:|-----------:|------------:|------|
| 0x0004 | 0x86 | 0xBE | header byte |
| 0x0005 | 0x0C | 0x14 | header byte |
| 0x0018 | 0x2C | 0x4E | style byte (bold/italic/underline) |
| 0x0019 | 0x00 | 0x12 | flags byte (alignment, wrap) |
| 0x0040 | 0x09 | 0x11 | font size value |
| 0x004C | 0x58 46 49 52 | 0x08 00 00 00 | second header or pointer |
| 0x0983 | Arcade * | Arcade * | font name string |

The alignment and style differences for left/right variants modify the bytes at offsets `0x18` and `0x19` in both formats, indicating these properties are stored in the same location.

## Suspected text length byte

Field variants hint that the byte at offset `0x004C` may hold a text length value. The table below lists this byte for a few sample files.

| File | Value at 0x004C |
|------|---------------|
| Field_Hallo.cst | 0x08 |
| Field_Hallo2.cst | 0x0D |
| Field_Hallo_3lines.cst | 0x09 |
| Text_Hallo.cst | `XFIR` |
| Text_Hallo2.cst | `XFIR` |
