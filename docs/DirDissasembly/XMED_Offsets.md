# XMED Offsets & Structures (Consolidated)

> Scope: **inside XMED text/field members only** (no CASt generalities). This file merges header pointers, text/run maps, font tables, and bit‑flag bytes. Values are derived from the provided test set and validated notes.

---

## 1) Header Directory (ASCII)
The XMED header starts with ASCII **directory rows**: STILL WRONG
```
<TYPE:4>,<OFFSET:8>,<COUNT:8>
```
- All three numbers are **hex strings** (ASCII).  
- Rows continue **until the first 0x00 byte** (no explicit header length).  
- Example: `0008,000005B0,00000010` → Type **0008** at offset **0x05B0** with **16** entries capacity. STILL WRONG


---
blocks starting with 
0x00 [XX XX: is the length of the content] [COMMA]
- First block is the text
- Next blocks are always the font name
- The latest : We always get these values in the latest blok
	045,046,182,181,149,181,165,165,046,039,034,145,146,147,148,133,131 	


### Known TYPEs (from tests) 
- **0008** → **Font‑names table** (“40,” records).STILL WRONG

> Parser rule: read directory rows → `DirEntry { Type, Offset, Count }`. Use `Count` as upper bound; do **not** scan past it.

---

## 2) Text Block (the text itself)
- At the end of the header there is an **ASCII length** for the text block (example bytes at **[240–244]**: `00 31 32 41 2C` → `"12A,"` → **0x12A = 298**).  
- **Text starts at 245** and spans that length (includes CR separators `0x0D` for multi‑line).

> Parser rule: read ASCII length, compute `textStart = 245`, slice `[textStart .. textStart+length)`. No scanning.

---

## 3) Font‑Names Table (TYPE=0008)
- Jump to OFFSET from the directory entry.  
- Structure repeats **COUNT** times:
  - ASCII **`"40,"`** (`34 30 2C`),
  - **1 byte** name length **L**,
  - **L** bytes ASCII font name,
  - optional **NUL padding** before next record.

> Empty slots have length **00**. Iterate exactly **COUNT** records.

---

## 4) Run / Style Map (20‑digit ASCII rows)
Multi‑style files contain a map of **ASCII hex digits**, grouped into 5 fields (4 digits each):
```
F1 F2 F3 F4 F5   (total 20 hex digits)
```
- **F3** = **run length** (chars/bytes for the text run).  
- **F5** = **style descriptor id** (used to select a descriptor block).

> Parser rule: read 20‑digit rows at the map location (from header/known offsets), convert each field from ASCII hex to `ushort`.

---

## 5) Style Descriptor Blocks
Found later in the file for multi‑style texts; each block carries:
- Style/flag bytes (same layout as below),
- Font link (font id/name),
- Optional color index and metric overrides (size/spacing).

Descriptors are referenced by **F5** in the run map. Single‑style files still use the same structure but only one descriptor is effective.

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

## 7) Metrics & Pointers (fixed offsets in member header)

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

---

## 8) End‑to‑End Parse (reference points)
- **Header** → `ReadDirectory()` until `0x00` → entries `{Type,Offset,Count}`.  
- **Text** → read ASCII length (e.g., at 240–244) → slice at 245 for `length` bytes.  
- **Fonts** → locate TYPE `0008` → iterate **COUNT** `"40,"` records.  
- **Run map** → read 20‑digit rows → `F3=length`, `F5=descriptorId`.  
- **Descriptors** → jump by id → read style/align, color index, font link.  
- **Flags** → bytes `0x001C`/`0x001D` give styles + alignment (bitwise).

---

## 9) Validation Hints
- Cross‑check line lengths from run map (`F3`) with actual text CR‑split lengths.  
- Font names discovered at TYPE `0008` must match names referenced by descriptors.  
- For alignment: decode `0x001D & 0b11` and verify Justified (=3) when present.

---

## 10) Known Good Samples (from this set)
- Multi‑line, multi‑style with verified lengths: **38, 70, 72, 70, 44**.  
- Font table example at **0x05B0**: `"40,05,Arial"` followed by slots (COUNT=16).  
- Alignment combined values observed: `00, 10, 15, 1A, 19, 03`.





[ 02 30 02 <font size key> 01 ] [ 02 ?? ?? <line height key> 01 ]


*Status legend*: Confirmed = stable across files; Confirm = needs more dedicated single‑feature files (time‑boxed).
