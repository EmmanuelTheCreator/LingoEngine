# XMED File Comparisons (Updated)

This note captures the byte-level differences observed when several sample
`CASt` files are compared against the baseline `Text_Hallo.cst`. The raw tables
from the original investigation are retained, but the surrounding commentary has
been refreshed to reflect what the current parser understands about those bytes.

---

## 1. Header byte deltas (text members)

The table below lists the length delta and the first five differing offsets for
each text sample relative to `Text_Hallo.cst`. Offsets are shown in hexadecimal
and values are little-endian bytes.

| File | Length diff | First diffs |
|------|-------------|-------------|
| ImgCast.cst | length 4139 vs 3213 | 0x0004:86→24, 0x0005:0C→10, 0x003C:0F→11, 0x0050:86→24, 0x0051:0C→10 |
| Text_12chars.cst | length 7296 vs 3213 | 0x0004:86→78, 0x0005:0C→1C, 0x0018:2C→7A, 0x0019:00→16, 0x002C:70→2A |
| Text_Hallo2.cst | length 6664 vs 3213 | 0x0004:86→00, 0x0005:0C→1A, 0x0018:2C→90, 0x0019:00→17 |
| Text_Hallo_2line_linespace_16.cst | length 7478 vs 3213 | 0x0004:86→F0, 0x0005:0C→1B, 0x0018:2C→B2, 0x0019:00→15, 0x002C:70→2A |
| Text_Hallo_2line_linespace_30.cst | length 7654 vs 3213 | 0x0004:86→DE, 0x0005:0C→1D, 0x0018:2C→DE, 0x0019:00→05, 0x002C:70→2A |
| Text_Hallo_2line_linespace_Default.cst | length 7478 vs 3213 | 0x0004:86→F0, 0x0005:0C→1B, 0x0018:2C→80, 0x0019:00→19, 0x002C:70→2A |
| Text_Hallo_NoBold.cst | length 6704 vs 3213 | 0x0004:86→28, 0x0005:0C→1A, 0x0018:2C→B8, 0x0019:00→17 |
| Text_Hallo_editable_true.cst | length 7518 vs 3213 | 0x0004:86→56, 0x0005:0C→1D, 0x0018:2C→E6, 0x0019:00→1A, 0x002C:70→2A |
| Text_Hallo_font_Vivaldi.cst | length 6886 vs 3213 | 0x0004:86→DE, 0x0005:0C→1A, 0x0018:2C→6E, 0x0019:00→18 |
| Text_Hallo_fontsize14.cst | length 7336 vs 3213 | 0x0004:86→58, 0x0005:0C→1C, 0x0018:2C→E8, 0x0019:00→19, 0x002C:70→2A |
| Text_Hallo_italic.cst | length 7336 vs 3213 | 0x0004:86→A0, 0x0005:0C→1C, 0x0018:2C→30, 0x0019:00→1A, 0x002C:70→2A |
| Text_Hallo_letterSpace_6.cst | length 6718 vs 3213 | 0x0004:86→36, 0x0005:0C→1A, 0x0018:2C→C6, 0x0019:00→17 |
| Text_Hallo_multiLine.cst | length 6846 vs 3213 | 0x0004:86→B6, 0x0005:0C→1A, 0x0018:2C→46, 0x0019:00→18 |
| Text_Hallo_tab_true.cst | length 7518 vs 3213 | 0x0004:86→56, 0x0005:0C→1D, 0x0018:2C→46, 0x0019:00→10, 0x002C:70→2A |
| Text_Hallo_textAlignLeft.cst | length 7478 vs 3213 | 0x0004:86→2E, 0x0005:0C→1D, 0x0018:2C→BE, 0x0019:00→1A, 0x002C:70→2A |
| Text_Hallo_textAlignRight.cst | length 7478 vs 3213 | 0x0004:86→2E, 0x0005:0C→1D, 0x0018:2C→D0, 0x0019:00→15, 0x002C:70→2A |
| Text_Hallo_underline.cst | length 7336 vs 3213 | 0x0004:86→A0, 0x0005:0C→1C, 0x0018:2C→9C, 0x0019:00→16, 0x002C:70→2A |
| Text_Hallo_wrap_off.cst | length 7518 vs 3213 | 0x0004:86→64, 0x0005:0C→1C, 0x0018:2C→F4, 0x0019:00→19, 0x002C:70→2A |
| Text_Hallo_changed_color.cst | length 6730 vs 3213 | 0x0004:86→42, 0x0005:0C→1A, 0x0018:2C→D2, 0x0019:00→17 |
| Text_Hallo_text_transform_all_on.cst | length 7346 vs 3213 | 0x0004:86→AA, 0x0005:0C→1C, 0x0018:2C→3A, 0x0019:00→1A, 0x002C:70→2A |
| Text_Hallo_margin_spacing_FirstInd.cst | length 6682 vs 3213 | 0x0004:86→12, 0x0005:0C→1A, 0x0018:2C→A2, 0x0019:00→17 |
| Text_Hallo_multifont.cst | length 7318 vs 3213 | 0x0004:86→8E, 0x0005:0C→1C, 0x0018:2C→1E, 0x0019:00→1A, 0x002C:70→2A |
| Text_Hallo_strikeout.cst | length 6714 vs 3213 | 0x0004:86→32, 0x0005:0C→1A, 0x0018:2C→C2, 0x0019:00→17 |
| Text_Hallo_subscript.cst | length 7352 vs 3213 | 0x0004:86→B0, 0x0005:0C→1C, 0x0018:2C→40, 0x0019:00→1A, 0x002C:70→2A |
| Text_Hallo_superscript.cst | length 6776 vs 3213 | 0x0004:86→70, 0x0005:0C→1A, 0x0018:2C→00, 0x0019:00→18 |
| Text_Hallo_with_name.cst | length 4982 vs 3213 | 0x0004:86→6E, 0x0005:0C→13, 0x0018:2C→FE, 0x0019:00→10 |
| Text_Multi_Line_Multi_Style.cst | length 11165 vs 3213 | 0x0004:86→96, 0x0005:0C→2B, 0x0018:2C→5E, 0x0019:00→07, 0x002C:70→2A |
| Text_Single_Line_Multi_Style.cst | length 10904 vs 3213 | 0x0004:86→90, 0x0005:0C→2A, 0x0018:2C→6A, 0x0019:00→07, 0x002C:70→2A |
| Text_Wider_Width4.cst | length 6642 vs 3213 | 0x0004:86→EA, 0x0005:0C→19, 0x0018:2C→7A, 0x0019:00→17 |

### Interpreting the offsets

The recurring offsets line up with the `CASt` header structure documented in the
`xmedCode.pat` pattern and test hints:

- `0x0018` — field width (twips). Wider samples update this value accordingly.【F:Test/TestData/Legacy/Texts_Fields/xmedCode.pat†L99-L115】
- `0x001C` — default style bits (bold/italic/underline/etc.).【F:Test/TestData/Legacy/Texts_Fields/xmedCode.pat†L52-L69】
- `0x001D` — layout flags (alignment, wrap, tab enable).【F:Test/TestData/Legacy/Texts_Fields/xmedCode.pat†L70-L87】
- `0x003C` / `0x0040` / `0x004C` — line spacing, base font size, and text length.
- `0x04DA` / `0x04DE` / `0x04E2` — margins and first-line indent.

When the table above shows a change at `0x0019`, it corresponds directly to the
alignment/wrap flags: left/right justification, `wrap off`, or tab toggles now
match the parser’s interpretation.

### Text and font runs

The multi-style samples (`Text_Multi_Line_Multi_Style*`) expose the run maps that
`BlXmedTokenParser` now consumes. The font sequence observed during the original
investigation (“Arcade *” → “Trajan Pro” → “Arcade *”) is still visible; the
modern parser resolves it through the `Fonts` block and assigns the resulting
family/style names to each `XmedStyleDescriptor`.【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenStyleParser.cs†L197-L215】

---

## 2. Field cast comparisons

Raw byte differences for field members are preserved below. These files reuse the
same header layout as text members, so the offset interpretations from the
previous section apply here as well.

| Field_Hallo2.cst | same | 0x0004:BE→4C, 0x0005:14→0E, 0x0018:4E→F0, 0x0019:12→06, 0x004C:08→0D |
| Field_Hallo_3lines.cst | same | 0x0004:BE→4C, 0x0005:14→0E, 0x0018:4E→DC, 0x0019:12→0B, 0x004C:08→09 |
| Field_Hallo_align_center.cst | same | 0x0004:BE→A4, 0x0005:14→0B, 0x0018:4E→34, 0x0019:12→09, 0x004C:08→04 |
| Field_Hallo_align_right.cst | same | 0x0004:BE→A4, 0x0005:14→0B, 0x0018:4E→BC, 0x0019:12→06, 0x004C:08→04 |
---
## 3. Colour experiments
Colour-altered casts (`Text_Hallo_changed_color.cst`, `Text_Hallo_text_transform_all_on.cst`) still show the `FFFF0000000600040001`
pattern at the start of the colour table. Later segments (e.g. `01CC00 01FF00
016600`) encode the RGB components that the style parser now lifts into
`XmedStyleDescriptor.ForegroundColor`.【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenStyleParser.cs†L92-L123】
The palette indices logged as `02:5B`, `02:5C`, etc., map to Director’s Fore/Back
colour ids and remain visible in the header group (`03:FFFF…`).
---
## 4. Historical scratchpad (kept for reference)

The original investigation contained several speculative notes. They are kept
here with inline corrections so future work can trace the evolution of the
understanding:
- **Directory rows as `<TYPE,OFFSET,COUNT>`** – offsets are not present. The
  parser derives block boundaries from the ASCII counts instead.
- **`00(40):` font name heuristic** – the pattern correctly spots font entries,
  but the follow-up block is part of the structured descriptor and not free-form
  padding.
- **Style flag bits** – the guessed meanings (bold/italic/underline/strikeout/
  subscript/superscript) align with the parser, but the “editable field” flag is
  stored in the `CASt` header rather than the style descriptor.
- **Control byte glossary (`01`, `02`, `03`, `81`, `82`, `C1`, `C2`, `C3`)** – the
  tokenizer still emits the same values, yet the parser now maps them to concrete
  structures (style records, tab stops, spacing blocks).
These annotations document where the research journey started; the corrected
sections above describe how the engine currently interprets the data.

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
