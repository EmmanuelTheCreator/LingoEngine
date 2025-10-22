# XMED_Declaration.md 

## XMED Log Format (intro)

This document uses the **token log** format.
Tokens are printed in read order.

- `00(some number):"text"` → literal text block.
- `01:xxxx` → literal/value.
- `02:xxxx` → number (often twips or offsets).
- `03:xxxx` → Block declarations
- `C1(xx)` → Padding, number of '0' values
- `C2(xx)` → padding, number of 'NULL' values
- `<81` → Repeat last token value, if new field then 00, and defines a new value/component
- `82` → NULL value or default value

Full specification: [XMED_Token_Log_Guide.md](XMED_Token_Log_Guide.md).



# Main blocks
Identifier: 03:00020000013000040003 
Starts with 03, then 
- 0002    : BlockType 
- 0000130 : unknown Value, perhaps length
- 0004    : unknown Value
- 0003    : items in the block’s payload
``` 
  :FFFF0000000600040001     // Header
03:00020000013000000000     // Block Full Text 
03:00040000002900000008     // Run styles
03:00050000001F00000006     // Run paragraphs 
03:00070000004D00000002 	// Paragraphs Definitions
03:00080000020A00000003		// Fonts  Block 
03:00090000001500000002 	// Line-height Paragraph Bounds
03:000A0000001500000002 	// Identical as Paragraph Bounds
03:000B0000000500000002  	// Some static List of 8 values
03:000C0000002A00000002   	// Paragraph format records
```


---


## FFFF / 0000 - Header 
`00:FFFF0000000600040001 01:77AA 03:0000000000XX00000000 …`

TODO


---

## 0001 - Layout 


TODO


---

## 03:0004 - Run styles 
starting with 02:0
Payload: alternating pairs `styleId endOffset`.
``` 
03:00040000002900000008 	// Run styles
  02:0 
		01:6 02:26 	 	// Style=6, 38,	"This text is... 	red		,... 	Arial,12px, centered" 		
		01:5 02:27 	 	// Style=5, 39,	"This text is... 	???		,...	Arial,12px, centered\r"  
		01:9 02:6D 	 	// Style=9, 109,"This text is... 	Yellow	...		bold, italic, underline"  
		01:7 02:6E 	 	// Style=7, 110,"This text is... 	Yellow	...		bold, italic, underline\r"   
		01:8 02:B7 	 	// Style=8, 183,"This text is... 	green	...		aligned, with spacing of 39"   
		01:A 02:FE 	 	// Style=10,254,"This text is... 	orange	...		bold, italic, underline"   
		01:6 02:12C 	// Style=6, 300,"This text is... 	red		...		centered again"   
		01:6          // Empty style
```

---


## 03:0005 - Run Paragraphs	
``` 
03:00050000001F00000006 	// Run paragraphs  
  02:0 
		01:1 02:27 	 	// Run=1, 39, "This text is... 	red		,... 	Arial,12px, centered" 	 
		01:0 02:6E 	 	// Run=0, 110,"This text is... 	Yellow	...		bold, italic, underline\r" 
		01:2 02:B7 	 	// Run=2, 183,"This text is... 	green	...		aligned, with spacing of 39"  
		01:0 02:FE 		// Run=0, 254,"This text is... 	orange	...		bold, italic, underline"
		01:1 02:12C 	// Run=1, 300,"This text is... 	red		...		centered again"
		01:0          // Empty run
``` 

---


## 03:0006 - Style blocks 
A style block has 77 tokens when all padding has been extracted.
``` 
03:00060000012B00000005 	// Block Styles
    01:0 
    <81 
    <81 
    01:C 										// Font Descent 
    01:3 										// Font Ascendent
    01:0 
    <81 
  <82
	<82 
    C1(04) 								  // Foreground RGBA: Black #000000
    01:FFFF <81 <81 01:0 	  // Background RGBA : White #FFFFFF
  <82
	  0000 02:0  				    // 12,	Font-size
    C2(0A) 
    02:6 02:0             // Font style index
    C2(07)                // Font style : bold, italic, underline
    C1(20)                // padding 20 unknown values
  <82 
  <82
    C1(03)                 // padding 3 unknown values
  <82
```

### Colors 
Are always 4 values : RGBA
Examples:
``` 
- 01:F700 01:2000 01:4A00 01:0 	// Red       #F7204A
- 01:2700 01:200 01:FD00 01:0   // Blue: 	  #2702FD
- 01:1E00 01:F000 01:2E00 01:0 	// Green :   #1EF02E
- 01:FF00 01:0 <81 <81 				  // red       #FF0000
- 01:FF00 <81 01:0 <81 				  // Yellow    #FFFF00
- <81 01:FF00 01:0 <81 			    // Green     #00FF00
- 01:FF00 01:9900 01:0 <81 		  // Orange    #FF9900
- C1(04) 						    	      // Black     #000000    Pad 4 x 0 values
``` 

### Identified Style Bits
``` 
C2(07) 01:1 01:0		 		    // Bold	  
C2(07) <81 01:1 01:0	 		  // Italic  
C2(07) <81 <81 01:1 01:0 		// Underline
C2(07) 01:1 <81 <81 01:0 		// Bold, Italic, Underline	
``` 

### Font Size <- to validate yet
- Actual point size is stored per style in `03:0006`.  
- Sometimes the header repeats an approximate pixel value (`pt × 1.333 × 10`).  
- Conversion back to points:  
  ```
  pt ≈ headerValue / 13.33
  ```
  
---


## 03:0007 - Paragraphs
They are composed of 28 tokens and then the tab stop identifier is there:
02:6A03E2AE
folowing by the number tab stops. They are *4 tokens, and then there is the default tab stop and then a padding
In total : 50 tokens + 4 +(4 * tab_stop_count)
``` 
03:00070000004D00000002 		// Paragraphs 
      01:0 
      <81 
      <81 
      C2(0F) <81 
    <82 
      02:1 02:0 
      C2(06) 02:6A03E2AE 01:0 02:18 01:0  						// Tab stops with only default Tab width of 24px 
    <82 
      C1(03)                 // padding 3 unknown values          
      C2(12) 
``` 



### Tab stops

#### No tab stops defined:
02:6A03E2AE 01:0 02:18 01:0 <82  
Description:
``` 
  - 02:6A03E2AE : Identifier
  - 01:0        : Number of defined styles
  - 02:18       : Default Tab width 24 px
  - 01:0        : 
  - <82         : NULL
```

  #### With Tabs defined
 02:6A03E2AE 01:4 
Description:
``` 
  - 02:1 02:96 02:0 <82   // Tab Stop Left 150 px
  - 02:1 02:D8 02:0 <82   // Tab Stop Left 216 px
  - 02:1 02:120 02:0 <82  // Tab Stop Left 288 px
  - 02:1 02:169 02:0 <82  // Tab Stop Left 361 px
  - 02:18 01:0 <82        // Default Tab width 24 px
```

#### With different tab stop types
02:6A03E2AE 01:5 
Description:
``` 
		02:1 02:63 02:0 <82     // Type 1 : Left align
		02:3 02:31D 02:0 <82    // Type 3 : Right Align
		02:2 02:190 02:0 <82    // Type 2 : Center Align  
		02:4 02:1F4 02:0 <82    // Type 4 : Decimal align
		02:4 02:257 02:0 <82    // Type 4 : Decimal align
		02:18 01:0 <82 
```


A decimal tab aligns numbers by their decimal point.
When you type numeric values in text (like 12.3, 4.56, 789.0), a decimal tab ensures that all the . (decimal points) line up vertically—so digits before and after stay neatly aligned.
It’s a long-standing feature in word processors (Word, PageMaker, Director’s text engine).
In your files, the 02:4 type marks that tab stop as decimal-aligned, used when displaying columns of numbers.










---



## 03:0008 - Font Table
Each entry begins with a pair of `00(40)` strings (font family + style name) followed by a fixed set of numeric fields. Tokens are split by `<82` separators, with `<81` repeating the previous value.

### Parsed descriptor fields

| Field | Notes |
|-------|-------|
| `TableIndex` | Declared slot in the font table. |
| `FontId` | Raster/vector identifier (`0x60FF` for Terminal). |
| `CodePage` | Windows code page; resolved to an `Encoding` via `Encoding.GetEncodings()` (for example `0x4E4 → windows-1252`). |
| `PitchFlags` / `FamilyClass` | Low-byte projection of `lfPitchAndFamily` surfaced as enums (`Fixed`, `Variable`, `Swiss`, `Modern`, …). |
| `PitchDecorations` | Remaining high bits, currently observed as `0x40000` for underline/italic hints. |
| `ScriptId` | Matches the trailing `C2(03)` payload within the font entry. |

`CodePagesEncodingProvider` is registered lazily inside `XmedFontDescriptor`, so legacy encodings are available even if the hosting application has not registered the provider beforehand.

```
00(40):"FontName"      ← Family
00(40):"Style"         ← Style name (empty = Regular)
01:<index>             ← Table index used by style records
01:<0> … 01:<fontId>   ← OEM/raster font identifier (0 for vector fonts, 0xFF60+ for Terminal)
02:4E4                 ← Windows code page (1252 = Western Latin)
02:400                 ← LOGFONT weight (400 = normal)
02:0                   ← Flags (reserved in observed samples)
02:1                   ← Font kind marker (1 = scalable/vector)
02:<cellHeight>        ← Raster cell height (0 for vector fonts, 0xFF for Terminal)
02:40008               ← LOGFONT pitch & family bits (0x00040008 = variable pitch, Swiss)
02:0                   ← Reserved slot (always 0 so far)
02:101 / C2(03)…       ← Script identifier (257 = Western/Latin I)
01:0                   ← Name index (references the inline string, currently 0)
```

Values were confirmed against `Test/TestData/Legacy/Texts_Fields/Text_Multi_Line_Multi_Style_13.analyse.txt`, which includes the full table with the Terminal raster font (FontId `0x60FF` and cell height `0xFF`).

---




## 03:0009 / 03:000A — 🧩 Paragraph Bounds

**Type:** Layout geometry list (identical structure)  
**Form:** `02:0 <82 02:X 02:Y 02:0 <82 02:X 02:Y` × paragraphCount  

| Field | Meaning | Notes |
|--------|----------|-------|
| `X` | Baseline / top offset | Scales with line-height or margin |
| `Y` | Paragraph width / right edge | Matches text span width |
| Count | Paragraph count | One tuple per paragraph |

**Interpretation:**  
Defines bounding boxes per paragraph; X encodes vertical spacing (line-height derived), Y the visual width.  
No explicit line-height token exists — it’s inferred from these bounds.  

---




## 03:000B - Some static List of 8 values 
03:000B always holds 8 total values — one literal 0 followed by 7 NULLs (82) — acting as a static padding or reset field block.


---






## 03:000C — Paragraph format records

**Observations (from your logs):**
- Structure = records of: `02:<S> <82 <82> 02:<A> 02:<B> 01:<f> [..optional..]` × count.  
- Count varies (1–2). Values change with **alignment** and **margins**.

**Correlations:**
- **Left**: `02:0 <82 <82> 02:F 02:4 01:0 … 01:1`. 
- **Right**: `02:5 <82 <82> 02:E 02:77 01:0 02:3B <82 01:1`. 
- **Center**: `02:0 <82 <82> 02:9 02:7 01:0 … 01:1`.
- **LineHeight 18/36**: count=2, includes constants `02:E3`, `02:6F`. 

**Conclusion:** 03:000C encodes per-paragraph formatting (alignment/indents/margins)

### Structure (per paragraph)
```
02:<S> <82 <82> 02:<A> 02:<B> 01:<f> …
```
| Symbol | Meaning | Notes |
|---------|----------|-------|
| `<S>` | Paragraph index | Increases sequentially per paragraph |
| `<A>` | Left or right margin | Changes with alignment and margin tests |
| `<B>` | Width / bounding span | Matches paragraph box width from 03:0009 |
| `<f>` | Flags byte | 0=normal, 1=justify or alignment flag |

**Link:**  
The `<S>` field directly maps to paragraph order — `S=0` → first paragraph, `S=1` → second, etc.  
Each record configures that paragraph’s layout (margins, width, alignment).  


---

### Tail — 00(44) Seems
Always present and identical in samples:
`00(44):45,46,182,181,149,181,165,165,46,39,34,145,146,147,148,133,131`
Treat as a global lookup/palette table.




---


## 📐 Text Box Size (Width & Height)





## 🧾 Paragraph Layout: Margins, Indents & Spacing

### Location
C2 is padding
Paragraph metrics appear mainly in the **`C2(03)`** and **`03:000C`** blocks.



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
Stored in **`C2(03)`** right after each margin group: C2 is padding
```
C2(03) 02:9  02:7  02:0
```
→ `9` = spacing before, `7` = spacing after (in points or twips).

### Summary Table
| Property | Source Block | Example | Unit | Description |
|-----------|---------------|----------|-------|--------------|
| Left Margin |  | `02:120` | twips | Paragraph left offset |
| Right Margin | | `02:168` | twips | Paragraph right offset |
| First Indent | | `02:1C` | twips | Indent for first line |
| Spacing Before |  | `02:9` | pt/twips | Space above paragraph |
| Spacing After |  | `02:7` | pt/twips | Space below paragraph |


## 🔠 Kerning & Character Spacing

### Location observations
C2 is padding
Kerning and extra letter spacing appear in the **`C2(03)`** and **`C2(04)`** sections following the header.

### Pattern
C2 is padding
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



