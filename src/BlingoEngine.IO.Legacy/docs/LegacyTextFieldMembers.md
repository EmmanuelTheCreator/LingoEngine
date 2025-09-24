# Legacy Text and Field Members

[_Back to documentation overview_](README.md)

These notes consolidate the byte layouts used by Director text and field cast members prior to the
latest MX projectors. The information mirrors the behaviour implemented in
`BlLegacyTextReader` and `BlLegacyFieldReader`: both readers surface raw `STXT` and `XMED`
payloads together with their detected container formats, leaving the higher layers free to interpret
fonts, style runs, and field flags. Tables below record every header byte consumed by historic
projector versions so that older resources remain compatible and the modern MX `XMED` layout can be
decoded without cross-referencing additional files.

## Resource Overview

- Director 2 and 3 export plain `STXT` streams. The cast-member header carries alignment, border,
  colour, and rectangle metadata while the companion `STXT` chunk holds the text itself.
- Director 4 through Director 10 extend the header with scrolling and maximum-height fields, but the
  text bytes continue to live in `STXT` chunks unless the movie opts into the styled `XMED` format.
- Styled text (`XMED`) keeps the same cast-member header and stores per-character formatting in a
  sidecar chunk. Modern offsets and parsing notes appear in the "Director 11+ Styled Text" section
  below.
- Bit 0 of the flag word/byte marks editable fields. Bit 1 enables auto-tab behaviour and bit 2
  disables word wrapping. Later records keep these semantics so fields can be identified after the
  raw bytes are loaded.

## Director 2 Layout (version &lt; 0x300)

| Hex Bytes | Length | Description |
| --- | --- | --- |
| `<border size>` | 1 byte | Frame thickness around the text box. |
| `<gutter size>` | 1 byte | Interior padding between the border and the text content. |
| `<box shadow>` | 1 byte | Drop-shadow distance recorded for legacy renderers. |
| `<text type>` | 1 byte | Cast subtype such as static text, scrolling text, or editable field. |
| `<text alignment>` | 2 bytes | Signed QuickDraw alignment word. |
| `<background red>` | 2 bytes | High-precision red component of the background colour. |
| `<background green>` | 2 bytes | Green component of the background colour. |
| `<background blue>` | 2 bytes | Blue component of the background colour. |
| `<pad2>` | 2 bytes | Reserved word, usually zero. Non-zero values indicate unrecognised data. |
| `<rect.top>`..`<rect.right>` | 8 bytes | Four signed 16-bit values describing the initial rectangle. |
| `<pad3>` | 2 bytes | Additional reserved word maintained for compatibility. |
| `<text shadow>` | 1 byte | Legacy drop-shadow depth applied to the glyphs themselves. |
| `<text flags>` | 1 byte | Low nibble stores field/text flags; high bits are left unused. |
| `<total text height>` | 2 bytes | Authored text height used when sizing widgets. |

Director 2 exports log a warning when the reserved words carry non-zero data or when the high five
bits of the flag byte are set, because only the low nibble is understood by older engines.

## Director 3 Layout (0x300 ≤ version &lt; 0x400)

| Hex Bytes | Length | Description |
| --- | --- | --- |
| `<border size>` | 1 byte | Same border thickness stored by Director 2. |
| `<gutter size>` | 1 byte | Interior padding value retained from earlier exports. |
| `<box shadow>` | 1 byte | Drop-shadow distance for the text box. |
| `<text type>` | 1 byte | Cast subtype selector identical to the Director 2 layout. |
| `<text alignment>` | 2 bytes | Signed alignment word read before colour data. |
| `<background red>` | 2 bytes | Red component of the background colour. |
| `<background green>` | 2 bytes | Green component of the background colour. |
| `<background blue>` | 2 bytes | Blue component of the background colour. |
| `<pad2>` | 2 bytes | Reserved word that remains in the stream. |
| `<rect.top>`..`<rect.right>` | 8 bytes | Four signed 16-bit values for the initial rectangle. |
| `<pad3>` | 2 bytes | Additional reserved word preserved for parity with Director 2. |
| `<text flags>` | 2 bytes | Flag word: bit 0 marks editable fields, bit 1 enables auto-tab, bit 2 disables wrapping. |
| `<total text height>` | 2 bytes | Authored height value retained by the editor. |

## Director 4–10 Layout (0x400 ≤ version &lt; 0x1100)

| Hex Bytes | Length | Description |
| --- | --- | --- |
| `<border size>` | 1 byte | Border thickness identical to earlier versions. |
| `<gutter size>` | 1 byte | Padding between the border and the text. |
| `<box shadow>` | 1 byte | Drop-shadow offset for the bounding box. |
| `<text type>` | 1 byte | Member subtype (static text, scrolling text, button text, etc.). |
| `<text alignment>` | 2 bytes | Signed alignment word (−1 for right alignment). |
| `<background red>` | 2 bytes | Red component of the background colour. |
| `<background green>` | 2 bytes | Green component of the background colour. |
| `<background blue>` | 2 bytes | Blue component of the background colour. |
| `<scroll offset>` | 2 bytes | Initial scroll position stored for scrolling text. |
| `<rect.top>`..`<rect.right>` | 8 bytes | Initial rectangle describing the cast-member bounds. |
| `<max height>` | 2 bytes | Maximum height used when wrapping multi-line text. |
| `<text shadow>` | 1 byte | Per-glyph drop-shadow depth reintroduced in Director 4. |
| `<text flags>` | 1 byte | Flag byte: bit 0 toggles editable fields, bit 1 enables auto-tab, bit 2 disables wrapping. |
| `<text height>` | 2 bytes | Preferred text height for layout calculations. |

## Button Text Extension

When these bytes are parsed for button cast members, an extra big-endian 16-bit value follows the
standard header. The value identifies the button type, counting from 1 in the authoring tool. The
runtime subtracts one so the type can be used as a zero-based enum.

## Locating STXT and XMED Resources

The header bytes above never include the author-facing text. That data lives in a companion resource:
older projectors reuse the cast-member ID to point at an `STXT` chunk, while Director 4 and later
keep an explicit child link. Styled movies replace or augment the `STXT` child with an `XMED`
resource that tracks font runs, colour tables, and letter spacing. Any payload read from `STXT` or
`XMED` is passed through unchanged by the legacy readers so experiments can continue even though the
latest MX-era format is still under investigation.

## Director 11+ Styled Text (`XMED`) Layout

Modern Director releases store per-character formatting inside an `XMED` stream. The chunk begins
with an ASCII directory that announces the offsets and capacities for each data block before listing
the text runs and style descriptors.

TODO


