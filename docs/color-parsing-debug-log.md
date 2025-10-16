# Color Parsing Debug Log

This log tracks approaches we attempted while investigating the failing XMED text color scenarios. Each entry lists the method we exercised, the outcome, and why the attempt failed so future passes can pick up where we left off.

## 2024-07-14 Investigation Pass

### `XmedFileTest.Multi_Text_color_samples_should_BeRead`
- **How we exercised it:** Ran `dotnet test Test/BlingoEngine.IO.Legacy.Tests/BlingoEngine.IO.Legacy.Tests.csproj --filter "FullyQualifiedName~Text_color"` to reach the multi-color scenario.
- **What happened:** The test threw a `FileNotFoundException` while calling `ReadDocument("MemberTests/Text_Multi_Style_Size_Color_13.bin")`.
- **Why it failed:** The asset path omits the `.xmed` portion of the filename. The fixture directory only contains `MemberTests/Text_Multi_Style_Size_Color_13.xmed.bin`, so the loader is pointing at a file that does not exist.
- **Follow-up ideas:** Update the fixture reference (either rename the asset or adjust the test/read helper) so the parser loads the real binary. Once the document loads we can inspect whether colors propagate into `XmedDocument.Styles` and runs.

### `XmedFileTest.Text_color_samples_should_BeRead`
- **How we exercised it:** The same filtered `dotnet test` command hits this theory after the multi-color fact aborts. We also inspected the method directly.
- **What happened:** The theory currently stops at `var doc = ReadDocument(...)` without validating any channels.
- **Why it failed:** The method is unfinished (`// TODO`). Without assertions we cannot confirm whether `BlXmedTextReader` is decoding the foreground color correctly, so the investigation stalls here.
- **Follow-up ideas:** After fixing the asset path above, add assertions that read `doc.Styles` (or runs) and verify `ColorIndex`/`ResolveColor` to confirm actual RGB values.

### `BlXmedTokenStyleParser.ReadStyles`
- **How we exercised it:** Reviewed the implementation to understand where color data should land while debugging the test failures.
- **What happened:** The parser only assigns `current.Color` when it encounters a `C1(0x04)` token through `reader.TryGetColor(out var col)`. In our captures the color changes are emitted via indexed palette references (field 2) rather than inline RGB tokens, so `current.Color` never updates from `_activeColor`.
- **Why it failed:** Styles that rely on palette indices do not map through to actual RGB values, which likely explains why earlier attempts to read colors returned defaults. The TODO tests would still fail even after the asset path fix unless we translate palette indices into concrete `BlLegacyColor` values.
- **Follow-up ideas:** Investigate how palette lookups are supposed to happen (see `XMED_Declaration.md` and token logs). We may need to hydrate `_document.Palette` and resolve `ColorIndex` to RGB when finalizing styles.

### `BlXmedTokenReader.GetColorComponents`
- **How we exercised it:** Traced the call when reviewing `ReadStyles` to see whether inline RGB tokens work.
- **What happened:** The helper only returns values if the stream emits a composite `C1(0x04)` followed by component tokens. That path is not triggered by palette-index style tokens, so our sample documents skip it entirely.
- **Why it failed:** Without inline RGB tokens, `GetColorComponents` produces an empty list and the parser falls back to `_activeColor` (which defaults to palette entry `0`).
- **Follow-up ideas:** Confirm via token logs whether palette-driven colors should fall through to `C1(0x05)` or similar composite records. If so, expand `GetColorComponents`/`ReadStyles` to read the palette entries and feed them into `XmedStyleDescriptor.Color`.

### Command Logging Notes
- The `dotnet test` invocation printed a `TerminalLogger` `ArgumentOutOfRangeException` after the `FileNotFoundException`. This appears to be a known SDK console logger quirk and did not mask the real failure, but re-running with `DOTNET_CLI_UI_LANGUAGE=en` or `DOTNET_CLI_TELEMETRY_OPTOUT=1` may keep the output cleaner on future passes.

## 2024-07-15 Investigation Pass

### `XmedFileTest.Multi_Text_color_samples_should_BeRead`
- **How we exercised it:** Pointed the fixture at `MemberTests/Text_Multi_Style_Size_Color_13.xmed.bin` and re-ran the filtered unit tests with verbose console logging (`dotnet test ... --logger "console;verbosity=detailed" --filter FullyQualifiedName~Multi_Text_color`).
- **What happened:** The test now reads the document and prints the captured run diagnostics. The only run reported `style 0`, `colorIndex <null>`, `inline #000000`, and `resolved #000000` even though the sample text describes three differently colored spans.
- **Why it failed:** No style descriptors were materialized for the colored spans—the run fell back to the base style (`styleId = 0`). Because `ResolveColor` only consults `ColorIndex` or `_activeColor`, the run inherits the default black color instead of the RGB values encoded in the file.
- **Follow-up ideas:** Trace how `XmedRunSliceBuilder` populates `_runBoundaries` for style IDs > 0. We likely need to inspect the `03:0006` style table tokens and ensure `BlXmedTokenStyleParser` hydrates both the palette index and the inline RGB triple so runs can bind to the correct descriptor.

## 2024-07-16 Investigation Pass

### Instrumenting `03:0006` style tokens and run boundaries
- **How we exercised it:** Augmented `BlXmedTokenStyleParser` and `XmedRunSliceBuilder` with diagnostic logging, then re-ran `dotnet test Test/BlingoEngine.IO.Legacy.Tests/BlingoEngine.IO.Legacy.Tests.csproj --filter "FullyQualifiedName~Multi_Text_color" --logger "console;verbosity=detailed"` to capture the token stream for `MemberTests/Text_Multi_Style_Size_Color_13.xmed.bin`.
- **What happened:** The logger shows only one style table entry (`style 3`) being visited. The token sequence inside the `03:0006` block is `00:0` (parent id) followed by two field separators and then three successive `PrefixedHex` values `00:C`, `00:3`, and `00:0`. Each value overwrites the descriptor's `ColorIndex`, so by the time the block terminates the color index has been reset to `0x00`. No inline `C1(04)` color composite emitted components for that style. At the run layer, the diagnostics never record a `pending end` boundary, leaving `XmedRunSliceBuilder` with only a single slice mapped to `style 0`. 【7f1445†L1-L40】
- **Why it failed:** Our parser treats the first post-style-id token as field `1`, but the log shows Director emits the parent style id before any field separator, so we drop the inheritance link entirely. The repeated field-2 values suggest Director encodes multiple palette indices (or palette+something else) in the same slot; because we always keep the last value, the descriptor falls back to black. Without resolved style ids, the run slicer never sees usable boundaries and defaults to `style 0`.
- **Follow-up ideas:**
  - Adjust the style parser so field `0` captures the parent id and handle multi-value color slots (possibly the first non-zero entry should win instead of the last).
  - Inspect the raw `03:0004` block and confirm why the run slicer never observes `02:<end>` pairs—the `pending end` log never fires, implying our reader is skipping the numeric tokens entirely. Extracting a token dump for `03:0004` should reveal whether we're missing a composite wrapper or using the wrong reader in `ParseBody`.

## 2024-07-17 Investigation Pass

### Token windows for `03:0004` and `03:0006`
- **How we exercised it:** Added a `DumpTokenWindows` helper to `XmedFileTest.Multi_Text_color_samples_should_BeRead` that tokenizes `MemberTests/Text_Multi_Style_Size_Color_13.xmed.bin` and logs focused windows around the `03:0004` run map and `03:0006` style table entries. Re-ran `dotnet test Test/BlingoEngine.IO.Legacy.Tests/BlingoEngine.IO.Legacy.Tests.csproj --filter "FullyQualifiedName~Multi_Text_color" --logger "console;verbosity=detailed"` to capture the expanded dump.
- **What happened:**
  - The `03:0004` window shows alternating `02:<end>` and `01:<styleId>` pairs for five boundaries (e.g., `0x14 → style 1`, `0x28 → style 0`, `0x2A → style 2`, `0x40 → style 0`, `0x5A → style 0`, `0x5D → style 0`). The data confirms the run map is present even though `XmedRunSliceBuilder` collapses everything into a single `style 0` slice.
  - The `03:0006` window begins with `01:3` and `01:0` tokens prior to the first field separator, indicating the parent style id lives in field `0`. After two separators, the field-two slot emits `01:C`, `01:3`, and `01:0`, matching our earlier observation that we overwrite the non-zero palette index with zero.
  - Immediately after the style block, nested `C1(03)` composites surface `01:F700`, `01:2000`, and `01:4A00` tokens—these encode the inline RGB triple for the red sample, but our parser ignores them because it only probes `C1(04)` composites. 【19bdfb†L41-L125】
- **Why it failed:** The raw token dump proves that both the run map and inline colors exist in the binary, but the parser never binds them because it: (a) treats the parent id as field `1` instead of field `0`; (b) overwrites the first non-zero color index with trailing zeros; and (c) only calls `TryGetColor` for `C1(04)` composites, missing the `C1(03)` color payload that follows the style entry.
- **Follow-up ideas:**
  - Teach `ReadStyles` to finalize each entry when the depth unwinds and to capture field `0` as the parent id before consuming separators.
  - When parsing field `2`, preserve the first non-zero palette index and track the full candidate list so we can reconcile palette/background slots later.
  - Extend the color reader to process the `C1(03)` composite that follows the style entry—`TryGetColor` likely needs a variant that understands both 0x03 and 0x04 composites so inline RGB triples reach `XmedStyleDescriptor.Color`.
  - Reconcile the `03:0004` map with `_runBoundaries`: the data shows five explicit endpoints, so instrument the builder to dump the `_runBoundaries` list and diagnose why `BuildRunSlices` fails the length check and falls back to `style 0`.

## 2024-07-18 Investigation Pass

### `XmedRunSliceBuilder.ReadRuns`
- **How we exercised it:** Ran the multi-color regression test with the new block preview logging enabled to watch how the slice builder walks `03:0004` tokens.
- **What happened:** The preview confirms the reader sees the entire `03:0004` payload (offset/style pairs), but the loop exits immediately because the block header token itself trips the `IsBlockBoundary` guard. No boundary pairs ever make it into `_runBoundaries`.
- **Why it failed:** `ReadRuns` was written for inline `C1(04)` composites and treats any `03:*` token as a boundary, so the real run map is skipped outright.
- **Follow-up ideas:** Teach the reader to consume the initial `03:0004` header and then parse the alternating `02`/`01` tokens so `_runBoundaries` reflects the run map before falling back to the single-style slice.

### `BlXmedTokenStyleParser.ReadStyles`
- **How we exercised it:** Observed the enhanced logging while running the multi-color test; the parser now records the `C1(03)` composite that follows the style entry.
- **What happened:** The preview shows the RGB triplet (`F700`, `2000`, `4A00`) and the sentinel zeros/`FFFF` tokens that surround it, confirming the inline color is present even though `TryGetColor` ignores `C1(03)`.
- **Why it failed:** Without a handler for `C1(03)` composites the parser never hydrates `XmedStyleDescriptor.Color`, so the runs resolve to the default black.
- **Follow-up ideas:** Extend the color reader so `C1(03)` composites feed into the descriptor, and verify whether the surrounding `FFFF`/`0` tokens represent alpha or flags we need to preserve.


## 2024-07-19 Investigation Pass

### `XmedRunSliceBuilder.ReadRuns`
- **How we exercised it:** Replaced the C1(04)-specific loop with a `ReadRunMapEntries` helper that consumes the `03:0004` header and walks the alternating `02:<end>` / `01:<styleId>` pairs while running `dotnet test Test/BlingoEngine.IO.Legacy.Tests/BlingoEngine.IO.Legacy.Tests.csproj --filter "FullyQualifiedName~Multi_Text_color" --logger "console;verbosity=detailed"`.
- **What happened:** The logger now captures every endpoint/style pair (e.g., `20 → style 1`, `42 → style 2`, `66 → style 4`) before the reader exits. Despite the richer diagnostics, `FinalizeRunsAndParagraphs` still produces a single slice mapped to `style 0`, so the rendered run remains black. 【e3c596†L13-L74】【e3c596†L91-L118】
- **Why it failed:** The collected boundaries never survive into the slice builder—either a later pass clears `_runBoundaries` or the normalization logic discards the entries before we assign them to the run map. We need an additional trace inside `FinalizeRunsAndParagraphs` (or an assertion in the test) to confirm whether `_runBoundaries` is empty by the time slices are built and, if so, who clears it.
- **Follow-up ideas:** Emit the `_runBoundaries` contents right before the normalization pass, then inspect `BuildRunSlices` to understand why it collapses to a single `style 0` segment even when the map contains multiple endpoints.

### `BlXmedTokenStyleParser.ReadStyles`
- **How we exercised it:** Taught the parser to treat field 0 as the parent id, lock onto the first non-zero palette index, track styles with inline colors, and call the updated `TryGetColor` helper for both `C1(03)` and `C1(04)` composites. The same focused test run drove the logging.
- **What happened:** The parser now records `color index 0x0C` for style `3`, but `inline <null>` persists and `ResolveColor` continues to output `#000000`. The logs show the inline RGB payload arriving in a second `C1(03)` composite immediately after the field terminator, yet the loop finalizes the style before that composite is processed. 【e3c596†L58-L118】
- **Why it failed:** When the reader encounters the first `C1(03)` (which is effectively empty) it advances, then the following field terminator unwinds the depth and finalizes the style. The trailing `C1(03)` that carries `F700/2000/4A00` tokens never runs through `TryGetColor`, so `inlineColorRead` remains `false`. We need to keep the style context alive long enough to consume the post-terminator composite.
- **Follow-up ideas:** After detecting a field terminator, peek ahead for trailing `C1(03)` composites tied to the just-closed style and parse them before clearing `current`. Alternatively, delay finalization until all immediate composites are consumed or maintain a queue of pending inline color composites per style id.

### `BlXmedTokenReader.TryGetColor`
- **How we exercised it:** Added a composite-id switch that treats `C1(03)` payloads as 16-bit little-endian RGB values (only capturing tokens whose numeric value exceeds `0xFF`). The helper still returns the first three components as bytes and hands them back to the style parser.
- **What happened:** The helper never fires because the style reader finalizes before the color composite is read, so the resolved run remains black in spite of the new conversion logic. 【e3c596†L91-L118】
- **Why it failed:** Filtering out `<= 0xFF` tokens prevents palette metadata from polluting the color tuple, but the upstream parser does not request those values yet. Once `ReadStyles` keeps the composite in scope, the conversion should start returning non-zero RGB bytes.
- **Follow-up ideas:** After fixing the style-finalization timing, add a one-off log inside `TryGetColor` to print the decoded RGB triple so we can confirm the helper returns `#F7204A` and similar hues before we wire the result into `ResolveColor`.

## 2024-07-20 Investigation Pass

### `XmedRunSliceBuilder.ReadRuns`
- **How we exercised it:** Continued running `dotnet test Test/BlingoEngine.IO.Legacy.Tests/BlingoEngine.IO.Legacy.Tests.csproj --filter "FullyQualifiedName~Multi_Text_color" --logger "console;verbosity=detailed"` while watching the new run-map logs.
- **What happened:** Restricting `_runBoundaries.Clear()` to the actual `03:0004` block stopped later `C1(04)` composites from blowing away the map. The normalized trace now lists every `(end, style)` pair and `BuildRunSlices` produces seven slices instead of collapsing into a single `style 0` segment.
- **Why it still fails:** The slices exist, but the later color resolution still picks the base descriptor because the referenced styles do not have foreground colors yet.
- **Follow-up ideas:** Keep the map logic as-is and focus on feeding real colors into the non-zero style ids so run construction can emit the expected RGB values.

### `BlXmedTokenStyleParser.ReadStyles`
- **How we exercised it:** Spiked a peek-ahead implementation that tried to keep the just-closed style alive long enough to absorb the trailing `C1(03)`/`C1(04)` composites logged in earlier passes.
- **What happened:** The prototype proved that Director emits an empty `C1(03)`, a `C1(04)` sentinel, and then a `C1(03)` carrying 16-bit RGB components. Unfortunately, keeping that logic inside the main loop let the parser confuse inline-color tokens with actual style identifiers, yielding bogus descriptors (`style 63232`) and dropping the legitimate font entries.
- **Why it failed:** The style reader needs a dedicated phase for trailing composites; interleaving them with the primary field loop makes it too easy to lose track of where the style entry ends and the color payload begins.
- **Follow-up ideas:** Finalize the style immediately when the terminator arrives, then delegate to a helper that scans the subsequent tokens for `C1(03)`/`C1(04)` payloads before allowing the next `03:0006` entry to begin.

## 2024-07-21 Investigation Pass

### `BlXmedTokenReader.TryGetColor`
- **How we exercised it:** Re-ran the multi-color regression test after teaching the helper to tolerate early `B_82` terminators and to stop after three inline components while walking `C1(03)` composites.
- **What happened:** The reader now captures the 16-bit RGB triples that follow the sentinel `C1(04)` block. The log shows the expected `#F7204A` inline color flowing out of the second `C1(03)` payload instead of bailing on the first empty composite.
- **Why it still falls short:** Subsequent composites in the same trail mention additional style ids (`01:1`, `01:2`) but the helper still assumes the just-finalized style id, so those RGB triples never reach the Tahoma or Terminal descriptors.
- **Follow-up ideas:** Teach the trailing composite parser to inspect the inline payload for a target style id and dispatch the decoded color to that descriptor instead of pinning everything to the caller's style.

### `BlXmedTokenStyleParser.ReadStyles`
- **How we exercised it:** Added a dedicated `ConsumeTrailingInlineColors` phase that skips non-color composites, probes each `C1(03)`/`C1(04)` payload, and logs the decoded RGB values before returning to the main loop.
- **What happened:** Style `3` now records the inline `#F7204A` color emitted by `Text_Multi_Style_Size_Color_13.xmed.bin`, confirming that the sample actually stores the red swatch as a trailing `C1(03)` composite rather than inside the primary style fields.
- **Why it still fails:** The helper always targets the style that triggered it, so the later composites that describe the green and blue runs (style ids `1` and `2`) are still ignored. The run map therefore resolves everything back to the base descriptor even though the color data is now visible in the token dump.
- **Follow-up ideas:** Detect when a trailing `C1(03)` payload starts with `01:<styleId>`/`01:0` pairs, retarget the color assignment to that style, and leave breadcrumbs in the logs so we can verify that styles `1`, `2`, and `4` also pick up their inline RGB triples.

## 2024-07-22 Investigation Pass

### `BlXmedTokenParser` body readers
- **How we exercised it:** Reworked the parser so the body is processed in sequential slices (`ReadTextSection`, `ReadRunSection`, `ReadStyleSection`, `ReadFooterSection`) and re-ran `dotnet test Test/BlingoEngine.IO.Legacy.Tests/BlingoEngine.IO.Legacy.Tests.csproj --filter "FullyQualifiedName~Multi_Text_color" --logger "console;verbosity=detailed"` to ensure the new flow still walks the sample without throwing.
- **What happened:** The text block now consumes the font table and raw characters before yielding to the run reader, which in turn stops as soon as the `03:0006` style table begins. The style reader then processes trailing composites before the footer skips the remaining `03:0008` payloads. The logs confirm that each stage advances monotonically through the token stream.
- **Why it still fails:** Even with deterministic block ordering, the inline color composites past the style table continue to target multiple style ids. Until we decode those intra-footer hints, only style `3` gains an inline color and the other runs still inherit black.
- **Follow-up ideas:** Extend the footer pass to dispatch `01:<styleId>` references to the style parser so that inline RGB triples emitted after the table can hydrate styles `1`, `2`, and `4` without breaking the sequential read contract.
