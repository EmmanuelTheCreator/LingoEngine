# XMED File Comparisons

This file lists the byte differences between sample casts. For a summary of known offsets see [XMED_Offsets.md](XMED_Offsets.md).

# Text CST Byte Differences

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
| Text_Hallo_changed_color.cst | length 6730 vs 3213 | 0x0004:86->42, 0x0005:0C->1A, 0x0018:2C->D2, 0x0019:00->17 |
| Text_Hallo_text_transform_all_on.cst | length 7346 vs 3213 | 0x0004:86->AA, 0x0005:0C->1C, 0x0018:2C->3A, 0x0019:00->1A, 0x002C:70->2A |
| Text_Hallo_margin_spacing_FirstInd.cst | length 6682 vs 3213 | 0x0004:86->12, 0x0005:0C->1A, 0x0018:2C->A2, 0x0019:00->17 |
| Text_Hallo_multifont.cst | length 7318 vs 3213 | 0x0004:86->8E, 0x0005:0C->1C, 0x0018:2C->1E, 0x0019:00->1A, 0x002C:70->2A |
| Text_Hallo_strikeout.cst | length 6714 vs 3213 | 0x0004:86->32, 0x0005:0C->1A, 0x0018:2C->C2, 0x0019:00->17 |
| Text_Hallo_subscript.cst | length 7352 vs 3213 | 0x0004:86->B0, 0x0005:0C->1C, 0x0018:2C->40, 0x0019:00->1A, 0x002C:70->2A |
| Text_Hallo_superscript.cst | length 6776 vs 3213 | 0x0004:86->70, 0x0005:0C->1A, 0x0018:2C->00, 0x0019:00->18 |
| Text_Hallo_with_name.cst | length 4982 vs 3213 | 0x0004:86->6E, 0x0005:0C->13, 0x0018:2C->FE, 0x0019:00->10 |
| Text_Multi_Line_Multi_Style.cst | length 11165 vs 3213 | 0x0004:86->96, 0x0005:0C->2B, 0x0018:2C->5E, 0x0019:00->07, 0x002C:70->2A |
| Text_Single_Line_Multi_Style.cst | length 10904 vs 3213 | 0x0004:86->90, 0x0005:0C->2A, 0x0018:2C->6A, 0x0019:00->07, 0x002C:70->2A |
| Text_Wider_Width4.cst | length 6642 vs 3213 | 0x0004:86->EA, 0x0005:0C->19, 0x0018:2C->7A, 0x0019:00->17 |

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
| Text_Hallo_changed_color.cst | Hallo | Arcade * |
| Text_Hallo_text_transform_all_on.cst | Hallo | Arcade * |
| Text_Hallo_margin_spacing_FirstInd.cst | Hallo | Arcade * |
| Text_Hallo_multifont.cst | Hallo | Arcade * |
| Text_Hallo_strikeout.cst | Hallo | Arcade * |
| Text_Hallo_subscript.cst | Hallo | Arcade * |
| Text_Hallo_superscript.cst | Hallo | Arcade * |
| Text_Hallo_with_name.cst | Hallo | Arcade * |
| Text_Multi_Line_Multi_Style.cst | This text is red, Arial,12px, centered\rThe text is yellow, Tahoma, 9px, left aligned, bold, italic, underline\rThe text is green, font Terminal, 18px, left aligned, with spacing of 39\rThe text is orange, Tahoma, 9px, left aligned, bold, italic, underline\rThis text is red, Arial,12px, centered again | Arial |
| Text_Single_Line_Multi_Style.cst | This text is red, Arial,12px, The text is yellow, Tahoma, 9px, bold, italic, underline The text is green, font Terminal, 18px, with spacing of 39 The text is orange, Tahoma, 9px, bold, italic, underline This text is red, Arial,12px, again | Arial |
| Text_Wider_Width4.cst | Hallo | Arcade * |

### Multi-font run example

`Text_Hallo_multifont.cst` lists fonts after the text. The sequence shows
"Arcade *" for the first run, then "Trajan Pro" for the middle letters, and
finally "Arcade *" again.

# Differences: Text_Hallo_subscript.cst vs Text_Hallo.cst

The table below lists the first few bytes that differ when comparing `Text_Hallo_subscript.cst` against `Text_Hallo.cst`.

| Offset | Base value | Subscript value |
|-------:|-----------:|----------------:|
| 0x0004 | 0x86 | 0xB0 |
| 0x0005 | 0x0C | 0x1C |
| 0x0018 | 0x2C | 0x40 |
| 0x0019 | 0x00 | 0x1A |
| 0x002C | 0x70 | 0x2A |
| 0x002D | 0x61 | 0x59 |
| 0x002E | 0x6D | 0x45 |
| 0x002F | 0x6D | 0x4B |

# Differences: Text_Hallo_superscript.cst vs Text_Hallo.cst

The table below lists the first few bytes that differ when comparing `Text_Hallo_superscript.cst` against `Text_Hallo.cst`.

| Offset | Base value | Superscript value |
|-------:|-----------:|------------------:|
| 0x0004 | 0x86 | 0x70 |
| 0x0005 | 0x0C | 0x1A |
| 0x0018 | 0x2C | 0x00 |
| 0x0019 | 0x00 | 0x18 |

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


## Member names

`Text_Hallo_with_name.cst` contains the string `MyText` at offset `0x0EF7`. Field
variants such as `Field_Hallo2.cst` embed `My field` at similar offsets. The base
files do not include these names.

### Member name offsets

| File | Offset | Name |
|------|------:|------|
| Text_Hallo_with_name.cst | 0x0EF7 | MyText |
| 0x0018 | — | Width value (twips) | `2C 00 00 00` |
| 0x001C | +0x04 | Style flags (bold/italic/underline) | `0x2C` |
| 0x001D | +0x01 | Alignment/tab/wrap flags | `0x00` |
| 0x003C | +0x1F | Line spacing value | `0F 00 00 00` |
| 0x04DE | +0x04 | Right margin value | `00 00 00 3F` |
| 0x04E2 | +0x04 | First indent value | `00 00 00 3F` |
## Color change

`Text_Hallo_changed_color.cst` uses foreground color `CCFF66` rather than the
`FF0000` value seen in the other text casts. The first changed long at offset
`0x0018` (`0x000017D2`) appears to encode this color.

Subscript uses style byte `0x40` while superscript uses `0x00`. Their flag bytes at
offset `0x0019` are `0x1A` and `0x18` respectively.

### Color table bytes

The ASCII sequence `FFFF0000 000600040001` appears in both casts. In the color
variant a second sequence occurs later in the file.

| File | Offset | Sequence |
|------|------:|---------|
| Text_Hallo.cst | 0x0622 | `FFFF0000000600040001` |
| Text_Hallo_changed_color.cst | 0x1100 | `FFFF0000000600040001` |

The color cast also shows the pattern `01CC00 01FF00 016600` near offset
`0x1354`, which corresponds to the RGB value `CCFF66`.

### Known text byte offsets

The table below lists byte offsets that have been identified in text casts. The
Δ column indicates the distance from the previous known value.

| Offset | Δ from prior | Meaning | Example |
|------:|------------:|---------|---------|
| 0x0018 | — | Style flags (bold/italic/underline) | `0x2C` |
| 0x0019 | +0x01 | Alignment and wrap flags | `0x00` |
| 0x0040 | +0x27 | Font size (32‑bit little endian) | `00 00 00 09` |
| 0x04DA | +0x49A | Left margin value | `00 00 00 3F` |
| 0x0622 | +0x148 | Start of color table | `FFFF0000000600040001` |
| 0x0983 | +0x361 | Font name string | `Arcade *` |
| 0x0CAE | +0x32B | Spacing before paragraph | `0D 00 00 00` |
| 0x0EF7 | +0x249 | Member name string | `MyText` |
| 0x1970 | +0xA79 | Spacing after paragraph | `09 00 00 00` |

### Annotated XMED bytes

Below is an excerpt from the `Text_Hallo_changed_color.cst` XMED chunk. Each
known section is placed on a new line followed by a short description.

```
00000622: 46 46 46 46 30 30 30 30 30 30 30 36 30 30 30 34 30 30 30 31
          FFFF0000 000600040001                       ; color table header
000006F8: 30 00
          -- unknown bytes
000006FA: 35 2C 48 61 6C 6C 6F 03
          ; text run "Hallo" (length prefix 5)
000008C8: 30 30 30 30 30 30 30 32 00
          -- padding / offsets
000008C9: 34 30 2C 05 41 72 69 61 6C 00
          ; font entry "Arial"
00000978: 02 31 30 31 01 30 00
          -- run terminator
0000097F: 34 30 2C 08 41 72 63 61 64 65 20 2A 00
          ; font entry "Arcade *"
```

### Text margin and spacing values

`Text_Hallo_margin_spacing_FirstInd.cst` encodes the following numbers:

| Left Margin | Right Margin | First Indent | Spacing Before | Spacing After |
|------------:|-------------:|-------------:|---------------:|--------------:|
| 0.5 | 0.6 | 0.3 | 13 | 9 |

Example byte locations:

```
00000CAE: 0D 00 00 00 04 00 00 00
          ; spacing before = 13
00001970: 00 00 04 00 00 00 09 00
          ; spacing after = 9
000004DA: 00 00 00 3F
          ; left margin = 0.5
```


### Annotated XMED bytes (multi-line & multifont)

The snippet below comes from `Text_Hallo_multifont.cst`. It shows the text run followed by the two font table entries used in that cast.

numeric fields as **StyleId**, **Unknown** (F2), **TextLength**, **F4** and
**BaseStyleId** (F5).

| Map offset | StyleId | F2 | TextLength | F4 | BaseStyleId | Descriptor offset | Δ prev desc | Notes |
|-----------:|--------:|---:|-----------:|---------:|------------:|------------------:|-------------:|-------|
Here the StyleId column increments with each row, perhaps referencing the
default style chain. The final field (BaseStyleId) points to the descriptor
blocks at offsets `0x16A8` and later. The zero values in the F4 column mean no
intermediate style was used for these lines.
| Offset | StyleId | F2 | TextLength | F4 | BaseStyleId | Notes |
|------:|--------:|---:|-----------:|---------:|------------:|------|

StyleId climbs from `0004` upward, implying a simple line index. BaseStyleId
matches the descriptor IDs from the table below. F4 is zero for all rows, so no
chained inheritance appears in this sample. When a descriptor is applied its
font and alignment override the default style.
| Offset | StyleId | F2 | TextLength | F4 | BaseStyleId | Notes |
|------:|-------:|---:|-----------:|---:|-----------:|------|
| Offset | StyleId | F2 | TextLength | F4 | BaseStyleId | Notes |
|------:|-------:|---:|-----------:|---:|-----------:|------|
00002687: 00080000036600000005
00002A01: 00090000001500000002
```

The single-line variant stores a comparable list near the end of the file:

```
The 20‑digit strings split into five four‑digit fields. Field three holds the
line length while field five clearly matches the style descriptor ID used for
that line. Field one also resembles a style index and may point to a parent
style for inheritance. To explore this possibility the table below labels the
numeric fields as **StyleId1**, **Unknown** (F2), **Length**, **StyleId2** (F4)
and **StyleId3** (F5).

| Map offset | StyleId1 | F2 | Length | StyleId2 | StyleId3 | Descriptor offset | Δ prev desc | Notes |
|-----------:|--------:|---:|-------:|---------:|---------:|------------------:|-------------:|-------|
| 0x1334 | 0004 | 0000 | 0029 | 0000 | 0008 | 0x16A8 | — | first/last line |
| 0x1371 | 0005 | 0000 | 001F | 0000 | 0006 | 0x18C4 | 0x021C | yellow line |
| 0x13A4 | 0006 | 0000 | 0273 | 0000 | 000B | 0x196E | 0x00AA | green line |
| 0x162B | 0007 | 0000 | 0070 | 0000 | 0003 | 0x1A30 | 0x00C2 | orange line |
| 0x16AF | 0008 | 0000 | 0366 | 0000 | 0005 | 0x26A8 | 0x0C78 | duplicate of 0008 |
| 0x1A29 | 0009 | 0000 | 0015 | 0000 | 0002 | — | — | terminator |

Here StyleId1 increments with each row, perhaps referencing the default style
chain. StyleId3 corresponds to the descriptor blocks at offsets `0x16A8` and
later. The zero values in the StyleId2 column mean no intermediate style was
used for these lines.
| Offset | StyleId1 | F2 | Length | StyleId2 | StyleId3 | Notes |
|------:|--------:|---:|-------:|---------:|---------:|------|

StyleId1 climbs from `0004` upward, implying a simple line index. StyleId3
matches the descriptor IDs from the table below. StyleId2 is zero for all rows,
so no chained inheritance appears in this sample. When a descriptor is applied
its font and alignment override the default style.
| Style ID | Descriptor offset | Δ prev desc | Font | Notes |
|--------:|------------------:|------------:|------|------|
| 0008 | 0x16A8 | — | Arial,12px | style used for first and last lines |
| 0006 | 0x18C4 | 0x021C | Tahoma,9px | yellow line style |
| 000B | 0x196E | 0x00AA | Terminal,18px | green line style |
| 0003 | 0x1A30 | 0x00C2 | Tahoma,9px | orange line style |
| 0008 | 0x2680 | 0x0C50 | Arial,12px | duplicate of style 0008 |
| 0005 | 0x26A8 | 0x0028 | Arial,12px | duplicate after map table |
| 0006 | 0x289C | 0x01F4 | Tahoma,9px | duplicate yellow line |
| 000B | 0x2946 | 0x00AA | Terminal,18px | duplicate green line |

### Color table values

The multi-style file stores a short color table near the start of the XMED data.
Each color entry begins with the byte `0x01` followed by four ASCII hex digits.
This means a single entry uses **five bytes** total rather than the six or
eight bytes normally seen in 24‑ or 32‑bit colour formats.

| Index | Offset | Color value |
|-----:|------:|------------|
| 1 | 0x0B6A | FFFF |
| 2 | 0x1128 | 77AA |
| 3 | 0x1189 | FF00 |
| 4 | 0x11D9 | 8C00 |
| 5 | 0x1601 | 9900 |
| 6 | 0x1A08 | 60FF |

Style `0008` references color index `0005`, pointing to the final entry above.
Other style descriptors use indices `0006` and `0008`, suggesting the table
can hold additional colours beyond the six shown here.

### XMED layout overview

An XMED text member begins with a short header and a default style block.
Immediately after the header the color table appears around offset `0x1100`.
The visible text follows twice: once near `0x120A` and again near `0x21E1`.
Between these copies sits a table of style map entries and the per-style
descriptor blocks containing font and alignment data.  The descriptors end with
a small index that points back to one of the colors in the table above.

Comparing 200 bytes from the two text locations shows the first 125 bytes are
identical, confirming the entire string is duplicated with different numeric
prefixes.

### Multi-style entry table

The 20-digit strings split into five four-digit fields.  Field five
matches the style record's ID while field three appears to reflect the
length of the associated text line.

| Offset | F1 | F2 | F3 | F4 | F5 | Notes |
|------:|----:|----:|----:|----:|----:|------|
Single-style casts such as `Text_Hallo.cst` use the same descriptor layout. They
contain only one style entry, matching ID `0008` from the table above, which
defines an Arial 12&nbsp;px font with centered alignment. This shows that the
multi-style table structure also applies to the simpler Hallo files.

| 0x1334 | 0004 | 0000 | 0029 | 0000 | 0008 | line 1: red, Arial, centered |
| 0x1371 | 0005 | 0000 | 001F | 0000 | 0006 | line 2: yellow, Tahoma |
| 0x13A4 | 0006 | 0000 | 0273 | 0000 | 000B | line 3: green, Terminal |
| 0x162B | 0007 | 0000 | 0070 | 0000 | 0003 | line 4: orange, Tahoma |
| 0x16AF | 0008 | 0000 | 0366 | 0000 | 0005 | line 5: red again |
| 0x1A29 | 0009 | 0000 | 0015 | 0000 | 0002 | table terminator |

Later in the file the same entries repeat starting at offset `0x230C`.

The single line variant holds a similar table:

| Offset | F1 | F2 | F3 | F4 | F5 | Notes |
|------:|----:|----:|----:|----:|----:|------|
| 0x22CB | 0004 | 0000 | 001E | 0000 | 0006 | first style |
| 0x22FD | 0005 | 0000 | 000A | 0000 | 0002 | second style |
| 0x231B | 0006 | 0000 | 01FF | 0000 | 0009 | third style |
| 0x252E | 0007 | 0000 | 0048 | 0000 | 0002 | fourth style |
| 0x258A | 0008 | 0000 | 0366 | 0000 | 0005 | fifth style |
| 0x2904 | 0009 | 0000 | 0013 | 0000 | 0002 | terminator |
### Style ID to font mapping

The table above lists the style indices for each line of the multi-style samples. Each index also appears near a style descriptor block that embeds the font name and size. A few offsets extracted from `Text_Multi_Line_Multi_Style.cst` illustrate this relationship:

| Style ID | Descriptor offset | Font | Notes |
|--------:|------------------:|------|------|
| 0008 | 0x16A8 | Arial,12px | style used for first and last lines |
| 0006 | 0x18C4 | Tahoma,9px | yellow line style |
| 000B | 0x196E | Terminal,18px | green line style |
| 0003 | 0x1A30 | Tahoma,9px | orange line style |
| 0005 | 0x26A8 | Arial,12px | duplicate of style 0008 |

### Per-style alignment and color bytes

Each descriptor begins with a short binary header. The first two bytes mirror
the style/flag pair used in single-style casts (offsets `0x0018`–`0x0019`).
Immediately after this header the style ID appears as four ASCII digits followed
by extra numbers. One of these numbers matches a color entry from the table that
starts near `0x1110`.

Example offsets from `Text_Multi_Line_Multi_Style.cst`:

```
0x16A8: 30 82 C1 03 ... "0008" ... "40," 'Arial'    ; centered red line
0x18C4: 02 30 82 02 ... "0006" ... "40," 'Tahoma'   ; yellow left aligned
0x196E: 30 30 30 38 ... "000B" ... "40," 'Terminal' ; green left aligned
```

The color table itself stores RGB hex pairs prefixed by `0x01`.  A fragment from
the changed-color sample at `0x1354` reads:

```
00001354: 01 CC 00 01 FF 00 01 66 00 01 30 01 46 46 46 46
```

The final digits in each style block select one of these color entries.


### Width byte comparison

`Text_Wider_Width4.cst` stores a much larger 32‑bit value at offset `0x0018`
(`7A 17 00 00` little endian) compared with `Text_Hallo.cst` which has
`2C 00 00 00`. Converting from twips, this roughly equals four inches of
field width.
to map style indices to character positions.  In this example the bytes around
`0x1700` reference two styles so that characters 0‑2 use *Arcade ***, the middle
letters switch to *Trajan Pro*, and the last letter reverts back.

```
000006F8: 30 00 35 2C 48 61 6C 6C 6F 03
          ; text run "Hallo" (length prefix 5)
00001724: 34 30 2C 08 41 72 63 61 64 65 20 2A 00
          ; font entry "Arcade *"
000017C8: 31 30 31 01 30 00 34 30 2C 0A 54 72 61 6A 61 6E 20 50 72 6F 00
          ; font entry "Trajan Pro"
```

### Style block offsets

The multi‑font cast stores a style table in ASCII form. Each entry is five
four‑digit numbers that likely encode a character range and the style ID.  Some
examples from `Text_Hallo_multifont.cst`:

```
00000702: 30 30 30 34 30 30 30 30 30 30 30 39 30 30 30 30 30 30 30 32
          ; "00040000000900000002"
00000860: 30 30 30 37 30 30 30 30 30 30 34 38 30 30 30 30 30 30 30 32
          ; "00070000004800000002"
0000145F: 30 30 30 34 30 30 30 30 30 30 31 30 30 30 30 30 30 30 30 34
          ; "00040000001000000004"
```

These values seem to link the fonts in the table to sections of the text
string, identifying where each style begins and ends.

### Known field byte offsets

Known offsets observed in field casts. The Δ column shows how far each value
occurs after the previous one.

| Offset | Δ from prior | Meaning | Example |
|------:|------------:|---------|---------|
| 0x0018 | — | Style byte | `0x4E` |
| 0x0019 | +0x01 | Flags byte | `0x12` |
| 0x0040 | +0x27 | Font size (little endian) | `11 00 00 00` |
| 0x004C | +0x0C | Text length | `08 00 00 00` |
| 0x0983 | +0x4D7 | Font name string | `Arcade *` |
| 0x0EF7 | +0x574 | Member name string | `My field` |
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
