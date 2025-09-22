# Director 10+ Score Stream (VWSC)

[\u2190 Back to documentation home](README.md)

Director MX 2004 (version 10) introduced an updated score stream inside the `VWSC` chunk. The
material below consolidates observations from ProjectorRays research samples so a new
implementation can parse, interpret, and render the timeline without relying on external tools. All
multi-byte integers are big-endian unless stated otherwise.

## Score entry table

Each `VWSC` chunk is composed of an outer header followed by a table of entry offsets. Every entry
provides either global score metadata or a per-sprite interval descriptor.

### Outer header (24 bytes)

| Offset | Type  | Name            | Notes |
| ------ | ----- | --------------- | ----- |
| `0x00` | `u32` | `totalLength`   | Total length of the score payload covered by this table. |
| `0x04` | `s32` | `headerType`    | Constant `-3` (`0xFFFFFFFD`). |
| `0x08` | `u32` | `offsetsOffset` | Start of the offset table. Observed `12`, but treat the field as a pointer in case other layouts emerge. |
| `0x0C` | `u32` | `entryCount`    | Number of entries described in the table. |
| `0x10` | `u32` | `notationBase`  | Observed to equal `entryCount + 1`. |
| `0x14` | `u32` | `entrySizeSum`  | Sum of individual entry lengths (matches the last offset). |

Immediately after the header sits an array of `entryCount + 1` 32-bit offsets. These offsets are
relative to the start of the array. The final offset equals `entrySizeSum` and therefore the total
payload length.

### Entry layout

Research samples arrange the entries in the following order:

1. **Entry 0 – Frame data block.** Contains the timeline header plus the variable-length keyframe
   stream detailed in the sections below.
2. **Entry 1 – Interval order list.** Begins with `u32 count` followed by `count` big-endian
   32-bit integers. Each integer indexes a descriptor entry (see item 4). When the list is empty the
   natural progression `3, 6, 9, …` is implied.
3. **Entries 2–(n).** Reserved slots frequently left empty. The order list skips them.
4. **Descriptor triplets.** Each sprite interval occupies three consecutive entries:
   - **Primary entry:** 44–48 byte interval descriptor.
   - **Secondary entry:** Behaviour references (`castLib`, `castMember`, `u32 reserved`).
   - **Tertiary entry:** Reserved. Director-generated files often leave it empty, but older
     documentation mentions sprite-name storage.

## Frame data block (entry 0)

### Frame data header (20 bytes)

| Offset | Field name         | Type | Meaning | Typical values |
| ------ | ------------------ | ---- | ------- | -------------- |
| `0x00` | `actualSize`       | `u32`| Total size of the keyframe payload. | Varies per file (e.g., `0x0000029E`). |
| `0x04` | `headerByte0`      | `u8` | Historically labelled `unkA1`. | `0x00`. |
| `0x05` | `headerByte1`      | `u8` | Historically labelled `unkA2`. | `0x00`. |
| `0x06` | `headerByte2`      | `u8` | Historically labelled `unkA3`. | `0x00`. |
| `0x07` | `headerLength`     | `u8` | Total header length. | `0x14` (20 bytes). |
| `0x08` | `highestFrame`     | `u32`| Highest referenced frame index (inclusive). | `0x00000006`–`0x00000037` in samples. |
| `0x0C` | `unkB1`            | `u8` | Constant so far. | `0x00`. |
| `0x0D` | `channelGroup`     | `u8` | Director exports show `0x0D`. Early notes called this `firstBlockSize` because the first nested block often spans 54–66 bytes. |
| `0x0E` | `spriteSize`       | `u16`| Size of each sprite record. | Always `0x0030` (48 bytes). |
| `0x10` | `timelineVersion`  | `u8` | Timeline format version. | `0x03`. |
| `0x11` | `timelineFlags`    | `s8` | Timeline feature flags. | Observed `0xEE` (`-18`). |
| `0x12` | `channelCount`     | `u16`| Maximum sprite channel slots. | Typically `150`. |

#### Sample header values

| File                          | Header length (bytes) | `actualSize` | `highestFrame` | `spriteSize` | `channelCount` | First nested block length* | Notes |
|-------------------------------|-----------------------|--------------|----------------|--------------|----------------|----------------------------|-------|
| `5spritesTest.dir`            | 20                    | 670          | 15             | 48           | 150            | 54                         | Standard multi-block. |
| `KeyFramesTestMultiple.dir`   | 20                    | 1908         | 44             | 48           | 150            | 66                         | Large block with many frames. |
| `KeyFramesTest.dir`           | 20                    | 834          | 30             | 48           | 150            | 66                         | Mid-size timeline with nested blocks. |
| `Dir_With_One_Img_Sprite_Hallo.dir` | 20             | 138          | 30             | 48           | 150            | 54                         | Minimal score containing a single sprite. |
| `KeyFrames_Lenear5.dir`       | 20                    | 302          | 55             | 48           | 150            | 8                          | Linear frames with a small first block. |
| `Animation_types.dir`         | 20                    | 1344         | 6              | 48           | 150            | 610                        | Extensive control data inside the first block. |

\*The first nested block length matches the first `0x0036` wrapper encountered after the header.

#### Header observations

- Bytes `0x04–0x06` have remained zero in all collected samples.
- `headerLength` at `0x07` consistently stores the literal value `20` (`0x14`).
- Timeline byte pair (`timelineVersion`, `timelineFlags`) mirrors the values seen in Director MX 2004.

### Sprite record (48 bytes)

Sprite defaults are stored in 48-byte records. Each keyframe update embeds one record using the
wrapper described in [Prefix words](#prefix-words-inside-the-frame-stream).

| Offset | Type     | Meaning |
| ------ | -------- | ------- |
| `0x00` | `u8`     | Tween flag bitfield for the sprite. See [Sprite-based tweening](#sprite-based-tweening) for bit meanings. |
| `0x01` | `s16`    | Control word affecting playback (`0x2010` or `0x1008` observed). |
| `0x03` | `u8`     | Ink mode (lower 7 bits). |
| `0x04` | `u8`     | Foreground colour index. |
| `0x05` | `u8`     | Background colour index. |
| `0x06` | `u16`    | Cast library identifier. |
| `0x08` | `u16`    | Cast member identifier. |
| `0x0A` | `u16`    | Reserved (zero). |
| `0x0C` | `u16`    | Sprite-property table offset used for behaviour lookup. |
| `0x0E` | `s16`    | Vertical position (`locV`). |
| `0x10` | `s16`    | Horizontal position (`locH`). |
| `0x12` | `s16`    | Height in pixels. |
| `0x14` | `s16`    | Width in pixels. |
| `0x16` | `u8`     | Channel chip colour. Bit `0x40` marks editable sprites; low nibble selects the score colour. |
| `0x17` | `u8`     | Blend value (0–255). Runtime converts to 0–100%. |
| `0x18` | `u8`     | Flip flags (`0x02` horizontal, `0x04` vertical). |
| `0x19` | `u8[5]`  | Reserved padding. |
| `0x1E` | `s32`    | Rotation (hundredths of a degree). |
| `0x22` | `s32`    | Skew (hundredths of a degree). |
| `0x26` | `u8[10]` | Padding to align the record to 48 bytes. |

### Sprite-based tweening

Tween settings belong to the sprite as a whole rather than individual keyframes. Director stores the
current tween flags inside the sprite record and updates ease, curvature, speed, and flag masks via
control tags described later in this guide. Grouped metadata (for example tag `0x0180` announcing a
payload length) always precedes the actual tween parameters to ensure the data is scoped to the
correct sprite channel.

## Frame stream organisation

### Nested block structure

Director encodes the frame stream as a series of nested blocks:

```
[Frame data header (20 bytes)]
[Sprite block wrapper]
├─ Block length prefix (`0x0036`)
├─ Sprite record (`0x0030` bytes)
├─ Control bytes (typically 6 bytes)
└─ Terminator (`0x0008`)
[Next block]
└─ …
```

Each 48-byte sprite record is typically followed by approximately 6 bytes of control data before the
`0x0008` end marker. The exact number varies with the payload tags required for the frame.

### Prefix words inside the frame stream

| Word     | Role | Notes |
| -------- | ---- | ----- |
| `0x0000` | Padding | Separates records or aligns subsequent data. |
| `0x0002` | Short payload prefix | The next word is the tag ID and the prefix encodes the payload length. |
| `0x0004` | Four-byte payload prefix | Used by composite tags such as size and position. |
| `0x0008` | End marker | Terminates the current nested block. If an advance-frame tag is pending this concludes the frame change. |
| `0x000C` | Composite marker | Often introduces frame-rectangle payloads. |
| `0x001E`, `0x0020` | Control markers | Seen near frame-level control flags; semantics unknown. |
| `0x0026`, `0x0028`, `0x0094` | Control markers | Additional transition points captured from samples. Preserve raw bytes for future study. |
| `0x0030` | Sprite record length | Always appears inside the `0x0036` wrapper. |
| `0x0036` | Sprite block wrapper | Announces a 54-byte sprite block: record plus trailing terminator. |
| `0x0120` | Channel tag prefix | Selects a sprite channel by index (payload = channel number relative to `0x10`). |
| `0x1000` | Block header | Signals that a sprite block (`0x0030`) follows. |

### Reading logic

When consuming the keyframe stream:

1. Read a prefix word (e.g., `0x0002` or `0x0004`) to learn the payload style.
2. Consume the tag word and interpret it according to the tables below.
3. Read the payload bytes announced by the prefix and apply them to the active sprite or control
   structure.

Capture any unrecognised prefix or tag verbatim so the research tables can grow with new samples.

### Keyframe payload tags

Unless noted, tags use the short payload prefix `0x0002` and the bytes that follow are interpreted as
big-endian values.

| Tag | Payload bytes | Applies to | Description |
| --- | ------------- | ---------- | ----------- |
| `0x0120` | 2 | Sprite defaults | Ease-in and ease-out values expressed as single bytes. Research still considers alternative roles for this tag; see [Tag 0x0120 hypotheses](#tag-0x0120-hypotheses). |
| `0x012E` | 2 | Sprite defaults | Tween speed scalar controlling intermediate steps along the motion path. |
| `0x0130` | 4 | Keyframe | Width and height (`s16` each). |
| `0x0136` | 2 | Control block | Advance-frame tag for channel 6. Payload bits match [Advance-frame flag bits](#advance-frame-flag-bits). |
| `0x015C` | 4 | Keyframe | Horizontal and vertical position (`s16` each). |
| `0x0166` | 2 | Control block | Advance-frame tag for channel 7. |
| `0x0180` | 2 | Control block | Announces the byte length of the upcoming nested payload (tween groups, behaviour lists). |
| `0x0182` | 2 | Keyframe | Foreground and background colour indices (`u8` each). |
| `0x018A` | Variable | Control block | Seen in composite metadata groups after `0x0180`; semantics under investigation. |
| `0x0190` | 6 | Keyframe | Width, height, blend (byte), and ink (byte) packed together. |
| `0x0196` | 2 | Control block | Advance-frame tag for channel 8. |
| `0x019E` | 2 | Keyframe | Rotation angle stored as hundredths of a degree (`s16`). |
| `0x01A2` | 2 | Keyframe | Skew angle stored as hundredths of a degree (`s16`). |
| `0x01B0` | Variable | Control block | Rare tag encountered near tween metadata; payload currently unidentified. |
| `0x01BA` | Variable | Control block | Additional control bytes, observed near the end of nested blocks. |
| `0x01C6` | 2 | Control block | Advance-frame tag for channel 9; also appears in sprite metadata sequences. |
| `0x01CE` | 2 | Keyframe | Rare tag with small payload; capture raw bytes for later analysis. |
| `0x01D2` | 2 | Keyframe | Seen alongside rotation/skew updates; semantics unresolved. |
| `0x01EC` | 8 | Keyframe | Frame rectangle (`locH`, `locV`, `width`, `height`). |
| `0x01F4` | 2 | Sprite defaults | Curvature strength for tweened motion (0–65535). |
| `0x01F6` | 1–2 | Sprite defaults | Tween-property bitmask. See [Tween flag mask (tags `0x01F6`/`0x1CF6`)](#tween-flag-mask-tags-0x01f6-0x1cf6). |
| `0x01FC` | 2 | Control block | Additional sprite flags toggled at runtime (bits under review). |
| `0x01FE` | 2 | Control block | Frame-level flags linked to transitions and sprite locks. |
| `0x0202` | 1 | Control block | Transition code associated with `0x01FE`. |
| `0x0212` | 2 | Keyframe | Foreground and background colour indices (`u8` each). |
| `0x0226` | 2 | Control block | Advance-frame tag commonly pointing to channel 10. |
| `0x0240` | 2 | Control block | Auxiliary sprite-channel helper (observed near `0x0226`). |
| `0x0256` | 2 | Control block | Advance-frame tag for channel 11. |
| `0x0286` | 2 | Control block | Advance-frame tag for channel 12. |
| `0x0316` | 2 | Control block | High-range channel selector (suspected channel 13). |
| `0x04B0` | 2 | Control block | Additional channel selector (channel 14 observed). |
| `0x04C0` | 4 | Keyframe | Duplicate of size tag `0x0130`. |
| `0x04C6` | 2 | Control block | Secondary channel or sprite index marker. |
| `0x1020` | 2 | Control block | Channel-linked metadata often preceding `0x0130`. |
| `0x1030` | 4 | Keyframe | Size repeat mirroring `0x0130`. |
| `0x1036` | 2 | Control block | Unknown flag near multi-channel payloads. |
| `0x13B0` | 2 | Control block | Channel link observed in multi-sprite payloads. |
| `0x13C0` | 4 | Keyframe | Size repeat variant. |
| `0x13C6` | 2 | Control block | Channel selector used alongside the `0x13C0` duplicate. |
| `0x1CE0` | 2 | Control block | Channel link observed in extended payloads. |
| `0x1CF0` | 4 | Keyframe | Size repeat for high channels. |
| `0x1CF6` | 2 | Sprite defaults | Extended tween flag payload (mirrors `0x01F6`). |

### Composite payload layouts

When a tag lists multiple properties, decode the payload using the layouts below. All values are
big-endian.

| Tag | Payload structure | Notes |
| --- | ----------------- | ----- |
| `0x0130`, `0x04C0`, `0x1030`, `0x13C0`, `0x1CF0` | `s16 width`, `s16 height` | Sprite bounds in pixels. |
| `0x015C` | `s16 locH`, `s16 locV` | Stage position relative to the top-left corner. |
| `0x0182`, `0x0212` | `u8 foreColor`, `u8 backColor` | Palette indices for the sprite. |
| `0x0190` | `s16 width`, `s16 height`, `u8 blend`, `u8 ink` | Blend converts to percentages with `blend / 255`. |
| `0x01EC` | `s16 locH`, `s16 locV`, `s16 width`, `s16 height` | Frame rectangle used by the stage editor. |
| `0x01F6`, `0x1CF6` | `u8 flags`, `[u8 mask]` | Tween-property mask; see [Tween flag mask](#tween-flag-mask-tags-0x01f6-0x1cf6). |

### Channel selectors and advance-frame tags

Channel tags follow a repeating pattern:

```
channel = ((tag - 0x0136) / 0x30) + 6
```

This formula matches most samples but remains under investigation—sprite data on channel 10 in one
ProjectorRays test did **not** obey the progression, so treat the mapping as speculative until more
files confirm it.

Known tag/channel pairs:

| Tag | Expected channel | Notes |
| --- | ---------------- | ----- |
| `0x0136` | 6 | Advance-frame payload follows. |
| `0x0166` | 7 | |
| `0x0196` | 8 | |
| `0x01C6` | 9 | |
| `0x0226` | 10 | Seen in most samples but the mapping failed for at least one sprite. |
| `0x0256` | 11 | |
| `0x0286` | 12 | |
| `0x0316` | 13 | |
| `0x04B0` | 14 | High-range selector. |

### Advance-frame flag bits

The 16-bit payload that accompanies channel tags encodes frame timing and additional control flags:

| Bits | Mask | Meaning |
| ---- | ---- | ------- |
| 15   | `0x8000` | Create a new keyframe for the active sprite when set. |
| 8–14 | `0x7F00` | Frame delta. Treat zero as one frame. |
| 5    | `0x0020` | Flip horizontal (FlipH). |
| 6    | `0x0040` | Flip vertical (FlipV). |
| 7    | `0x0080` | Tween continuation flag (no new keyframe). |
| 0–4  | `0x001F` | Reserved or currently unknown. |

For tag `0x0136` specifically, payload byte `0x01` creates a real keyframe while `0x81` keeps the
sprite in tween-only mode. The sequence `0x0000 0x0002` after the flag payload advances playback by
one frame.

### Tag `0x0120` hypotheses

Although ProjectorRays treats tag `0x0120` as the ease-in/ease-out pair for the active sprite, earlier
notes captured alternative interpretations. Keep these possibilities in mind when analysing new
samples:

- Structural block separator dividing logical tag groups.
- Block type identifier comparable to the behaviour of `0x1CF6` flag blocks.
- Data-alignment padding or legacy opcode.
- Frame-state reset marker bracketing control bytes.

### Tween flag mask (tags `0x01F6`/`0x1CF6`)

The tween flag payload indicates which sprite properties interpolate between keyframes.

| Bit | Mask   | Meaning |
| --- | ------ | ------- |
| 0   | `0x01` | Path (position). |
| 1   | `0x02` | Size. |
| 2   | `0x04` | Rotation. |
| 3   | `0x08` | Skew. |
| 4   | `0x10` | Blend. |
| 5   | `0x20` | Foreground colour. |
| 6   | `0x40` | Background colour. |
| 7   | `0x80` | Sprite carries tweening data (global enable flag). |

Example: payload `0x4A` (`01001010b`) enables rotation, blend, and path interpolation. Payload
`0x81` enables tweening plus background-colour interpolation.

### Example sprite-level tween block

```
00 04 01 80 00 30 00 00   ; 48 bytes follow
00 02 01 20 10 80         ; Sprite 8 (channel = 8)
00 02 01 F4 00 96         ; Curvature = 150 (~59%)
00 02 01 20 10 80         ; EaseIn = 0x10, EaseOut = 0x80
00 02 01 F6 00 4A         ; Tween Flags = Path, Rotation, Blend
```

### Tag ranges (hypothesis)

Tag values cluster into broad groups. Treat these ranges as guidance only until additional samples
confirm them.

| Range | Purpose / group | Examples | Notes |
| ----- | ---------------- | -------- | ----- |
| `0x0100–0x01FF` | Sprite and tweening tags | `0x0130` (Size), `0x015C` (Position), `0x0120` (Ease), `0x01F4` (Curvature), `0x01F6` (Tween flags) | Primary animation data. |
| `0x0200–0x02FF` | Colour, palette, and extensions | `0x0212` (Colours), `0x0226` (Channel selector) | Handles palette indices and higher sprite channels. |
| `0x0300–0x03FF` | Special control tags | `0x0316` (High-range channel selector) | Rare in simple scores. |
| `0x0400–0x04FF` | Per-channel repeats and helpers | `0x04C0` (Size repeat), `0x04B0` (Channel selector) | Duplicate payloads emitted for grouped sprites. |
| `0x1000–0x1FFF` | Multi-sprite or multi-frame tags | `0x1030`, `0x13C0`, `0x1CF6` | Used when the engine repeats payloads across sprite groups. |
| `0xFF00–0xFFFF` | Frame/keyframe headers | `0xFF00` | Used in start-of-block headers, not as standalone tags. |

### Composite-tag bit layout hypothesis

Several composite tags (`0x0190`, `0x01FE`, `0x01F6`) appear to encode multiple sprite properties via
bitfields where each bit corresponds to attributes such as path, size, rotation, skew, blend, and
colour channels. This hypothesis explains why certain payloads bundle properties together and should
be considered when reverse-engineering new opcodes.

## Interval descriptors and behaviours

### Interval descriptor record (44+ bytes)

| Offset | Type   | Meaning |
| ------ | ------ | ------- |
| `0x00` | `s32`  | Start frame (inclusive). |
| `0x04` | `s32`  | End frame (inclusive). |
| `0x08` | `s32`  | Unknown; currently observed as zero. |
| `0x0C` | `s16`  | Unknown; currently zero. |
| `0x0E` | `u16`  | Sprite flags (bits correspond to flip, lock, trails, moveable, editable, etc.). |
| `0x10` | `s32`  | Channel number (6-based; aligns with sprite record channel). |
| `0x14` | `s16`  | Constant `1` in observed files. |
| `0x16` | `s16`  | Unknown A (typically zero). |
| `0x18` | `s16`  | Unknown B (values around `15`). |
| `0x1A` | `u8`   | Unknown constant `0xE1`. |
| `0x1B` | `u8`   | Unknown constant `0xFD`. |
| `0x1C` | `s16`  | Unknown (zero so far). |
| `0x1E` | `s32`  | Unknown (zero so far). |
| `0x22` | `s32[]`| Optional trailing integers (commonly `0` or the descriptor size). |

### Behaviour list entry (secondary descriptor)

Each 8-byte tuple binds a behaviour script to the interval:

| Offset | Type | Meaning |
| ------ | ---- | ------- |
| `0x00` | `s16`| Cast library ID. |
| `0x02` | `s16`| Cast member ID. |
| `0x04` | `u32`| Reserved (zero). |

### Entry ordering

The interval order list (entry 1) references the **primary** descriptor entries. If the list is empty,
interpret the descriptors sequentially in groups of three (`primary`, `secondary`, `tertiary`) until
the offset table is exhausted. Skip zero-length entries automatically.

## Rendering workflow

A renderer targeting Director 10+ score data should:

1. Parse the outer header and offset table to isolate each entry.
2. Consume the frame data header to learn the sprite record size, timeline metadata, and channel
   count.
3. Iterate the keyframe stream, using the prefix words and tag tables above to decode sprite blocks
   and apply property updates.
4. Track active sprites with the advance-frame channel tags while respecting the speculative mapping
   notes for higher channels.
5. Merge sprite defaults with keyframe deltas to reconstruct per-frame stage state, preserving any
   unknown tags for later analysis.
6. Load interval descriptors to determine sprite lifetimes and attach behaviour references.
7. Apply sprite-level tween flags, ease curves, curvature, and speed metadata to interpolate between
   keyframes, noting that tweening is scoped to sprites rather than individual frames.

