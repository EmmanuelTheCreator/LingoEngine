# XMED Offsets & Structures (Revisited)

This file consolidates what the current parser understands about modern
`XMED` chunks. Earlier research notes are retained at the end so the investigative
trail is not lost, but the corrected information appears first.

---

## Confirmed ASCII block directory

Every `XMED` stream begins with ASCII records that declare the major blocks. The
`BlXmedTokenizer` reads each `03:…` header into an `XmedMainTokenGroup`; the
first four characters are the block id, the next eight the byte count, the next
four an unknown value, and the final four the declared item count.【F:src/BlingoEngine.IO.Legacy/Texts/XmedTokenGrouper.cs†L36-L79】
The ids map to the parser’s enum and drive how each group is interpreted.【F:src/BlingoEngine.IO.Legacy/Texts/Data/XmedTokenGroup.cs†L73-L115】

| Block id | Parser enum                                   | Purpose (current implementation) |
|---------:|-----------------------------------------------|----------------------------------|
| `FFFF`   | `RunHeaderFFFF`                               | File header & global metadata    |
| `0000`   | `RunHeader`                                   | Secondary header (rare)          |
| `0001`   | `Layout`                                      | Raw layout bytes (tabs/wrap flags) |
| `0002`   | `FullText`                                    | Zero blocks that store the literal text payload |
| `0004`   | `RunStyles`                                   | Style run map (style id + end offset pairs) |
| `0005`   | `RunParagraphs`                               | Paragraph run map (paragraph id + end offset pairs) |
| `0006`   | `Styles`                                      | 77-token style descriptors       |
| `0007`   | `Paragraphs`                                  | Paragraph descriptors + tab stops |
| `0008`   | `Fonts`                                       | Font table entries (family/style metadata) |
| `0009`   | `ParagraphBounds`                             | Baseline/width tuples per paragraph |
| `000A`   | `ParagraphBounds2`                            | Duplicate bounds stream (fallback) |
| `000B`   | `UnknownB`                                    | Reserved payload, currently passed through |
| `000C`   | `ParagraphFormats`                            | Format records with indentation/alignment flags |
| `000F`   | `ParagraphSpacing`                            | Before/after spacing values      |
| `0013`   | `Unknown13`                                   | Reserved payload                 |
| `0128`   | `Unknown128`                                  | Reserved payload                 |
| `0129`   | `Unknown129`                                  | Reserved payload                 |
| `FFFE`   | `PreRenderedBitmap`                           | Embedded TXc preview image       |

---

## Style, font, and paragraph records

### Style descriptors (`03:0006`)
- `XmedTokenGrouper` slices each descriptor into 77 tokens before handing the
  data to `BlXmedTokenStyleParser`.【F:src/BlingoEngine.IO.Legacy/Texts/XmedTokenGrouper.cs†L118-L150】
- The parser resolves:
  - Parent style id and font slot references.【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenStyleParser.cs†L34-L87】
  - Foreground/background colours and palette indices.【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenStyleParser.cs†L92-L123】
  - Font size, letter spacing, and the bold/italic/underline flags (other bits
    remain unused until more samples appear).【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenStyleParser.cs†L125-L159】
  - Style inheritance between descriptors via parent ids.【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenStyleParser.cs†L167-L214】

### Font table (`03:0008`)
Each entry is parsed into an `XmedFontDescriptor` that exposes the font family,
style, Windows code page, weight, pitch/family flags, and other Win32 LOGFONT
fields.【F:src/BlingoEngine.IO.Legacy/Texts/Data/XmedFontDescriptor.cs†L7-L88】
The parser keeps the original table index so style descriptors can resolve their
font slot later.【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenStyleParser.cs†L40-L47】【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenStyleParser.cs†L197-L215】

### Paragraph descriptors (`03:0007`)
`XmedParagraphDescriptorReader` consumes each paragraph block, resolves margins,
indentation, spacing, and alignment, and gathers tab-stop metadata. The reader
also applies paragraph bounds, formats, and spacing from the auxiliary block
streams once style slices are available.【F:src/BlingoEngine.IO.Legacy/Texts/XmedParagraphDescriptorReader.cs†L5-L118】【F:src/BlingoEngine.IO.Legacy/Texts/XmedSpacingReader.cs†L7-L164】

### Text and run maps (`03:0002`, `03:0004`, `03:0005`)
- `XmedFullTextReader` (invoked by `BlXmedTokenParser`) concatenates the
  `00(N):"…"` blocks to reconstruct the UTF-8 text. The run maps then slice that
  string by style and paragraph boundaries via `XmedSliceBuilder` before the
  style parser and paragraph reader attach semantics.【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenParser.cs†L33-L110】

---

## CASt member offsets (observed in samples)

The surrounding `CASt` data still exposes quick access fields for layout. These
values are read directly from the cast header (independent of the token stream):

| Offset  | Meaning (little endian)                      | Notes |
|--------:|----------------------------------------------|-------|
| `0x0018` | Field width in twips                         | Confirmed by test hints for `WiderWidth4`.【F:src/Director/BlingoEngine.Director.LGodot/Importer/TestData/XmedTestHints.cs†L23-L33】
| `0x001C` | Bit flags for style defaults (bold/italic/underline/…) | Matches the `FontTextStyle` bitfield used by legacy tools.【F:Test/TestData/Legacy/Texts_Fields/xmedCode.pat†L52-L69】
| `0x001D` | Layout flags (alignment, wrap, tabs)         | Mirrors the `Layout` bitfield in the pattern file.【F:Test/TestData/Legacy/Texts_Fields/xmedCode.pat†L70-L87】
| `0x003C` | Line spacing (twips)                         |                           |
| `0x0040` | Base font size (twips)                       |                           |
| `0x004C` | Declared text length                         | Used to delimit the text payload.|
| `0x04DA` | Left margin                                  | Field members share the same offsets.【F:Test/TestData/Legacy/Texts_Fields/xmedCode.pat†L89-L124】
| `0x04DE` | Right margin                                 |                           |
| `0x04E2` | First line indent                            |                           |
| `0x0CAE` | Spacing before paragraph                     | Values match spacing variants in samples.|
| `0x1970` | Spacing after paragraph                      |                           |

Offsets above `0x0500` shift when the descriptor/table blocks grow, so treat the
latter values as “typical locations” rather than fixed addresses.

---

## Historical research notes (superseded)

The following observations remain for context; the parser has since clarified
most of the uncertainties:

- Early notes assumed the directory rows encoded `<TYPE>,<OFFSET>,<COUNT>`.
  While the ASCII structure is correct, offsets are not present – the parser now
  derives group boundaries from the declared byte counts.
- A repeated `00(40):` pattern was linked to font names. The modern parser keeps
  these as font-table entries; the follow-up zeroed block is part of the
  descriptor payload rather than padding.
- Style-flag interpretations (bold/italic/underline/strikeout/subscript/
  superscript) were accurate, but the “editable” bit lives in the surrounding
  `CASt` header instead of the style descriptors themselves.
- Control-byte tables (`01`, `02`, `03`, `81`, `82`, `C1`, `C2`, `C3`) remain
  valid for understanding the tokenizer output, yet concrete semantics are now
  defined by the parser classes referenced above.

These historical snippets helped locate the modern structures, so they remain as
footnotes even though the speculation has been superseded.
