# XMED Token Log — Reading Guide

This document defines the neutral, byte-for-byte notation produced by the
`BlXmedTokenizer`. The log is intended for **comparison and investigation** – it
does not attempt to reverse the meaning of the data. Every token maps to the
exact bytes emitted by Director and is emitted in read order.

---

## 1) Token Categories

### A. Prefixed ASCII/hex spans
- **Form:** `NN:VALUE`
- **Source:** Control bytes `01`, `02`, or `03` followed by printable ASCII.
- **Interpretation:** The prefix byte becomes `NN`; the trailing ASCII payload is
  written exactly as found. When the payload happens to represent a number it is
  still logged verbatim. (`BlXmedTokenizer.Tokenize` builds these tokens and
  stores the parsed numeric value for later readers.)【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenizer.cs†L33-L88】

### B. Padding composites (`C1` / `C2` / `C3`)
- **Form:** `C1(xx)`, `C2(xx)`, `C3(xx)`
- **Meaning:** Raw bytes `C1`, `C2`, or `C3` followed by a 1-byte type value.
  The token records the opener; any payload that follows is emitted as separate
  tokens. Composite payloads are expanded later when the parser inspects the
  stream.【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenizer.cs†L90-L121】

### C. Repeat / terminator markers (`81` / `82`)
- **Form:** `<81`, `<82`
- **Meaning:** Literal bytes `0x81` and `0x82`. The tokenizer keeps them as
  stand-alone markers so run readers can recognise field separators and
  terminators.【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenizer.cs†L123-L139】【F:src/BlingoEngine.IO.Legacy/Texts/Data/BlXmedToken.cs†L60-L72】

### D. 0x00 blocks
- **Form:** `00(N):…`
- **Behaviour:** When a `0x00` byte is encountered the tokenizer captures the
  comma-delimited payload as a single block. The prefix number `N` is logged as
  written; no validation is performed. Two patterns are common:
  1. `00(N):"…"` – ASCII text emitted verbatim (paragraph content, font names).
  2. `00(N):a,b,c,…` – comma-separated byte list, typically the tail table at
     the end of the block.【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenizer.cs†L143-L207】

### E. ASCII spans
- **Form:** quoted text inside the log without a prefix.
- **Meaning:** Printable characters (0x20–0x7E) that are not part of another
  control structure. The tokenizer writes them as a single token so diffs can
  focus on literal strings.【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenizer.cs†L209-L224】

### F. Raw bytes
- **Form:** single tokens that carry the numeric value.
- **Meaning:** Any byte that does not match the patterns above is logged as a
  `Byte` token to keep positional information intact.【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenizer.cs†L226-L232】

> **Note:** The tokenizer no longer labels values as `true`/`false`. Consumers
> resolve booleans by reading the numeric payload (usually `02:0` or
> `02:1`).【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenReader.cs†L115-L164】

---

## 2) Layout of the log

- Each token is separated by a single space unless readability would suffer; in
  that case the dumper inserts a newline (`DumpTokensUltraCompact`).【F:src/BlingoEngine.IO.Legacy/Texts/BlXmedTokenizer.cs†L246-L327】
- Composite markers (`C1/C2/C3`), repeat markers, and `00(…)` blocks always
  start on a fresh line to keep boundaries visible.
- Text blocks keep their original newlines so multi-line paragraphs remain easy
  to read.

---

## 3) Example excerpt

```
00:FFFF0000000600040001 01:77AA
03:00000000004F00000000 02:40001 02:101 02:-7FFD6FE0 02:0
  C2(03) 02:480048 02:-1 02:0 02:18 01:0
  C1(03) 02:-1 <82 02:0
  C2(0A) 02:18 02:B2 01:FF00 <81 <81 01:0 <82 <82 02:1 02:0
  C2(04) 02:5 01:0
03:00020000000900000000
  00(5):"Hallo"
```

The snippet above matches the default `Text_Hallo` sample. Notice that numeric
values such as `02:1` are kept verbatim – downstream code decides whether they
represent flags, booleans, or dimensions.

---

## 4) Using the logs for comparisons

1. Generate token logs for the files you want to diff.
2. Compare them with a standard text diff tool.
3. Focus on the token groups:
   - Prefixed entries (`NN:…`) highlight structural changes.
   - `C1/C2` lines show composite records (colours, style descriptors, etc.).
   - `00(…):"…"` exposes edited text.
   - `00(…):a,b,…` lists help track font-table or run tables changing length.
4. Defer semantic interpretation to the dedicated parser (`BlXmedTokenParser`)
   which translates the tokens into styles, fonts, and paragraph metadata.

---

## 5) Guarantees & non-goals

**Guaranteed by the logger**
- Token order matches file order.
- Each token covers a precise byte span (start offset + length).
- Text payloads are emitted verbatim, including embedded newlines.
- Numeric lists preserve the raw bytes without reformatting.

**Non-goals**
- No attempt is made to validate declared lengths inside `00(…)` blocks.
- The logger does not infer meaning from numeric payloads.
- The format is one-way; it is not suitable for rebuilding the binary stream.

---
