# XMED Token Log — Reading Guide

This document defines a **neutral, assumption‑free** notation for inspecting XMED bytes via a compact token log. It is meant for **comparison and investigation**, *not* for round‑tripping or asserting semantics.

---

## 1) Token Categories

### A. Control‑prefixed ASCII
- **Form:** `NN:VALUE`
- **Meaning:** `NN` is a single control byte in hex (e.g., `01`, `02`, `03`). `VALUE` is the literal ASCII payload that immediately follows in the file until the next token begins.
- **Notes:**
  - `VALUE` may look numeric (decimal/hex/signed) but is logged **verbatim**.
  - Examples: `01:77AA`, `02:40001`, `03:00000000005C00000000`.

### B. Booleans
- **Source bytes:** `01 31` → `true`, `01 30` → `false`.

### C. Tag Bytes
- **Form:** `C1(xx)`, `C2(xx)`, `C3(xx)`
- **Meaning:** Literal tag byte (`C1`,`C2`,`C3`) followed by a 1‑byte **type** `xx` in hex.
- **Payload:** Any payload after these tags is logged by subsequent tokens (not inside the tag token).

### D. Perhaps Link Markers
- **Form:** `<81`, `<82`
- **Meaning:** Raw bytes `81` / `82`. Perhaps “link/relate” to the previous logical token. No transformation applied.

### E. 0x00 Blocks (Text or Structured Bytes)
- **Generic Form:** `00(perhaps len):…`
- **Perhaps Declared length:** The decimal number **as written in the file** immediately after `0x00`, then a ASCII comma. Len is perhaps not a len but a controlbyte
- **Two variants are logged:**

1. **Text block**
   - **Form:** `00(perhaps len):"…"`
   - **Meaning:** A block that is logged as ASCII text **verbatim**, including embedded newlines.
   - **Use:** Free text content (paragraphs, labels, etc.).
   - **Note:** The logger does **not** enforce `len`. len is perhaps not a len but a controlbyte; it preserves the file’s bytes and prints the text exactly as read until the next token begins.

2. **Numbers/Tail block**
   - **Form:** `00(perhaps len):b0,b1,b2,…`
   - **Meaning:** Hex bytes printed as a **comma‑separated list**. Typically used by the **final** 0x00 block (e.g., run codes).

### F. Font Name Pair (Observed Pattern)
- **Form:** successive `00(40):"Name"` followed by a fixed 64‑byte style tail (zero‑filled in observed files).
- **Purpose:** Logged as two consecutive tokens (first shows the name, second is a zeroed tail captured implicitly by advancing the cursor). This preserves on‑disk order for diffs.

> **Important:** All of the above are **descriptive logs**, not claims about Director/XMED semantics. Use them to line up bytes across files.

---

## 2) Line Breaking & Spacing

- **New lines** are started for:
  - Tag tokens `C1/C2/C3`
  - `00(...)` blocks
  - Long sequences exceeding the configured token‑per‑line limit (for readability)
- Inside a line, items are separated by a single space.
- Quoted text (`"..."`) may contain newlines; these are preserved exactly.

---

## 3) Examples (from real logs)

```
00:FFFF0000000600040001 01:77AA  03:00000000005C00000000  02:40001  02:101  02:-7FFF6FE0  02:0 
C2(03) 02:480048  02:-1  02:0  02:140  false
C1(03) 02:-1  <82  02:0  02:5  02:0  02:5  02:0 
...
00(108):"My first paragraph centered with all 0
Paragraph with align Left, Margin Left 4, Margin Right 5, First Indent 0.4inch Spacing Before 9, spacing after 7
Paragraph with align Left, Margin Left 1, Margin Right 2, First Indent 0.3inch Spacing Before 4, spacing after 5"
...
00(40):"Arial"
00(40):""
...
00(44):45,46,182,181,149,181,165,165,46,39,34,145,146,147,148,133,131
```

---

## 4) How to Compare Two Files with This Log

- Generate the token logs for both files.
- Diff the text outputs:
  - **Control groups** (`NN:VALUE`) reveal structural changes.
  - **Tag lines** (`C1/C2/C3`) show where typed records differ.
  - **Text blocks** (`00(len):"..."`) surface content edits and line breaks.
  - **Number tails** (`00(len):a,b,...`) spotlight run/code list differences.
- Avoid interpreting the **meaning** of numbers or tags in this document—use it only to **locate** and **describe** byte differences consistently.

---

## 5) Guarantees & Non‑Goals

- **Guarantees**
  - Order is preserved.
  - Every token corresponds to an exact span of bytes.
  - ASCII text is logged verbatim.
  - Numeric lists are losslessly represented as hex bytes (CSV).

- **Non‑Goals**
  - No claims about semantics (runs, colors, styles).
  - No enforcement that `len` equals any measured data length.
  - No round‑trip promises. This is a **viewer/log format**.

---


