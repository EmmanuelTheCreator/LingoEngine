# XMED Offsets & Structures (Consolidated)

> Scope: **inside XMED text/field members only** (no CASt generalities). This file merges header pointers, text/run maps, font tables, and bit‑flag bytes. Values are derived from the provided test set and validated notes.

Handy tool and docs : https://imhex.werwolv.net/  : https://docs.werwolv.net/imhex/views/hex-editor#data-visualizers

---

## 1) Header Directory (ASCII)
It seems The XMED header starts with ASCII **directory rows**: PERHAPS
```
<TYPE:4>,<OFFSET:8>,<COUNT:8>
```

- All three numbers are **hex strings** (ASCII).  
- Rows continue **until the first 0x00 byte** (no explicit header length).  
- Example: `0008,000005B0,00000010` → PERHAPS: Type **0008** at offset **0x05B0** with **16** entries capacity. STILL WRONG


Perhaps blocks starting with 
0x00 [XX XX: is the length of the content] [COMMA]
- First block is the text
- Next blocks are always the font name
- The latest : We always get these values in the latest blok
	045,046,182,181,149,181,165,165,046,039,034,145,146,147,148,133,131 	

Perhaps:
- **0008** → **Font‑names table** (“40,” records).

- Jump to OFFSET from the directory entry.  
- Structure repeats **COUNT** times:
  - ASCII **`"40,"`** (`34 30 2C`),
  - **1 byte** name length **L**,
  - **L** bytes ASCII font name,
  - optional **NUL padding** before next record.


## 5) Style Descriptor Blocks
Found later in the file for multi‑style texts; each block carries:
- Style/flag bytes (same layout as below),
- Font link (font id/name),
- Optional color index and metric overrides (size/spacing).

Descriptors are referenced by **F5** perhaps in the run map. Single‑style files still use the same structure but only one descriptor is effective.

---

## 6) Flag Bytes (core per‑member header)
Two adjacent bytes encode styles and alignment/layout:

### 0x001C — Style Flags (**bitmask**)
| Bit | Mask | Meaning |
|---:|:----:|---------|
| 0 | `0x01` | Bold |
| 1 | `0x02` | Italic |
| 2 | `0x04` | Underline |
| 3 | `0x08` | Strikeout *(seen in diffs; confirm per file)* |
| 4 | `0x10` | Subscript *(tests suggest; confirm)* |
| 5 | `0x20` | Superscript *(tests suggest; confirm)* |
| 6 | `0x40` | Outline/Shadow/Tabbed‑field marker *(semantic varies; confirm)* |
| 7 | `0x80` | **Editable field (member‑wide only)** — valid in header, ignored in run descriptors |

> Current test set confirms 0–2. Others have evidence but remain flagged **confirm** with more files.

### 0x001D — Alignment / Layout (**bitfield**)
| Bits | Mask | Meaning |
|:---:|:----:|---------|
| 0–1 | `0b00000011` | **Alignment core**: `00` Center, `01` Right, `10` Left, `11` Justified |
| 3 | `0x08` | **Wrap disable** (1 = **NoWrap**) |
| 4 | `0x10` | **Tab present** |
| 2,5–7 | — | Reserved / TBD |

**Observed combined values** (examples from tests):  
`0x00` (Center), `0x10` (Center+Tab), `0x15` (Right+Tab), `0x1A` (Left+Tab), `0x19` (Center+NoWrap+Tab), `0x03` (Justified).

---

## 7) Metrics & Pointers (fixed offsets in member header perhaps) perhaps

| Offset | Size | Meaning |
|------:|-----:|---------|
| `0x0018` | 2/4 | Field width (twips) |
| `0x003C` | 2/4 | Line spacing (leading) |
| `0x0040` | 4  | Font size (LE `Int32`) |
| `0x004C` | 4  | Text length (LE `Int32`) → used to delimit text block |

### Paragraph / Margins (from test set; reconfirm on more files)
| Offset | Meaning |
|------:|---------|
| `0x04DA` | Left margin |
| `0x04DE` | Right margin |
| `0x04E2` | First line indent |

### Additional data seen in samples
| Offset | Meaning |
|------:|---------|
| `0x0622` | Color table start |
| `0x0983` | Font name string (ASCII, NUL‑terminated) |
| `0x0CAE` | Spacing **before** paragraph |
| `0x0EF7` | Member name string |
| `0x1354` | Second color table (multi‑style) |
| `0x1970` | Spacing **after** paragraph |

> Note: Offsets beyond the core header can drift with file variants. Treat these as **typical locations** observed in this test batch.




# Assumptions
plain ASCII bytes for readable data, and non-ASCII bytes (≤ 0x20 or ≥ 0x80) as control flags.

### 🧩 1. Two intertwined layers we think
| Layer	          | What it carries	                                                               | Byte range      |
|-----------------|--------------------------------------------------------------------------------|---------------- |
| Data layer	  | Anything you can type — text, hex digits for RGB, font names, numbers, etc.	   | 0x20–0x7E (normal printable ASCII) |
| Control layer   |	Structural delimiters, property markers, run boundaries, style field switches  | 0x00–0x1F and 0x80–0xFF            |

Director’s text system reuses Apple’s TextEdit style runs (moaTEStyles / ScrpSTElement) where high-bit markers signal “this is not literal text”.

### 🧠 2. Common control families already seeing Perhaps
| Byte(s) | Binary | Typical meaning (from pattern analysis) |
|----------|---------|-----------------------------------------|
| **01**			 | 0000 0001			 | Start/value marker — next bytes are literal ASCII data (digits, hex pairs, etc.) |
| **02**			 | 0000 0010			 | Numeric token start — read following ASCII digits as numeric value (font size, line height, etc.) |
| **03**			 | 0000 0011			 | Small counter/terminator or parameter separator (often follows C1) |
| **0A–0F** 🔹		 | 0000 1010 … 0000 1111 | *Low control codes used for style toggles:*<br>  • 0A / 0C = superscript markers (start/close)<br>  • 0B / 0D = subscript markers (start/close)<br>  • 0E / 0F = internal run or alignment boundaries |
| **10–19 (hex)** 🔹 | 0001 0000 … 0001 1001 | *Higher “style state” IDs*:<br>  • 11 (0x0B paired with 0x11) = subscript block end<br>  • 12 (0x0A paired with 0x12) = superscript block end<br>  • 13 = strikeout end marker<br>  • 1C = underline end marker<br>  • 1E / 20 = normal/baseline state |
| **2D**			 | 0010 1101			 | ASCII ‘-’; inside numeric token denotes negative (e.g., “-1”) |
| **30–39, 41–46**	 | —					 | Printable ASCII digits ‘0’–‘9’, ‘A’–‘F’; literal data (colors, numbers) |
| **81**			 | 1000 0001			 | Continuation / next sub-field within same property (RGB separator, multi-value) |
| **82**			 | 1000 0010			 | End-of-value marker / boundary before next property block |
| **83–87**			 | 1000 0011 … 1000 0111 | Reserved high-bit controls (unseen or undefined so far) |
| **C1 xx**			 | 1100 0001 xxxx xxxx   | Style-opcode prefix — `xx` identifies property (bold, underline, strikeout, sub/sup, etc.) |
| **C2 XX **         | 1100 0010			 | A block of numbers: PERHAPS : where XX is the count of following ASCII numbers? nont sure
| **C3–CF**			 | 1100 0011 … 1100 1111 | Higher control tags (rare; likely extended run types) |
| **FF**			 | 1111 1111			 | Padding / file-section end marker |

All other bytes (30–39, 41–46, etc.) are literal characters used in property data — text, numbers, “FF00”, etc.

### 🧩 3. Why this design perhaps

Director had to store styled text inline with the actual characters but still survive on both Mac (Big-Endian, resource-fork) and Windows (flat files).
ASCII-safe encoding made it portable, so any byte outside printable range was reserved for control signals — a bit-flagged mini-language marking:

run start / end

property ID (font, size, color, alignment, etc.)

sub-field separators (for multi-value props like RGB)

### Colors bytes discovered
Text_Hallo_col_*_13.xmed
Color Blue      : #0000ff : 01 30 82 82 81 01 46 46 30 30  81 01 30        01 46 46 46 46 81 81 01 30 82 02 
Color Yellow    : #ffff00 : 01 30 82 82    01 46 46 30 30  81 01 30 81     01 46 46 46 46 81 81 01 30 82 02 
Color Pink      : #ff00ff : 01 30 82 82    01 46 46 30 30  01 30           01 46 46 30 30 01 30 01 46 46 46 46 81
Color LightGreen: #ccff99 : 01 30 82 82    01 43 43 30 30  01 46 46 30 30  01 39 39 30 30  01 30 01 46
Color Orange    : #ffcc66 : 01 30 82 82    01 46 46 30 30  01 43 43 30 30  01 36 36 30 30  01 30 01 46
Color Bordeau   : #880000 : 01 30 82 82    01 38 38 30 30  01 30 81 81     01 46 46 46 46 81 81 01 30 82

Text_Multi_Style_Size_Color_13.xmed
Red:	#F7204A:	                                 01 43 01 33 01 30 81 82 82 01 46 37 30 30 01 32 30 30 30 01 34 41 30 30 01 30 01 46 46 46 46 81 81 01 30 82 02 43 30 30 30 30 02 30
Green: 	#1EF02E:	               82 01 31 01 30 81 01 39 01 32 01 30 81 82 82 01 31 45 30 30 01 46 30 30 30 01 32 45 30 30 01 30 01 46 46 46 46 81 81 01 30 82 02 39 30 30 30 30 02 30
Blue: 	#2702FD:										01 43 01 33 01 30 81 82 82 01 32 37 30 30 01 32 30 30 01 46 44 30 30 01 30 01 46 46 46 46 81 81 01 30 82 02 43 30 30 30 30 02 30
Blue: 	#2702FD:	03 82 01 32 01 30 81 01 31 30 01 32 01 30 81 02 34 30 30 02 30 01 32 37 30 30 01 32 30 30 01 46 44 30 30 01 30 01 46 46 46 46 81 81 01 30 82 02 31 32 30 30 30 30 02 30

### style maps?

Perhaps: [ 02 30 02 <font size key> 01 ] [ 02 ?? ?? <line height key> 01 ]
font size, line height, bytes
|------:|---------|
| 09	|	11	|	02 30 02 39 32 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 10	|	12	|	02 30 02 39 33 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 11	|	13	|	02 30 02 39 46 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 12	|	14	|	02 30 02 41 30 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 13	|	16	|	02 30 02 41 38 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 14	|	17	|	02 30 02 41 43 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 15	|	18	|	02 30 02 41 46 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 16	|	19	|	02 30 02 42 39 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 17	|	21	|	02 30 02 42 46 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 18	|	22	|	02 30 02 43 44 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 19	|	23	|	02 30 02 44 31 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 20	|	24	|	02 30 02 45 30 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 21	|	25	|	02 30 02 45 42 01 30 C1 03 02 2D 31 82 02 35 42		| 
| 22	|	27	|	02 30 02 31 30 37 01 30 C1 03 02 2D 31 82 02 35		| 
| 23	|	28	|	02 30 02 31 30 43 01 30 C1 03 02 2D 31 82 02 35		| 
| 24	|	29	|	02 30 02 31 31 31 01 30 C1 03 02 2D 31 82 02 35		| 
| 28	|	34	|	02 30 02 31 34 43 01 30 C1 03 02 2D 31 82 02 35		| 
| 29	|	35	|	02 30 02 31 35 39 01 30 C1 03 02 2D 31 82 02 35		| 
| 39	|	47	|	02 30 02 32 31 43 01 30 C1 03 02 2D 31 82 02 35		| 
| 40	|	48	|	02 30 02 32 32 35 01 30 C1 03 02 2D 31 82 02 35		| 
| 41	|	49	|	02 30 02 32 32 45 01 30 C1 03 02 2D 31 82 02 35		| 
| 42	|	51	|	02 30 02 32 34 30 01 30 C1 03 02 2D 31 82 02 35		| 
| 43	|	52	|	02 30 02 32 34 39 01 30 C1 03 02 2D 31 82 02 35		| 
| 47	|	57	|	02 30 02 32 37 36 01 30 C1 03 02 2D 31 82 02 35		| 
| 49	|	59	|	02 30 02 32 38 38 01 30 C1 03 02 2D 31 82 02 35		| 
| 50	|	60	|	02 30 02 32 43 44 01 30 C1 03 02 2D 31 82 02 35		| 
| 51	|	62	|	02 30 02 32 45 31 01 30 C1 03 02 2D 31 82 02 35		| 
| 52	|	63	|	02 30 02 33 32 41 01 30 C1 03 02 2D 31 82 02 35		| 
| 53	|	64	|	02 30 02 33 33 35 01 30 C1 03 02 2D 31 82 02 35		| 
| 54	|	65	|	02 30 02 33 34 42 01 30 C1 03 02 2D 31 82 02 35		| 
| 69	|	71	|	02 30 02 35 31 31 01 30 C1 03 02 2D 31 82 02 35		| 
| 79	|	95	|	02 30 02 36 43 46 01 30 C1 03 02 2D 31 82 02 35		| 
| 89	|	107	|	02 30 02 37 39 42 01 30 C1 03 02 2D 31 82 02 35		| 
| 96	|	116	|	02 30 02 38 33 42 01 30 C1 03 02 2D 31 82 02 35		| 
| 200	|	241	|	02 30 02 32 44 42 37 01 30 C1 03 02 2D 31 82 02		| 

## AI proposition

What stays constant

Printable bytes (30–39, 41–46) carry the payload (numbers, hex).

Non-ASCII/high-bit bytes are control:

02 … = numeric token (ASCII digits follow)
01 / 81 = value/continuation (like with color)
C1 xx = style opcode within a run; different xx identify/toggle specific styles.
82 82 / 82 02 = property/run boundaries.

Style opcodes I can fingerprint
| Style            | Opcode pattern (decimal shown; hex in () ) | Notes / where seen                                   |
|------------------|--------------------------------------------|------------------------------------------------------|
| (baseline, none) | C1 30 (1E), C1 32 (20)                                                                     | Common structure in all files. |
| **Underline ON** | **C1 28 (1C)** (+ small value: `81 81 01 31 01 30`) replaces a baseline `C1 32 (20)` at that spot | Underline file swaps a `C1 32` region for `C1 28 …`. |
| **Strikeout ON** | **C1 11 (0B)** … value `01 31 01 30` … **C1 19 (13)**                                      | Clear paired markers around the toggle. |
| **Subscript**    | **C1 11 (0B)** … `01 33 01 30` … **C1 17 (11)**; and later **C1 13 (0D)** … **C1 17 (11)** | Two paired sites for the subscript state.  |
| **Superscript**  | **C1 10 (0A)** … `01 33 01 30` … **C1 18 (12)**; and later **C1 12 (0C)** … **C1 18 (12)** | Mirror of subscript with its own codes.  |
| **Tabs enabled** | Same baseline opcodes; no extra C1 low-codes like above                                    | No special low `C1,xx` toggles beyond the common set. {index=5} |

Concrete windows (so you can annotate)

Underline vs NoBold around the same spot:
NoBold: … 02 30 C2 07 **C1 20** 82 82 C1 03 82 03 …
Underline: … 02 30 C2 07 **81 81 01 31 01 30 C1 1C** 82 82 C1 03 82 03 …
Strikeout at the toggle site:
… 02 30 C2 07 **C1 0B 01 31 01 30 C1 13** 82 82 C1 03 …
Subscript (first site):
… **C1 0B 01 33 01 30 C1 11** 82 82 C1 03 … and later … **C1 0D … C1 11** …
Superscript (first site):
… **C1 0A 01 33 01 30 C1 12** 82 82 C1 03 … and later … **C1 0C … C1 12** …

How to read it (same logic as color)

C1 xx = which style toggle/attribute.
01/81 = field/value delimiters.
Small ASCII numbers after the toggle (01 31, 01 33 = ‘1’, ‘3’) are the state/variant.

A later C1 yy often closes or confirms the same style (paired markers).

# Observered 4 values in 03:0000 Header block

| File                                      | Header quartet after C1(03) |
|-------------------------------------------|------------------------------|
| F02_MultiLine_Tabs_V2_13.xmedlog.txt      | 02:7 		02:C 	02:7 	02:C	| 
| Text_3_Paragraps_3T_13.xmedlog.txt        | 02:0 		02:5 	02:0 	02:5	| 
| Text_Hallo_margin_spacing_FirstInd_13...  | 02:0 		02:5 	02:0 	02:5	| 
| Text_Hallo_changed_color_13.xmedlog.txt   | 02:0 		02:5 	02:0 	02:5	|
| F02_MultiLine_Tabs_13 					| 02:7 		02:C 	02:7 	02:C	|
| Color_1_13.xmedlog.txt					| NONE								|
| Color_2_13.xmedlog.txt					| 02:7 		02:D 	02:7 	02:D	|						
| Color_3_13.xmedlog.txt					| 02:E  	02:14 	02:E 	02:14	|
| Color_4_13.xmedlog.txt					| 02:15 	02:1B 	02:15 	02:1B	|

# C2 INVESTIGATIONS

Observed C2 tags

### 🎨 Run Styles
| C2(Tag) | Example | Context |
|----------|----------|----------|
| `C2(07)` | `C2(07) 01:1 01:0`, `C2(07) <81 <81 01:1 01:0` | Always appears near text style flags (bold, italic, underline). |
| `C2(0A)` | `C2(0A) 02:6 02:0`, `C2(0A) 02:5 02:0`, `C2(0A) 02:1 02:0` | Always follows `02:<num> 02:0`, linking to font index. |
| `C2(13)` | `C2(13)` | Appears when a text run continues across a line or wrap boundary. |
| `C2(0B)` | Rare; e.g. `C2(0B)` | Appears occasionally after 0A in long runs — unknown but distinct from paragraph data. |

---

### 📏 Paragraph Definitions
| C2(Tag) | Example | Context |
|----------|----------|----------|
| `C2(04)` | `C2(04) 02:55 01:0` | Appears near tabstop and text width definitions. |
| `C2(06)` | `C2(06) 02:6A03E2AE 01:4 02:1 02:96 02:0 …` | Tab stops list: first byte = tab count, then pairs `02:1 02:<pos>`. |
| `C2(07)` | `C2(07)` (empty) | Appears after (06) in some files, likely continuation marker. |
| `C2(0D)` | `C2(0D)` | Single isolated occurrence — possibly margin padding. |
| `C2(20)` | `C2(20)` | Seen in multi-paragraph layout definitions — unknown struct header. |
| `C2(23)` | `C2(23)` | Single appearance in paragraph header, maybe alignment flag group. |
| `C2(26)` | `C2(26)` | Appears once near right-margin values. |
| `C2(0A)` | `C2(0A)` | Found rarely here too, likely shared field reused for linking to style/font. |

---

### 🔠 Fonts Block
| C2(Tag) | Example | Context |
|----------|----------|----------|
| `C2(03)` | `C2(03) 02:101 01:0` | Script ID (Western/Latin 1252 = 257). Always ends font record. |
| `C2(0C)` | `C2(0C) 02:400 02:0` | Width/pitch metric; seen after font weight. |
| `C2(0A)` | `C2(0A)` | Appears rarely in font definitions, same form as run styles, may indicate cross-link to style. |


### 🧾 Tab / Wrap Test Files (Paragraph-Layout Related)

| C2(Tag) | Example Snippet | Perhaps  Meaning |
|----------|-----------------|------------------|
| `C2(06)` | `C2(06) 02:6A03E2AE 01:4 02:1 02:96 02:0 <82 02:1 02:D8 02:0 <82 02:1 02:120 02:0 <82 02:1 02:169 02:0 <82 02:18 01:0 <82` | Tab-stop list. The first `01:4` = tab count. Each `02:1 02:<val>` pair defines one stop position (e.g. 0x96, 0xD8, 0x120, 0x169 = 150, 216, 288, 361 px). Final `02:18` ≈ tab width (default 24 px). |
| `C2(07)` | `C2(07)` (often empty) | Wrap or continuation marker immediately after (06). Indicates that text flow respects word-wrap for that paragraph. |
| `C2(0F)` | `C2(0F) <81 <82 02:1 02:0` | Paragraph-header flag before a tab section. Likely signals paragraph start or block reference. |
| `C2(12)` | `C2(12)` (appears between paragraph and font blocks) | Connector between paragraph formatting and subsequent font list; seems to delimit the end of paragraph definitions. |
| `C2(20)` | `C2(20)` | Found in multi-paragraph blocks; probably an alignment or layout flag group. |
| `C2(26)` | `C2(26)` | Appears once near right-margin region — possible right-margin definition or hanging-indent marker. |

**Notes**
- Every paragraph-layout block starts with a `C2(06)` list → defines perhaps tab stops.
- Files without tabs still show a minimal `C2(06) … 02:18 01:0` entry → implies “no tabs, default width = 24 px”.
- `C2(07)` always follows `C2(06)` → confirms a logical grouping for *tab + wrap* handling.
- `C2(0F)` and `C2(12)` delimit higher-level paragraph sections or transitions to font/line-height data.

### Paragraphs in different files
//------------------------------------ File: Text_Multi_Style_Size_Color_13.analyse.txt
03:00070000004D00000002 		// Paragraphs Definitions
01:0 
// Paragraph 0
		<81 <81       							C2(0F) 
	<81 <82 02:1 02:0 							C2(06) 
	02:6A03E2AE 01:0 02:18 01:0 <82  						// Tab stops with only default Tab width of 24px
      C1(03)       								C2(12) 
// Paragraph 1	  
      C1(03) 							       	C2(0F) 
	  <81 <82 02:1 02:0       					C2(06) 
	  02:6A03E2AE 01:1 
		<82 02:17B 02:0 
		<82 02:18  01:0 
		<82 C1(03)  							C2(12) 		// Tab stops with 1 tab stop at 397px and default Tab width of 24px
		
//------------------------------------ File: Text_3_Paragraps_4T_13.analyse.txt
03:0007000000D400000004 		// Paragraphs Definitions  
    01:0 
// Paragraph 0 	  	
	  <81 <81  									C2(0F) 
	  <81 <82 02:1 02:0 						C2(06) 
	  02:6A03E2AE 01:0 02:18 01:0 <82 C1(03) 	C2(12) 
// Paragraph 1	  	  
	  01:1 01:0 <81 							C2(0F) 
	  <81 <82 02:1 02:0							C2(06) 
	  02:6A03E2AE 01:0 02:18 01:0 <82 C1(03) 	C2(12) 
// Paragraph 2	  
      C1(03) 02:120 02:168 02:1C 02:0 			C2(03) 
	  02:9 02:7 02:0 							C2(05) 
	  <81 <82 02:1 02:0 					    C2(06) 
	  02:6A03E2AE 01:0 02:18 01:0 <82 C1(03) 	C2(12) 
// Paragraph 3	  	  
      C1(03) 02:48 02:90 02:15 02:0       		C2(03) 
	  02:4 02:5 02:0 							C2(05) 
	  <81 <82 02:1 02:0       					C2(06) 
	  02:6A03E2AE 01:4 
		02:1  02:96  02:0 <82 
		02:1  02:D8  02:0 <82 
		02:1  02:120 02:0 <82 
		02:1  02:167 02:0 <82 
		02:18 01:0   <82 
      C1(03) 						      		C2(12) 		
//------------------------------------	  File : F02_MultiLine_Tabs_13.analyse.txt
03:00070000004B00000002 		// Paragraphs Definitions 
    01:0 
// Paragraph 0 	 	
	<81 <81       								C2(0F) 
	<81 <82 02:1 02:0       					C2(06) 
	02:6A03E2AE 01:0 02:18 01:0 <82 C1(03) 		C2(12) 
// Paragraph 1 	 	
    C1(03)       								C2(05) 
	02:E 02:0       							C2(08) 
	<81 <82 02:1 02:0       					C2(06) 
	02:6A03E2AE 01:0 02:18 01:0 <82 C1(03)      C2(12) 
	
//------------------------------------ File: Text_3_Paragraps_13.analyse.txt
03:0007000000B200000004 	 		// Paragraphs Definitions
01:0 
// Paragraph 0
    <81 <81       							C2(0F) 
	<81 <82 02:1 02:0       				C2(06) 
	02:6A03E2AE 01:0 02:18 01:0 <82 C1(03) 	C2(12) 
// Paragraph 1
	01:1 01:0 <81 	      					C2(0F) 
	<81 <82 02:1 02:0 						C2(06) 
	02:6A03E2AE 01:0 02:18 01:0 <82 C1(03)  C2(12)  	// Tab stops with only default Tab width of 24px      
// Paragraph 2	  
      C1(03)
		02:120 											// 288,	Margin Left  
		02:168  										// 360,	Margin Right 
		02:1C  											// 28,	Indent from Left margin 
		02:0								C2(03) 
	02:9 02:7 02:0 							C2(05)		// Link to 9,7,0
	<81 <82 02:1 02:0       				C2(06) 
	02:6A03E2AE 01:0 02:18 01:0 <82 C1(03)  C2(12) 		// Tab stops with only default Tab width of 24px  
//------------------------------------	


- C2(0F) 			C2(06) C2(12) 
- C2(03) C2(05) 	C2(06) C2(12)
- C2(05) C2(08)		C2(06) C2(12)


## XMED Tokens & Block Structure (revised, neutral version)

> **Note:** Nothing below is called a “fact” unless confirmed by a direct action in Director’s UI that changes the same bytes.  
> Entries are marked as **ASSUMPTION** (likely interpretation) or **OBSERVATION** (repeated pattern only).

---

## 1) Tokens & Separators

| Byte(s) | Status | Description |
|---|---|---|
| `01` | **OBSERVATION** | Starts a literal ASCII-hex value; often used for short constants or toggles. |
| `02` | **OBSERVATION** | Starts a numeric ASCII-hex value (may include `2D` `'-'` for negative). Appears in size, spacing, and header data. |
| `81` | **OBSERVATION** | Separator between sub-fields inside one property (e.g., between RGB parts). |
| `82` | **OBSERVATION** | Terminator for a composite property group. |

---

## 2) Structural Markers

| Byte(s) | Status | Description |
|---|---|---|
| `C1 xx` | **OBSERVATION** | Opens a property/run/paragraph segment. |
| `C2 yy` | **OBSERVATION** | Closes the most recent open block; subtype (`yy`) varies with context. |
| `C3 yy` | **NOT SEEN** | No occurrences found in provided samples. |

---

## 3) Common C1→C2 Block Pairs

| Open → Close | Status | Internal Token Pattern | Working Hypothesis |
|---|---|---|---|
| `C1 03 → C2 07 / 0A` | **OBSERVATION** | Sequence: `VAL VAL VAL` (RGB) → `NUM` → small numerics | Likely a core **style run** for a text range. |
| `C1 04 → C2 20` | **OBSERVATION** | `NUM (NUM?)` | Appears related to **alignment** or paragraph layout. |
| `C1 05 → C2 04 / 05` | **ASSUMPTION** | Several `NUM` tokens | Possibly tab/spacing settings. |
| `C1 0A → C2 12` | **OBSERVATION** | `VAL VAL` | Appears to toggle superscript. |
| `C1 0B → C2 13` | **OBSERVATION** | `VAL VAL` | Appears to toggle subscript. |
| `C1 1C → C2 13 / 0F` | **OBSERVATION** | `VAL VAL` | Appears to toggle underline or strikeout. |
| `C1 03 → C2 03` | **ASSUMPTION** | Several `NUM` tokens near file start | Possible **header preamble** section. |
| `C1 20 → C2 07 / 0A / 20` | **ASSUMPTION** | Few or no numerics | Possible paragraph or grouping wrapper. |

---

## 4) Alignment & Flags

| Item | Status | Notes |
|---|---|---|
| First numeric inside `C1 04 → C2 20` | **ASSUMPTION** | May indicate alignment mode (`0=center`, `1=right`, `2=left`, `3=justify`). |
| Additional small numerics | **ASSUMPTION** | Possibly wrap or tab options, not yet mapped. |

---

## 5) Decoration Toggles

| Decoration | Block | Status | Internal Shape |
|---|---|---|---|
| Superscript | `C1 0A → C2 12` | **OBSERVATION** | `01 33` then `01 30`. |
| Subscript | `C1 0B → C2 13` | **OBSERVATION** | `01 33` then `01 30`. |
| Underline / Strike | `C1 1C → C2 13 / 0F` | **OBSERVATION** | `01 31` then `01 30`. |

---

## 6) Run Map Records

| Element | Status | Description |
|---|---|---|
| 20-character ASCII-hex entries (`0004…0129`) | **OBSERVATION** | Describe text slices and link them to style IDs. |
| Offset / Length fields | **ASSUMPTION** | Likely mark start/length of visible text spans. |
| Type / StyleId | **ASSUMPTION** | Identify which C1→C2 block applies to that run. |

---

## 7) Color Encoding (inside style runs)

| Pattern | Status | Description |
|---|---|---|
| `01 <RR...> 81 81  01 <GG...> 81 81  01 <BB...> (82)` | **OBSERVATION** | RGB sequence; 81 separates channels; 82 ends the composite. |
| Channel length | **ASSUMPTION** | Each color value appears as 16-bit hex words (not yet verified for endianness). |

---

## 8) Header Preamble Perhaps (early C2 blocks)

| Field order | Status | Description |
|---|---|---|
| First few `02` values | **OBSERVATION** | Appear before any style runs; may contain schema/version and base address info. |
| Repeating `02` numerics | **ASSUMPTION** | Could include offsets, dimensions, or baseline defaults. |
| `C2 03` | **OBSERVATION** | Appears at start and sometimes mid-stream; acts as a section boundary, not necessarily a terminator. |

_No field in this section has yet been confirmed to match any Director UI property (e.g., width, height, baseline)._

---

## 9) Unobserved or Uncertain Areas

| Topic | Status | Notes |
|---|---|---|
| Any `C3` markers | **NOT SEEN** | No example across all test files. |
| Decimal-encoded numbers | **NOT SEEN** | All numeric fields look hexadecimal so far. |
| Proven mapping to Director UI controls | **NOT CONFIRMED** | Pending controlled UI experiments. |

---

## 10) Current Research Questions

1. Which field in the header corresponds to editable / locked state of the text member?  
2. Which numeric pair controls line spacing or paragraph spacing?  
3. Are there per-paragraph structures nested under `C1 20` that correspond to alignment settings?  
4. What exactly do `C2 07`, `C2 0A`, and `C2 20` delimit semantically (soft vs. hard block ends)?  

---

### Summary

At this stage, all interpretations are **tentative** until verified by live Director edits.  
Use these patterns for decoding structure and token boundaries only — **not** for semantic binding to UI fields.
