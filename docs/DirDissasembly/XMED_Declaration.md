# XMED_Declaration.md 

## XMED Log Format (intro)

This document uses the **token log** format.
Tokens are printed in read order.

- `00(some number):"text"` → literal text block.
- `01:xxxx` → literal/value.
- `02:xxxx` → number (often twips or offsets).
- `C1(xx)` → Padding, number of '0' values
- `<81` → Repeat last token value, if new field then 00, and defines a new value/component
- `82` → end of current block/struct.

Full specification: [XMED_Token_Log_Guide.md](XMED_Token_Log_Guide.md).

### Token grouping hierarchy

The grouper emits four nested levels so the debug dump mirrors the `.analyse.txt` snapshots:

1. **Blocks (`03:xxxx`)** – detected from the 12-digit header and annotated with the declared entry count.
2. **Structures** – payload slices separated by `<82` markers.
3. **Field segments** – inner collections for style, paragraph, and font payloads.
4. **`C2` sequences** – each `C2(nn)` token becomes its own group and captures every token until the next `C2`, a `C1` marker, or another terminator.

Comments in the dump now include short descriptions (e.g. `C2(06)` → tab stops, `C2(0A)` → font link).

### Fixed-length payloads

Some `C2` groups expose a constant number of numeric slots. The grouper normalizes those payloads by trimming excess padding and inserting zero tokens when the source omits trailing values.

| Tag | Expected Count | Notes |
|-----|----------------|-------|
| `C2(07)` | 4 | Bold, italic, underline, strikethrough boolean flags. `<81>` repeat markers are expanded so each slot is explicit. |
| `C2(0A)` | 2 | Font slot reference (`fontIndex`, `0`). |

## To investigate C2 values in Font styles:

| C2(Tag) | Example Occurrences | Confirmed / Suspected Meaning | Notes | Found In Block |
|----------|---------------------|-------------------------------|--------|----------------|
| **C2(0F)** | seen in older files | Unknown | Placeholder-like behaviour | **Header** |
| **C2(0A)** | `C2(0A) 02:5 02:0` | Font Link (Font Slot / Index) | Always follows `02:<fontID> 02:0` | **Run Styles / Fonts Block** |
| **C2(04)** | `C2(04) 02:55 01:0` | Possibly Text Length / Tab Info | Appears near run or paragraph metrics | **Paragraph Definitions** |
| **C2(06)** | `C2(06) 02:6A03E2AE 01:4 ...` | Tab Stop List | First number = tab count, followed by `02:1 02:<pos>` pairs | **Paragraph Definitions** |
| **C2(0B)** | rare, `C2(0B)` | Possibly paragraph justification flags | Only seen in long text blocks, unconfirmed | **Paragraph Definitions** |
| **C2(0D)** | rare, `C2(0D)` | Unknown metric (padding?) | Always isolated, no data | **Paragraph Definitions** |
| **C2(20)** | `C2(20)` | Possibly paragraph block header | Seen around large run sets | **Paragraph Definitions** |
| **C2(23)** | `C2(23)` | Unknown, single occurrence | May mark paragraph group boundary | **Paragraph Definitions** |
| **C2(26)** | `C2(26)` | Unknown | Possibly internal reference ID | **Paragraph Definitions** |
| **C2(07)** | `C2(07) 01:1 <81 <81 01:0` | Font Style Flags (Bold/Italic/Underline) | Also stores alignment and spacing info | **Run Styles** |
| **C2(13)** | `C2(13)` | Continuation / Line-wrap marker | Found when a style restarts mid-text or new line | **Run Styles** |
| **C2(0E)** | `C2(0E)` | Unknown (possibly spacing-related) | Similar area as (0A)/(07) | **Run Styles** |
| **C2(0C)** | `C2(0C) 02:400 02:0` | Width / pitch or scaling | Seen in font entries, may hold Win32 lfWidth | **Fonts Block** |
| **C2(03)** | Font blocks → `C2(03) 02:101 01:0` | Script / Charset ID | Always ends font entry, e.g. `257 = Western` | **Fonts Block** |



# Main blocks
Identifier: 03:00020000013000000000 
Starts with 03, then 
- 0002    : BlockType 
- 0000130 : Length
- 0000    : items in the block’s payload

  :FFFF0000000600040001     // Header
03:00020000013000000000     // Block Full Text 
03:00040000002900000008     // Run styles
03:00050000001F00000006     // Run paragraphs 
03:00070000004D00000002 		// Paragraphs Definitions
03:00080000020A00000003			// Fonts  Block 
03:00090000001500000002 	  // Line-height spacing descriptor
03:000A0000001500000002 	  // Identical as Line-height spacing

## Run styles — 03:0004 (required)
starting with 02:0
Payload: alternating pairs `styleId endOffset`.

03:00040000002900000008 	// Run styles
  02:0 
		01:6 02:26 	 	// Style=6, 38,	"This text is... 	red		,... 	Arial,12px, centered" 		
		01:5 02:27 	 	// Style=5, 39,	"This text is... 	???		,...	Arial,12px, centered\r"  
		01:9 02:6D 	 	// Style=9, 109,"This text is... 	Yellow	...		bold, italic, underline"  
		01:7 02:6E 	 	// Style=7, 110,"This text is... 	Yellow	...		bold, italic, underline\r"   
		01:8 02:B7 	 	// Style=8, 183,"This text is... 	green	...		aligned, with spacing of 39"   
		01:A 02:FE 	 	// Style=10,254,"This text is... 	orange	...		bold, italic, underline"   
		01:6 02:12C 	// Style=6, 300,"This text is... 	red		...		centered again"   
		01:6 

## Run Paragraphs		
03:00050000001F00000006 	// Run paragraphs  
  02:0 
		01:1 02:27 	 	// Run=1, 39, "This text is... 	red		,... 	Arial,12px, centered" 	 
		01:0 02:6E 	 	// Run=0, 110,"This text is... 	Yellow	...		bold, italic, underline\r" 
		01:2 02:B7 	 	// Run=2, 183,"This text is... 	green	...		aligned, with spacing of 39"  
		01:0 02:FE 		// Run=0, 254,"This text is... 	orange	...		bold, italic, underline"
		01:1 02:12C 	// Run=1, 300,"This text is... 	red		...		centered again"
		01:0 


## Paragraphs
03:00070000004D00000002 		// Paragraphs 
- Struct 1
      01:0 
      <81 
      <81 
      C2(0F) <81 
    <82                     // End of struct  
- Struct 2      
      02:1 02:0 
      C2(06) 02:6A03E2AE 01:0 02:18 01:0  						// Tab stops with only default Tab width of 24px 
    <82                     // End of struct  
- Struct 3    
      C1(03)                 // padding 3 unknown values          
      C2(12) 




## Style blocks — 03:0006 (required)

03:00060000012B00000005 	// Block Styles
- Struct 1
    01:0 
    <81 
    <81 
    01:C 										// Font Descent 
    01:3 										// Font Ascendent
    01:0 
    <81 
  <82                     // End of struct
- Struct 2
	<82                     // End of struct 
- Struct 3  
    C1(04) 								  // Foreground RGBA: Black #000000
    01:FFFF <81 <81 01:0 	  // Background RGBA : White #FFFFFF
  <82                     // End of struct
- Struct 4
	  0000 02:0  				    // 12,	Font-size
    C2(0A) 02:6 02:0      // Font style index
    C2(07)                // Font style : bold, italic, underline
    C1(20)                // padding 20 unknown values
  <82                     // End of struct 
- Struct 5       
  <82                     // End of struct
- Struct 6        
    C1(03)                 // padding 3 unknown values
  <82                     // End of struct





### Colors 
Are always 4 values : RGBA
Examples:
- 01:F700 01:2000 01:4A00 01:0 	// Red       #F7204A
- 01:2700 01:200 01:FD00 01:0   // Blue: 	  #2702FD
- 01:1E00 01:F000 01:2E00 01:0 	// Green :   #1EF02E
- 01:FF00 01:0 <81 <81 				  // red       #FF0000
- 01:FF00 <81 01:0 <81 				  // Yellow    #FFFF00
- <81 01:FF00 01:0 <81 			    // Green     #00FF00
- 01:FF00 01:9900 01:0 <81 		  // Orange    #FF9900
- C1(04) 						    	      // Black     #000000    Pad 4 x 0 values


### Identified Style Bits
C2(07) 01:1 01:0		 		    // Bold	  
C2(07) <81 01:1 01:0	 		  // Italic  
C2(07) <81 <81 01:1 01:0 		// Underline
C2(07) 01:1 <81 <81 01:0 		// Bold, Italic, Underline	


### Font Size <- to validate yet
- Actual point size is stored per style in `03:0006`.  
- Sometimes the header repeats an approximate pixel value (`pt × 1.333 × 10`).  
- Conversion back to points:  
  ```
  pt ≈ headerValue / 13.33
  ```

### Tab stops

#### No tab stops defined:
  C2(06) 02:6A03E2AE 01:0 02:18 01:0 <82  
Description:
  - C2(06)      : C2 definition Fix
  - 02:6A03E2AE : Identifier
  - 01:0        : Number of defined styles
  - 02:18       : Default Tab width 24 px
  - 01:0        : 
  - <82        : End of struct

  #### With Tabs defined
C2(06) 02:6A03E2AE 01:4 
Description:
  - 02:1 02:96 02:0 <82   // Tab Stop Left 150 px
  - 02:1 02:D8 02:0 <82   // Tab Stop Left 216 px
  - 02:1 02:120 02:0 <82  // Tab Stop Left 288 px
  - 02:1 02:169 02:0 <82  // Tab Stop Left 361 px
  - 02:18 01:0 <82        // Default Tab width 24 px



## Header 
`00:FFFF0000000600040001 01:77AA 03:0000000000XX00000000 …`

TODO










## Font Table — 03:0008 (required)
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





## Tail — 00(44) Seems
Always present and identical in samples:
`00(44):45,46,182,181,149,181,165,165,46,39,34,145,146,147,148,133,131`
Treat as a global lookup/palette table.





## 📐 Text Box Size (Width & Height)





## 🧾 Paragraph Layout: Margins, Indents & Spacing

### Location
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
Stored in **`C2(03)`** right after each margin group:
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



