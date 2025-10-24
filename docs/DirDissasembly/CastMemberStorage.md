# Cast Member Storage in `.cst` Files

The observations below summarize what is currently understood about how Director stores cast members inside `.cst` files.  Points marked as *probable* are based on limited samples and still need confirmation.

## Container structure

* A `.cst` file is a **RIFX** container.  The samples seen so far use little‑endian byte order.
* After the header, an **`imap`** chunk provides the offset of the **`mmap`** chunk.
* The **`mmap`** (memory map) lists every chunk in the file.  Each entry records the FourCC, byte length and file offset of a chunk.

## Locating members

* The **`CAS*`** chunk contains the list of cast‑member IDs.  This is confirmed by inspecting the `CAS*` entry in multiple sample files.
* For each ID, the corresponding **`CASt`** entry in the memory map gives the byte offset and length for that member.  This behaviour is consistent across all test casts.

## `CASt` chunk layout

A `CASt` chunk begins with a 12‑byte header:

| Offset | Field            | Notes |
|-------:|-----------------|-------|
| 0      | `Type`          | Identifies the member type (e.g., `0x0F` for a field member). |
| 4      | `InfoLen`       | Length of the cast‑info block in bytes. |
| 8      | `SpecificDataLen` | Length of the member's data block in bytes. |

* After the header, `InfoLen` bytes form the **cast‑info block** (`Cinf`).  This block is itself a list of offsets pointing to variable‑length fields such as the member name or script text.
* The **specific data block** immediately follows the info block and contains the raw member data (e.g., text characters, bitmap pixels, sound samples).


## Member Info Bytes (Cinf Offsets)

| Offset | Field / Feature            | Type / Enum                        | Values / Meaning                                    |
|:-------:|:---------------------------|:-----------------------------------|:----------------------------------------------------|
| `0x44`  | **CastInfoFlags**          | `CastInfoFlags`                    | Bit 2 = DTS Off (0 = DTS On, 1 = Off)               |
| `0x46`  | **TextFraming**            | `TextFraming`                      | 0 = Fixed, 1 = Scrolling, 2 = AdjustToFit           |
| `0x8E`  | **AntiAlias + Kerning**    | `TextAntiAlias`, `TextKerningMode` | Shared byte — lower nibble = AA, upper nibble = Kerning |
| `0xBA`  | **AntiAlias Threshold**    | `byte`                             | Point size threshold (for AA LargerThan mode)       |
| `0xCE`  | **Kerning Enable Bit**     | `bool`                             | Bit 0 = Kerning enabled                             |
| `0xD2`  | **Kerning Threshold**      | `byte`                             | Point size threshold (for Kerning LargerThan mode)  |
| `0xD6`  | **TextFeatureFlags**       | `TextFeatureFlags`                 | Bit 0 = UseHyperlinkStyles                          |
| `0xE6`  | **InkType**                | `BlingoInkType`                    | Matches Director ink enum (0–41 range)              |
| `0xA2`  | **TextMemberFlags**        | `TextMemberFlags`                  | Bit 0 = IsEditable                                  |

## Text Framing and Kerning Modes

| Enum                | Value | Meaning / Description                  | Source Offset |
|:--------------------|:------|:---------------------------------------|:--------------|
| **TextFraming**     | 0x00  | Fixed (no scrolling or resizing)       | `Cinf + 0x46` |
|                     | 0x01  | Scrolling text box                     | `Cinf + 0x46` |
|                     | 0x02  | AdjustToFit (auto-size to content)     | `Cinf + 0x46` |
| **TextKerningMode** | 0x40  | None (no kerning adjustments)          | `Cinf + 0x8E` |
|                     | 0x30  | AllText (kerning applied globally)     | `Cinf + 0x8E` |
|                     | 0x70  | LargerThan (apply if font > threshold) | `Cinf + 0x8E` |
| **Kerning Enable**  | Bit 0 | 1 = enabled, 0 = disabled              | `Cinf + 0xCE` |
| **Kerning Threshold** | byte | Point-size limit for LargerThan mode  | `Cinf + 0xD2` |


## XMED styled‑text information

* For text and field members that carry styling, an **`XMED`** chunk supplies font and colour runs.  The link between a `CASt` chunk and its `XMED` data is recorded in the key table (`KEY*`) and memory map.
* It is *probable* that each styled text member references exactly one `XMED` chunk.  Some casts include two copies of the `XMED` data; the second copy appears to be the one referenced by the memory map, but this needs further verification.

## Extracting member data

To extract a member's raw bytes and metadata:

1. Read the `imap` chunk to locate the `mmap`.
2. Iterate the `mmap` to build a table of FourCC, offset and length.
3. Read the `CAS*` chunk to obtain cast‑member IDs.
4. For each ID, look up its `CASt` entry in the memory map to get the byte range.
5. Within the `CASt` data:
   * Start of info block = `chunkOffset + 8 + 12` (8 for list header, 12 for `CASt` header).
   * Start of specific data = `infoStart + InfoLen`.
   * End of member data = `specificStart + SpecificDataLen`.
6. If an `XMED` chunk is referenced, parse it for styled text information.

These steps accurately reproduce the offsets and lengths observed in the sample casts, but additional testing is required to ensure they hold for all files.

