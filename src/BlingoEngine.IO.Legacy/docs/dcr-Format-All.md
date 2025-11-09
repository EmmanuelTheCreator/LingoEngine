# Shockwave Movie (`.dcr`) Container

[← Back to Docs Home](README.md)

## Overview

Shockwave exports use the same RIFF foundation as classic movies but switch the archive type to `FGDM` or `FGDC`. These identifiers signal the "Afterburner" layout: the resource table is compressed, offsets are encoded with variable-length integers, and selected resources may be preloaded into memory.

## Top-level structure

The first twelve bytes match the standard RIFX layout:

| Field           | Length  | Notes                                                              |
| --------------- | ------- | ------------------------------------------------------------------ |
| `RIFX` / `XFIR` | 4 bytes | Container signature (`RIFX` = big-endian, `XFIR` = little-endian). |
| Chunk size      | 4 bytes | Total length of the movie chunk, including the header.             |
| Archive type    | 4 bytes | `FGDM` or `FGDC` identifies an Afterburner archive.                |

The fields that follow are specific to Afterburner and appear in the order below.

## `Fver` – Afterburner version header

| Field                  | Length    | Notes                                                                            |
| ---------------------- | --------- | -------------------------------------------------------------------------------- |
| `46 76 65 72` (`Fver`) | 4 bytes   | Afterburner version tag.                                                         |
| Encoded length         | 1–5 bytes | Variable-length integer describing how many bytes belong to the version payload. |
| Encoded version        | 1–5 bytes | Variable-length integer that records the Afterburner build number.               |

The length byte tells the parser how many bytes to consume when decoding the version number.

## `Fcdr` – compression descriptor

| Field                  | Length         | Notes                                                                         |
| ---------------------- | -------------- | ----------------------------------------------------------------------------- |
| `46 63 64 72` (`Fcdr`) | 4 bytes        | Descriptor tag.                                                               |
| Encoded length         | 1–5 bytes      | Variable-length integer giving the number of bytes in the descriptor payload. |
| Descriptor payload     | `length` bytes | Compression descriptor bytes. The loader skips them after reading the length. |

## `ABMP` – compressed resource map

Afterburner compresses the resource metadata into the `ABMP` block. The outer header uses variable-length integers (varints) that pack seven payload bits per byte; the high bit indicates whether more bytes follow.

| Field                      | Length   | Notes                                                         |
| ------------------------- | --------- | ------------------------------------------------------------- |
| `41 42 4D 50` (`ABMP`)    | 4 bytes   | Afterburner map tag.                                          |
| Encoded map length        | 1–5 bytes | Number of compressed bytes that follow.                       |
| Encoded compression type  | 1–5 bytes | Compression algorithm selector (0 = stored, non-zero = zlib). |
| Encoded uncompressed size | 1–5 bytes | Expected size of the metadata after decompression.            |

The compressed payload inflates into a second stream of varints with the following layout:

| Field                     | Notes                                              |
| ------------------------- | -------------------------------------------------- |
| Encoded control value 1   | First control varint preserved for diagnostics.    |
| Encoded control value 2   | Second control varint preserved for diagnostics.   |
| Encoded resource count    | Number of resource rows described by the metadata. |
| Encoded resource id       | Resource index for the current row.                |
| Encoded offset            | Relative byte offset. A decoded value of `-1` flags an Initial Load Segment (ILS) resource stored in memory. |
| Encoded compressed size   | Length of the compressed payload.                  |
| Encoded uncompressed size | Length expected after decompression.               |
| Encoded compression type  | Algorithm selector for this row (0 = stored, non-zero = zlib). |
| Resource tag              | Four-character chunk identifier appended after the varints.    |

Offsets greater than or equal to zero point into the movie file. When the compression type is non-zero, the byte range is a zlib stream.

## `FGEI` – Initial Load Segment (ILS)

After the map, Afterburner provides an Initial Load Segment that stores resources whose offsets were marked as `-1`.

| Field                                      | Length             | Notes                                                                         |
| ------------------------------------------ | ------------------ | ----------------------------------------------------------------------------- |
| `46 47 45 49` (`FGEI`)                     | 4 bytes            | ILS tag.                                                                      |
| Encoded control value                      | 1–5 bytes          | Varint read before iterating the payload.                                     |
| Repeating: encoded resource id + raw bytes | Varies             | Each varint resource id is followed by the raw chunk bytes for that resource. |

The loader copies these byte blobs into memory so requests for ILS resources can be satisfied without seeking the file.

## Resource access

When a resource is requested:

1. The reader locates the metadata row for the resource id.
2. If the offset was `-1`, the bytes are pulled from the ILS buffer.
3. Otherwise the reader seeks to `offset` in the movie file and reads `compressed size` bytes.
4. When `compression type` is non-zero, the bytes are inflated with zlib before being returned.
5. The resulting stream still begins with the 8-byte RIFF sub-header (`tag` + `length`). Most parsers skip this header before decoding the payload.

This combination of varint metadata, optional compression, and the ILS block allows Shockwave movies to ship compact archives while keeping essential resources readily available.


[← Back to Docs Home](README.md)