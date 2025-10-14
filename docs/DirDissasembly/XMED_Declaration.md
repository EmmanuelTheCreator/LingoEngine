# XMED_Declaration.md 

## XMED Log Format (intro)

This document uses the **token log** format.
Tokens are printed in read order.

- `00(len):"text"` → literal text block.
- `01:xxxx` → literal/value.
- `02:xxxx` → number (often twips or offsets).
- `C1(xx)` / `C2(xx)` → open a composite block.
- `81` → next field in the same block.
- `82` → end of current block.

Full specification: [XMED_Token_Log_Guide.md](XMED_Token_Log_Guide.md).

Example (RGB color in a style block):
```
C1(04) 01:FF00 81 01:CC00 81 01:6600 82
```

## Layout
```
Header → Text → Run Maps (03:0004/0005/0006) → Font Table (00(40)) → Tail (00(44))
```

## Header (stable)
`00:FFFF0000000600040001 01:77AA 03:0000000000XX00000000 …`
`C2(03)` + `C1(03)` sequences carry layout/metrics. Values vary per file but shape is fixed.

## Text
`00(-1):"…"` contains the full text, including CR/LF. Offsets in maps are 0-based into this text.

## Run Map — 03:0004 (required)
Payload: alternating pairs `(02:<endOffset>) (01:<styleId>)`.
Runs = pairCount + 1. The last pair closes the final run.
Example (Multi_Line_Multi_Style):
```
… 02:26 01:6  02:6D 01:9  02:B7 01:8  02:FE 01:A  02:12C 01:6
```
→ Runs: [0–0x26)=6, [0x27–0x6D)=9, [0x6E–0xB7)=8, [0xB7–0xFE)=A, [0xFE–0x12C)=6.

## Paragraph Flags — 03:0005 (required)
Same boundary list as 0004. Each boundary has a boolean flag (center/left etc.).
E.g. at 0xB7 → false, 0xFE → true. Use to set alignment per run/paragraph.

## Text slice record

Tuple: `(start, end, styleId, paragraphId)`

Derive:
- From `03:0004`: pairs `02:<end> 01:<styleId>`.
- Starts: `s0=0`, `s(i)=end(i-1)+1` (0-based).
- Final `E`: last `02:<end>` (or `03:0128/0129`).

Paragraphs:
- From `03:0005`: same `02:<end>` boundaries + booleans.
- Build paragraph spans between boundaries; assign ordinal = `paragraphId`.

Pseudocode:
```
B = [(e1,id1),(e2,id2),...(E,idN)]
runs = []
s = 0; p = paragraph_index_map_from_0005()
for k,(ek,idk) in enumerate(B):
    runs += [(s, ek, idk, p.span_of(s,ek))]
    s = ek+1
```

Style details: map `idk` via `03:0006`.

## Style Table — 03:0006 (required)
Maps styleId → concrete attributes:
- FontRef (index into Font Table)
- Size (pt)
- Bold / Italic / Underline
- ForeColor / BackColor
- Optional spacing/leading overrides
Observed across: Single-Line, Multi-Line, NoBold, Multicolor, Multifont files.

## Font Table — 00(40) pairs (required)
Sequence of pairs per font used:
```
00(40):"FontName"
00(40):""
```
Order matches style references (e.g., Arial, Tahoma, Terminal, Vivaldi, Arcade*).

## Colors (observed tokens)
Color indices appear as short tokens near style entries; examples seen:
`01:FF00` (red), `01:CC00`, `01:6600`. Use alongside 0006 to apply per run.

## 🎨 Color Parsing

**Location:** inside a style composite `C1(04) … 82`.

**Grammar (tokens):**
- `C1(xx)` = open; `81` = next field; `82` = close; `01:<v>` = value.

**Examples:**
- Bordeau: mixed ASCII/binary; missing G,B ⇒ **880000**.

### Color channels: missing values

Rule:
- Colors are inside `C1(04) … 82`.
- Channels come as `01:<R> [81] 01:<G> [81] 01:<B>`.
- When a channel token is **absent**, its value is **0** (default). No padding step needed.
- Numeric tokens can be ASCII (`01:FF`) or two-byte hex (`01:FF00` → `0xFF`). Always take the **high byte**.

Pseudocode:
```
(r,g,b) = (0,0,0)
read until 82:
  if token == 01:<v> and expecting R then r=v
  else if token == 01:<v> and expecting G then g=v
  else if token == 01:<v> and expecting B then b=v
  if token == 81 continue
```
Works for ASCII-hex and binary forms.

## Line/Paragraph Metrics (observed)
`03:000C` varies in the 2-line/line-space sample. Treat as paragraph metrics (line spacing, before/after).

## Tail — 00(44) (stable)
Always present and identical in samples:
`00(44):45,46,182,181,149,181,165,165,46,39,34,145,146,147,148,133,131`
Treat as a global lookup/palette table.

## Token Roles (recap)
- `01` literal/styleId marker
- `02` numeric (positions/sizes)
- `81` continuation within a composite
- `82` composite terminator
- `C1/C2` block open/close (style/paragraph groups)


## 🔗 Composite Tokens: `81` / `82` (incl. Colors)

- **Blocks:** `C1(xx)` / `C2(xx)` start a **composite**.
- **Continue:** `81` = another field in the **current** composite.
- **End:** `82` = **close** current composite (stack-based; multiple `82` can close nested blocks).

### Color triplet (inside style)
```
C1(04) …            ← style composite
  01:FF00 81        ← R
  01:CC00 81        ← G
  01:6600           ← B
82                  ← end style composite
```
- Read **R,G,B** values between `C1/C2` … `82`.
- Apply to the run referenced by `03:0006` (style id).







## 🧮 Font Size & Line Height Mapping

### Source
Values appear in the **header (`C2(03)` block)** and in **style descriptors (`03:0006`)**.

### Line Height
- Found in early `C2(03)` numeric pairs (`02:<value>`).  
- Matches filename numbers directly (e.g. `13`, `16`, `20`, `39`).  
- Encoding: stored as `value × 10` (e.g. `13 → 0x82`, `20 → 0xC8`, `39 → 0x186`).

### Font Size
- Actual point size is stored per style in `03:0006`.  
- Sometimes the header repeats an approximate pixel value (`pt × 1.333 × 10`).  
- Conversion back to points:  
  ```
  pt ≈ headerValue / 13.33
  ```

### Rule of Thumb
- Use `03:0006` for **true font size**.  
- Use `C2(03)` for **baseline and line height defaults** shared by all runs.


## 📐 Text Box Size (Width & Height)

### Location
Bounding dimensions are stored in the **`C2(0A)`** block near the start of each XMED member.

### Pattern
```
C2(0A) 02:<Left>  02:<Right>  ...  02:<Top>  02:<Bottom>  ...
```
Typical form:
```
C2(0A) 02:18  02:B2  ...      ← Normal width  
C2(0A) 02:1E  02:120 ...      ← Wider field  
```

### Width
- Calculated as `Right - Left` (in **twips**, 1/20 pt).  
- Increasing these values enlarges the horizontal span of the text area.  
- Example:
  - `0x18–0xB2` → width ≈ 146 twips  
  - `0x1E–0x120` → width ≈ 258 twips

### Height
- Height derives from the **Top–Bottom** pair inside the same block (if present).  
- If omitted, use line-based reconstruction:
  ```
  Height ≈ LineCount × LineHeight
  ```
- `LineHeight` originates from the **header (`C2(03)`)** or per-style values in **`03:0006`**.
- Optional extra leading or paragraph spacing appears in **`03:000C`**.

### Summary
| Field | Source | Unit | Description |
|--------|---------|------|-------------|
| Left / Right | `C2(0A)` | twips | Horizontal bounds |
| Top / Bottom | `C2(0A)` | twips | Vertical bounds |
| LineHeight | `C2(03)` / `03:0006` | twips | Line spacing |
| Paragraph spacing | `03:000C` | twips | Additional before/after offset |

Together these fields define the complete **text box size and placement** used by Director.


## 🧾 Paragraph Layout: Margins, Indents & Spacing

### Location
Paragraph metrics appear mainly in the **`C2(03)`** and **`03:000C`** blocks.

### Observed Patterns
```
C1(03) 02:120  02:168  02:1C  02:0     ← Paragraph 1
C1(03) 02:48   02:90   02:15  02:0     ← Paragraph 2
```

### Field Mapping
| Field | Example (hex) | Decimal | Meaning |
|-------|----------------|----------|----------|
| `02:120` | 288 | Left margin |
| `02:168` | 360 | Right margin |
| `02:1C`  | 28  | First line indent |
| `02:48`  | 72  | Smaller left margin |
| `02:90`  | 144 | Smaller right margin |
| `02:15`  | 21  | First indent (0.3–0.4 inch range) |

### Spacing Before / After
Stored in **`C2(03)`** right after each margin group:
```
C2(03) 02:9  02:7  02:0
```
→ `9` = spacing before, `7` = spacing after (in points or twips).

### Summary Table
| Property | Source Block | Example | Unit | Description |
|-----------|---------------|----------|-------|--------------|
| Left Margin | `C1(03)` | `02:120` | twips | Paragraph left offset |
| Right Margin | `C1(03)` | `02:168` | twips | Paragraph right offset |
| First Indent | `C1(03)` | `02:1C` | twips | Indent for first line |
| Spacing Before | `C2(03)` | `02:9` | pt/twips | Space above paragraph |
| Spacing After | `C2(03)` | `02:7` | pt/twips | Space below paragraph |

### Meaning
Each paragraph has its own `C1(03)` block defining margins/indent and a following `C2(03)` block for vertical spacing.  
These parameters perfectly match the Director UI: margin-left/right, first indent, and spacing before/after.


## 🔠 Kerning & Character Spacing

### Location
Kerning and extra letter spacing appear in the **`C2(03)`** and **`C2(04)`** sections following the header.

### Pattern
```
C2(03) 02:20000  <82  02:0 
C2(04) 02:1  02:0
```

### Interpretation
| File | Key Token | Observed Value | Meaning |
|------|------------|----------------|----------|
| **A01_Core_Min_13** | `02:20000` | default | Base kerning spacing |
| **Font_Spacing_30_13** | `02:1E` | +30 | Increased tracking/spacing |
| **Font_Kerning_Pos2_13** | `02:20000` then `02:18` | minor offset | Kerning enabled |

### Mapping
- `C2(03)` → Base kerning table or offset between pairs (auto spacing).  
- `C2(04)` → Additional user-defined character spacing (tracking).  
- Units are likely **twips** (1/20 pt).  
- Director’s “Spacing” field in Text Inspector maps directly to these values.

### Summary
| Property | Block | Example | Description |
|-----------|--------|----------|--------------|
| Kerning | `C2(03)` | `02:20000` | Default glyph pair spacing (auto) |
| Character Spacing | `C2(04)` | `02:1E` | Manual tracking; increases distance between characters |
| Combined Result | — | sum of both | Effective horizontal spacing per glyph |

### Notes
- `A01_Core_Min_13.xmedlog` defines the base state (no extra spacing).  
- `Font_Spacing_30_13.xmedlog` raises the spacing offset, confirming scaling.  
- `Font_Kerning_Pos2_13.xmedlog` adds positional kerning correction without changing font metrics.


## ✍️ Font Style Flags

### Location
Font styling attributes appear in the **`03:0006` style descriptor** and are reflected by control bytes like `C1(1C–1E)` and `C1(0A–0B)` inside each text run.

### Identified Style Bits
| Style | Example File | Marker | Description |
|--------|---------------|---------|--------------|
| **Bold** | *(absent in NoBold)* | `C1(1E)` missing | Style weight normal (Bold disabled) |
| **Italic** | `Text_Hallo_italic_13` | `C1(1D)` | Activates italic rendering |
| **Underline** | `Text_Hallo_underline_13` | `C1(1C)` | Adds underline line |
| **Strikeout** | `Text_Hallo_strikeout_13` | `C1(13)` | Draws horizontal strike line |
| **Superscript** | `Text_Hallo_superscript_13` | `C1(0A)` / `C1(12)` | Raises baseline |
| **Subscript** | `Text_Hallo_subscript_13` | `C1(0B)` / `C1(11)` | Lowers baseline |

### Notes
- Each `C1(xx)` control appears after the run’s `C2(07)` group, applying to that run only.  
- Combination of multiple flags (e.g., Bold + Underline) stacks within the same style entry of `03:0006`.  
- These flags correspond directly to Director’s text inspector checkboxes.

### Example
```
C1(1C) <82> <82>       ← Underline  
C1(13) <82> <82>       ← Strikeout  
C1(0A)/C1(12) <82> <82> ← Superscript  
C1(0B)/C1(11) <82> <82> ← Subscript
```

Together with color and font references, these bits define the full per-run style in XMED.


## ⚙️ Field Properties (Tabs, Wrapping, Editable)

### Location
Global text-field properties appear in the **header control blocks** (`C2(03)` … `C2(0B)`) and are boolean flags not tied to runs.

### Observed Properties

| Property | Example File | Key Marker | Value | Meaning |
|-----------|--------------|-------------|--------|----------|
| **Tabs Enabled** | `Text_Hallo_tab_true_13` | `C2(07)` → followed by `true false` | `true` | Tabulation support active |
| **Word Wrap** | `Text_Hallo_wrap_off_13` | `C2(07)` near header | `false` | Word wrapping disabled |
| **Editable** | `Text_Hallo_editable_true_13` | `C2(0B)` followed by `true 02:0` | `true` | Text field is user-editable |

### Meaning
- `C2(07)` controls text flow behavior (tabs, wrapping).  
  - First boolean → tab expansion.  
  - Second boolean → wrapping on/off.  
- `C2(0B)` defines editability for the field object.  
  - When `true`, Director allows runtime text input.  

### Example
```
C2(07) true false     ← Tabs enabled, wrap disabled
C2(0B) true 02:0      ← Editable field
```

### Summary Table
| Code | Function | Example Value | Description |
|------|-----------|----------------|-------------|
| `C2(07)` | Tabs / Wrapping | `true false` | Tabulation and word wrap control |
| `C2(0B)` | Editable | `true` | Field can be modified by the user |




# XMED Status (quick map)

## ✅ Known (parse now)
- C2(03): header, lineheight
- C2(0A): box Left/Right/(Top/Bottom)
- C2(04): character spacing (tracking)
- 03:0004: run boundaries
- 03:0005: paragraph align flags
- 03:0006: style table (font,size,b/i/u,s/u,strike,color)
- 00(40): font table
- C1(04)+81/82: RGB triplet
- 03:000C: spacing before/after
- C2(07): tabs/wrap
- C2(0B): editable

## 🟧 Probable
- C1(20): group delimiter
- C2(06), C2(12), C2(0F): flow/meta

## ⬜ Unknown (todo)
- 03:0007 role
- 03:0013 flags matrix
- 00(44) tail meaning
- Header: 02:40001, 02:-7FFD6FE0
- Repeated 01:FFFF usage

---

## ❓ Open Questions / Uncertain Parts

- **03:0007** — block purpose unknown (likely run/link meta?).  
- **03:0013** — flags/feature matrix; semantics unclear.  
- **00(44)** — trailing byte table: exact meaning unresolved.  
- **Header fields** — `02:40001`, `02:-7FFF6FE0`: need definitive mapping.  
- **01:FFFF** — repeated marker; context-dependent meaning not fixed.  
- **C1(20), C2(06), C2(12), C2(0F)** — suspected flow/group/meta; verify roles and ordering.  

*Notes:* keep captures from diverse samples; confirm with round‑trip edits.

## 🔎 Additional Findings 

- **Tab Stops**
  - In `C1(03)`, sequential `02:<pos>` pairs define tab-stop positions per paragraph.
  - Seen as `02:7 02:C …` in *MultiLine_Tabs*.

- **Run Maps**
  - `03:0004/0005/0006` consistent for 5-run samples; use as validation.

- **Per-Line Metrics**
  - Multi-line headers show repeating pairs (e.g., `02:11 02:17`), acting as per-line metric slots.

- **Text Length Index**
  - `03:0128` and `03:0129` carry the final text end offset (e.g., `0x1E/0x22/0x26`).

- **Still Uncertain**
  - `03:0013`, `C2(06)`, `C2(12)`, `C2(0F)`, `C2(08)` → flow/meta/cache (TBD).
