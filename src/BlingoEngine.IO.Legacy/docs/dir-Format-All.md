# Director Movie (`.dir`) Container

[← Back to Docs Home](README.md)

## Overview

Classic Director movies store their data in a RIFF-style container. Most exports use the big-endian `RIFX` tag, while little-endian bundles spell the signature backwards (`XFIR`) or use the standard `RIFF`/`FFIR` pair. The top-level chunk announces the archive subtype with a four-character code (`MV93`, `MC95`, `APPL`, `FGDM`, or `FGDC`) that decides how the resource map is parsed.

## Detecting the container

### Raw exports and plain archives

Raw `.dir` files start directly with one of the container signatures:

| Bytes | Meaning |
| --- | --- |
| `52 49 46 58` (`RIFX`) / `58 46 49 52` (`XFIR`) | Big-endian vs. little-endian Director movie. |
| `52 49 46 46` (`RIFF`) / `46 46 49 52` (`FFIR`) | Little-endian RIFF movie bundle. |

Once these four bytes are read, the loader can treat the remainder as a RIFX/RIFF stream and jump to the header described in [RIFX header and subtype](#rifx-header-and-subtype).

### Windows projectors

Self-contained Windows executables wrap the movie with a short projector header. The bytes immediately preceding the RIFX data identify the Director generation and provide the offset to the embedded archive.

#### Director 2–3 projectors

Older projectors keep an external movie list before the RIFX payload. The format repeats for every entry:

| Field | Length | Notes |
| --- | --- | --- |
| Entry count | 2 bytes (little-endian) | Number of MMM entries that follow. |
| Unknown padding | 5 bytes | Reserved bytes skipped by the loader. |
| MMM size | 4 bytes per entry (little-endian) | Stored size for each MMM file. |
| MMM filename | Pascal string | Length-prefixed filename for the MMM resource. |
| Directory name | Pascal string | Length-prefixed directory containing the MMM file. |

After the directory listing, the reader seeks to the embedded RIFF/RIFX data.

#### Director 4 projectors (`PJ93`)

| Field | Length | Notes |
| --- | --- | --- |
| `50 4A 39 33` (`PJ93`) | 4 bytes | Signature identifying a Director 4 projector. |
| RIFX offset | 4 bytes (little-endian) | File position of the embedded movie. |
| Font map offset | 4 bytes (little-endian) | Pointer to the bundled font map. |
| Resource fork offset 1 | 4 bytes (little-endian) | First stored resource-fork pointer. |
| Resource fork offset 2 | 4 bytes (little-endian) | Second stored resource-fork pointer. |
| Graphics DLL offset | 4 bytes (little-endian) | Location of the graphics helper DLL. |
| Sound DLL offset | 4 bytes (little-endian) | Location of the sound helper DLL. |
| Alternate RIFX offset | 4 bytes (little-endian) | Duplicate copy of the movie offset. |
| Projector flags | 4 bytes (little-endian) | Flag word preserved for compatibility. |

The reader skips these fields and then seeks to the recorded RIFX offset.

#### Director 5 projectors (`PJ95`)

| Field | Length | Notes |
| --- | --- | --- |
| `50 4A 39 35` (`PJ95`) | 4 bytes | Signature identifying a Director 5 projector. |
| RIFX offset | 4 bytes (little-endian) | File position of the embedded movie. |
| Projector flags | 4 bytes (little-endian) | Primary projector bitfield. |
| Window flags | 4 bytes (little-endian) | Secondary window-behaviour flags. |
| Window X | 2 bytes (little-endian) | Window X coordinate. |
| Window Y | 2 bytes (little-endian) | Window Y coordinate. |
| Window width | 2 bytes (little-endian) | Stored window width in pixels. |
| Window height | 2 bytes (little-endian) | Stored window height in pixels. |
| Component count | 4 bytes (little-endian) | Number of bundled projector components. |
| Driver file count | 4 bytes (little-endian) | Count of driver filenames that follow. |
| Font map offset | 4 bytes (little-endian) | Pointer to the font map data. |

The embedded movie begins at the recorded offset immediately after this header.

#### Director 7 projectors (`PJ00` / `PJ01`)

| Field | Length | Notes |
| --- | --- | --- |
| `50 4A 30 30` (`PJ00`) or `50 4A 30 31` (`PJ01`) | 4 bytes | Signature identifying a Director 7-era projector. |
| RIFX offset | 4 bytes (little-endian) | File position of the embedded movie. |
| Reserved dword 1 | 4 bytes (little-endian) | Unused header word. |
| Reserved dword 2 | 4 bytes (little-endian) | Unused header word. |
| Reserved dword 3 | 4 bytes (little-endian) | Unused header word. |
| Reserved dword 4 | 4 bytes (little-endian) | Unused header word. |
| DLL offset | 4 bytes (little-endian) | Pointer to the bundled DLL. |

After skipping the reserved dwords, the reader jumps to the RIFX offset.

### Mac projectors

Macintosh projectors also begin with a `PJ**` header when compiled for PowerPC. The four-byte signature (`PJ93`, `PJ95`, or `PJ00`) is followed by a big-endian 32-bit offset to the embedded RIFX data. 68k exports omit the header and start with `RIFX`/`XFIR`, so the archive begins at offset zero.

## RIFX header and subtype

Immediately after the signature, the top-level chunk follows the standard RIFF layout:

| Field | Length | Notes |
| --- | --- | --- |
| `RIFX` / `XFIR` / `RIFF` / `FFIR` | 4 bytes | Container signature. |
| Chunk size | 4 bytes | Unsigned 32-bit size of the entire movie chunk (header + payload). |
| Archive type | 4 bytes | Determines the map format: `MV93`, `MC95`, or `APPL` for memory-map movies; `FGDM` or `FGDC` for Afterburner/ Shockwave movies. |

The byte order depends on the signature (`RIFX` is big-endian, `XFIR` is little-endian).

## Memory-map movies (`MV93`, `MC95`, `APPL`)

Classic Director movies expose two control blocks before the resource entries.

### `imap` header

| Field | Length | Notes |
| --- | --- | --- |
| `69 6D 61 70` (`imap`) | 4 bytes | Memory-map prologue tag. |
| `imap` length | 4 bytes | Size of the `imap` block. |
| Map version | 4 bytes | Observed values: `0` or `1`. |
| `mmap` offset | 4 bytes | File position of the resource table. |
| Archive version | 4 bytes | Director release marker (`0x00000000`, `0x000004C1`, `0x000004C7`, `0x00000708`, `0x00000742`, `0x00000744`). |

The observed archive versions map to specific Director generations. Director MX 2004 (the final "Director 10" release) reuses
the same map layout as the Director 10 entry below, so no additional parsing rules are required.

| Value | Director release |
| --- | --- |
| `0x00000000` | Director 4 |
| `0x000004C1` | Director 5 |
| `0x000004C7` | Director 6 |
| `0x00000708` | Director 8.5 |
| `0x00000742` | Director 10 / Director MX 2004 |
| `0x00000744` | Director 10.1 |

### `mmap` resource table

| Field | Length | Notes |
| ----- | --- | --- |
| `6D 6D 61 70` (`mmap`) | 4 bytes | Resource table signature. |
| `mmap` length | 4 bytes | Size of the `mmap` chunk. |
| Header size | 2 bytes | Number of bytes between the `mmap` header and the first entry. |
| Entry size | 2 bytes | Size of each resource row. |
| Total entries | 4 bytes | Count of allocated rows. |
| Filled entries | 4 bytes | Number of populated rows. |
| Padding | 8 bytes | All `0xFF`; separates the header from the freelist pointer. |
| First free resource id | 4 bytes | Index of the first unused slot (`-1` when none). |

Each table row then contributes:

| Field | Length | Notes |
| --- | --- | --- |
| Resource tag | 4 bytes | Four-character chunk type. |
| Resource size | 4 bytes | Payload length excluding the 8-byte RIFX sub-header. |
| Resource offset | 4 bytes | File offset of the chunk within the archive. |
| Flags | 2 bytes | Attribute bitfield copied from Director. |
| Unknown | 2 bytes | Placeholder that mirrors Director's layout. |
| Next free index | 4 bytes | Chains unused rows inside the freelist. |

## Chunk layout

Each resource chunk inside the archive follows the RIFF convention:

| Field | Length | Notes |
| --- | --- | --- |
| Chunk tag | 4 bytes | Four-character identifier such as `CASt`, `Lscr`, or `KEY*`. |
| Chunk length | 4 bytes | Number of bytes that follow. |
| Payload | `length` bytes | Resource data. Memory-map readers typically skip the 8-byte sub-header before handing the payload to higher-level parsers. |

The resource map marks each chunk as it is accessed so tooling can report unused entries when the archive closes.

[← Back to Docs Home](README.md)