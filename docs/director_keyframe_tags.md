# Director Keyframe Tags Documentation

## Core Concepts

This document outlines all currently understood fixed and variable tag formats used in Director's keyframe system. It includes control blocks, tag definitions, and observed channel mappings across various `.dir` files.

---

# 🧱 Director Keyframe Block Header Bytes

This section documents the **header structure** found at the beginning of each Director Score keyframe block (VWSC). These values are typically fixed across files, except for `HighestFrameNumber`, `actualSize`

---

## 🧾 Header Structure (Offsets 0x00–0x13)

| Offset | Field                 | Type     | Meaning                        | Example Values     |
|--------|------------------------|----------|--------------------------------|---------------------|
| 0x00   | actualSize            | int32    | Total size of the keyframe block | Varies (e.g., 670)  |
| 0x04   | unkA1                 | int8     | Constant? Usually 0             | 0                   |
| 0x05   | unkA2                 | int8     | Constant? Usually 0             | 0                   |
| 0x06   | unkA3                 | int8     | Constant? Usually 0             | 0                   |
| 0x07   | unkA4                 | int8     | Constant 20                     | 20                  |
| 0x08   | HighestFrameNumber    | int32    | Highest referenced frame        | 6–55                |
| 0x0C   | unkB1                 | int8     | Constant?                       | 0                   |
| 0x0D   | unkB2                 | int8     | Unknown                         | 13                  |
| 0x0E   | spriteSize            | int16    | bytes count of fixed sprite block      | 48 (always)         |
| 0x10   | unkC1                 | int8     | Constant?                       | 3                   |
| 0x11   | unkC2                 | int8     | Unknown                         | -18                 |
| 0x12   | SpriteChannelCount    | int16    | Max channel slots available     | 150                 |
| 0x14   | FirstBlockSize        | int16    | Possibly tag count or pointer   | 8–610               |

---

## 📂 Sample Files & Header Values

| File                        | actualSize | HighestFrameNumber | SpriteSize | SpriteChannelCount | FirstBlockSize |
|-----------------------------|------------|---------------------|------------|---------------------|--------|
| 5spritesTest                | 670        | 15                  | 48         | 150                 | 54     |
| KeyFramesTestMultiple.dir   | 1908       | 44                  | 48         | 150                 | 66     |
| KeyFramesTest.dir           | 834        | 30                  | 48         | 150                 | 66     |
| Dir_With_One_Img_Sprite_Hallo | 138      | 30                  | 48         | 150                 | 54     |
| KeyFrames_Lenear5.dir       | 302        | 55                  | 48         | 150                 | 8      |
| Animation_types.dir         | 1344       | 6                   | 48         | 150                 | 610    |

---

## 🧠 Observations

- Fields `unkA1–unkA3` are always 0 so far.
- `unkA4` is always `20`.
- `unkC1` and `unkC2` are consistent (`3` and `-18`) across all samples.

---

## Control Prefixes (Header Byte Sequences)

| Byte(s)         | Type         | Meaning                                      |
|------------------|--------------|----------------------------------------------|
| `00 02`          | Prefix       | Repeat tag or delta                          |
| `00 04`          | Prefix       | 4-byte payload (e.g., size, position)        |
| `00 08`          | Marker       | End of logical frame block                   |
| `00 30`          | Marker       | Start of 48-byte keyframe block              |
| `00 36`          | BlockSize    | Full block = 54 bytes                        |
| `00 0C`          | Composite    | Often FrameRect follows                      |
| `00 1E`, `00 20` | Control      | Frame control marker                         |
| `00 26`, `00 28`, `00 94`, etc. | Control      | Unknown control transition points |
| `00 00`          | Padding      | Empty marker                                 |
                  |

---

## Known Keyframe Tags (Bytes 2–3)

These tags appear after the prefix (`00 02`, `00 04`, etc.) and identify the type of operation or sprite property. 



| Tag     | Name           | Byte Size | Meaning                                    | Notes |
|---------|----------------|-----------|--------------------------------------------|-------|
| `01 30` | Size           | 4         | Width + Height                             | 2 bytes each |
| `01 5C` | Position       | 4         | LocH + LocV                                | 2 bytes each |
| `01 20` | unknown        | 2         | unknown                                    | unknown |
| `01 36` | AdvanceFrame   | 2–8       | Real or Tween keyframe switch              | Flag 0x01 = real keyframe, 0x81 = tween only |
| `01 66` | SwapCurrentChannelTo 6       | 2         | change to channel 6                |  |
| `01 96` | SwapCurrentChannelTo 7            | 2         | change to channel 7           |  |
| `01 9E` | Rotation       | 2         | Degrees * 100                              | Fixed-point (e.g. 1234 = 12.34°) |
| `01 A2` | Skew           | 2         | Skew degrees * 100                         | Fixed-point |
| `01 C6` | ?              | 2         | Possibly sprite ID or controller           | Unknown |
| `01 D2` | Skew (alt)     | 2         | Seen with Rotation                         | Possibly second skew axis |
| `01 EC` | FrameRect      | 8         | LocH, LocV, Width, Height                  | Composite |
| `01 F4` | Curvature      | 2–4       | Single-byte curvature, sometimes extended  | 0–255 = 0–100% |
| `01 F6` | TweenFlags     | 1         | Bitmask for tween properties               | See below |
| `01 FE` | Flags          | 2         | Frame state or transition flags            | Unconfirmed |
| `02 02` | TransitionCode | 1         | Likely transition control                  | Seen with 0x01FE |
| `02 12` | Colors         | 2         | ForeColor + BackColor                      | Palette indices |
| `02 26` | SwapCurrentChannelTo 10     | 2         | Sprite Channel Tag (channel = 10)          | |
| `02 56` | SwapCurrentChannelTo 11     | 2         | Sprite Channel Tag                         | |
| `02 86` | SwapCurrentChannelTo 12     | 2         | Sprite Channel Tag                         | |
| `03 16` | SwapCurrentChannelTo 13     | 2         | Sprite Channel Tag                         |  |
| `04 B0` | SwapCurrentChannelTo 14     | 2         | Sprite Channel Tag                         |  |
| `04 C0` | Size Repeat    | 4         | Width + Height                             | Often with 0x0130 |
| `04 C6` | Flag/Link      | 2         | Possibly sprite index                      | Unknown |
| `10 20` | Channel Data   | 2         | Possibly per-channel value                 | Usually followed by 0x0130 |
| `10 30` | Size Repeat    | 4         | Same as 0x0130                             | |
| `10 36` | Flag/Link      | 2         | Unknown, likely channel index              | |
| `13 B0` | Channel Link   | 2         | Unknown                                    | |
| `13 C0` | Size Repeat    | 4         | Width + Height                             | |
| `13 C6` | Flag/Link      | 2         | Unknown                                    | |
| `1C E0` | Channel Link   | 2         | Unknown                                    | |
| `1C F0` | Size Repeat    | 4         | Width + Height                             | |
| `1C F6` | TweenFlags     | 2         | Byte 1 = flags, Byte 2 = bitmask           | Enhanced tween flags |
| `01 2E` | Speed           | 2         | Movement speed between keyframes         | Sprite-based (seen in tween block) |
| `01 BA` | Unknown01BA     | 6–8       | Control/timing unknown                   | Often in end-of-block sequences |
| `01 B0` | Unknown01B0     | 6–8       | Possibly sprite bounds or padding        | Seen after 019E/01A2/01D2 |
| `01 8A` | Unknown018A     | 6–8       | Seen in secondary composite blocks       | Often after 0180 |
| `01 CE` | Unknown01CE     | 2         | Possibly alignment/skew-related          | |
| `02 40` | Channel Flag?   | 2         | Sprite channel helper?                   | Seen with 02 26 etc |
| `04 B0` | SwapCurrentChannelTo 14      | 2         | Sprite channel index tag                 | Derived tag |



---

PS.: Tweening: The tweening are not keyframe based but sprite based!

### unkown 0x0120
🟡 0x0120 Might Be One of the Following:
Possibility	Supporting Evidence
- A structural block separator	Appears between logical tag groups, possibly segmenting “channels” or “zones”
- A block type ID or category	May act like a mode switch — similar to 0x1CF6 style flag blocks
- A data corruption / null padding point	Unlikely here, but worth keeping open if file is malformed
- A frame state reset marker	Preceded and followed by tag-like data and padding
- A deprecated opcode	Possibly an old tag type no longer used or interpreted



## 🧩 Offset Diagram for Composite Tags


| Tag     | Total Size | Offset | Field         | Description                            |
|----------|------------|--------|----------------|----------------------------------------|
| `0x0130` | 4 bytes    | 0 – 1  | Width          | Sprite width (pixels)                  |
|          |            | 2 – 3  | Height         | Sprite height (pixels)                 |
| `0x015C` | 4 bytes    | 0 – 1  | LocH           | Horizontal screen position             |
|          |            | 2 – 3  | LocV           | Vertical screen position               |
| `0x0182` | 2 bytes    | 0      | ForeColor      | Foreground color (palette index)       |
|          |            | 1      | BackColor      | Background color (palette index)       |
| `0x0190` | 6 bytes    | 0 – 1  | Width          | Sprite width                           |
|          |            | 2 – 3  | Height         | Sprite height                          |
|          |            | 4      | Blend          | Blend percentage (byte, 0 – 255)       |
|          |            | 5      | Ink            | Drawing ink mode (constant index)      |
| `0x01EC` | 8 bytes    | 0 – 1  | LocH           | Frame rectangle left                   |
|          |            | 2 – 3  | LocV           | Frame rectangle top                    |
|          |            | 4 – 5  | Width          | Frame rectangle width                  |
|          |            | 6 – 7  | Height         | Frame rectangle height                 |
| `0x0212` | 2 bytes    | 0      | ForeColor      | Foreground color index (1 byte)        |
|          |            | 1      | BackColor      | Background color index (1 byte)        |
| `0x01F6` | 1 byte     | 0      | Tween Flags    | Bit flags for tweening fields (see below) |



## 📌 Sprite-Based Tweening: Association and Scoping

> 🔁 **Tweening values (EaseIn, EaseOut, Curvature, Speed, Flags) are associated with entire sprites, not individual keyframes.**

When you select a sprite in the Director GUI, the **tweening panel (sliders and toggles)** applies to all its keyframes collectively. This behavior is reflected in the binary:

### 🧩 Per-Sprite Association

- **Tweening tags (`0x0120`, `0x01F4`, `0x01F6`)** appear **outside the main keyframe stream**.
- They are grouped **after `0x0180` and `0x0120`**, which together define:
  - **How many bytes follow (block size)**
  - **Which sprite these values are assigned to** (e.g. `0x1080` = sprite 8)


### 🧪 TweenFlags Bitmask (Tag 0x01F6 or 0x1CF6)

The tweening flag mask indicates which properties are interpolated between keyframes.

| Bit | Mask   | Meaning        |
|-----|--------|----------------|
| 0   | `0x01` | Path (Position) |
| 1   | `0x02` | Size            |
| 2   | `0x04` | Rotation        |
| 3   | `0x08` | Skew            |
| 4   | `0x10` | Blend           |
| 5   | `0x20` | ForeColor       |
| 6   | `0x40` | BackColor       |
| 7   | `0x80` | Sprite has tweening data (global toggle) |

Example:

- `0x4A` = `01001010` → Tween: Rotation, Blend, Path
- `0x81` = `10000001` → Tweening + BackColor


### ✅ Example

```hex
00 04 01 80 00 30 00 00   ; 48 bytes follow
00 02 01 20 10 80         ; Sprite 8 (channel = 8)
00 02 01 F4 00 96         ; Curvature = 150 (~59%)
00 02 01 20 10 80         ; EaseIn = 0x10, EaseOut = 0x80
00 02 01 F6 00 4A         ; Tween Flags = Path, Rotation, Blend
```


## 🧭 Director Tag Ranges (Hypothesis)

| Range         | Purpose / Group                 | Examples                              | Notes |
|---------------|----------------------------------|----------------------------------------|-------|
| `0x0100–0x01FF` | Sprite and tweening tags         | `0130` (Size), `015C` (Position), `0120` (Ease), `01F4` (Curvature), `01F6` (TweenFlags) | Most visible and animation-related data |
| `0x0200–0x02FF` | Color, palette, and extensions   | `0212` (Colors), `0226` (Palette?)     | Likely color and palette info |
| `0x0300–0x03FF` | Unknown / special control tags   | `0316` (?), sometimes with `81 00`     | Rare in normal keyframes |
| `0x0400–0x04FF` | Per-channel offsets and repeats  | `04C0`, `0430`, `04B0`                 | Possibly channel bounding or inheritance |
| `0x1000–0x1FFF` | Multi-sprite or multi-frame tags | `1030`, `13C0`, `1CF0`, `1CF6`         | Consistently seen repeating block data |
| `0xFF00–0xFFFF` | Frame/Keyframe header            | `FF00`, etc.                           | Not real tags; used in STARTKEYFRAME headers |





---

## Reading Logic

- `2 bytes`: Payload length (after the tag)
- `2 bytes`: Tag value (property or sprite channel)
- `X bytes`: Payload data


## Sprite Channel swapping & AdvanceFrame Flag Byte (0x0136 , 0x0166, ...à

Tags can also reference sprite channels by offsetting from `0x0136`.

```csharp
// Encode:
tag = 0x0136 + (channel - 6) * 0x30

// Decode:
channel = ((tag - 0x0136) / 0x30) + 6
```

Examples:

| Tag     | Channel |
|---------|---------|
| `01 36` | 6       |
| `01 66` | 7       |
| `01 96` | 8       |
| `01 C6` | 9       |
| `02 26` | 10      |
| `02 56` | 11      |
| `02 86` | 12      |
THIS table is not correct, see last chapter in this document!

These are used to direct the upcoming data to the correct sprite channel.

this number are FLAGS!
###  Director Keyframe Flag Bits 

| **Bit Position** | **Hex Value** | **Binary**   | **Meaning**                                  | **Confirmed?** |
|------------------|---------------|--------------|----------------------------------------------|----------------|
| Bit 0            | `0x01`        | `00000001`   | advance 1frame and create Real keyframe      | ✅ Yes        |
| Bit 1            | `0x02`        | `00000010`   | Unknown                                      | ❌ No         |
| Bit 2            | `0x04`        | `00000100`   | Unknown                                      | ❌ No         |
| Bit 3            | `0x08`        | `00001000`   | Unknown                                      | ❌ No         |
| Bit 4            | `0x10`        | `00010000`   | Unknown                                      | ❌ No         |
| Bit 5            | `0x20`        | `00100000`   | Flip Horizontal (FlipH)                      | ✅ Yes        |
| Bit 6            | `0x40`        | `01000000`   | Flip Vertical (FlipV)                        | ✅ Yes        |
| Bit 7            | `0x80`        | `10000000`   | Tweening continuation flag                   | ✅ Yes        |



Special handling is required for composite tags (e.g. `01 90`), frame-level headers (e.g. `01 EC`), and control sequences (`00 08`, `00 0A`, etc).

### Byte 4: Flags if keyframe is real or tweeening
for tag 0x0136:
 - 01 → a real keyframe
 - 81 → no new keyframe, just continuation or tween data
 
 Then next byte: 
 00 02 advance one keyframe


## Composed tags

This table lists all **tags observed to contain multiple sprite properties**, drawn from real data in `5spritesTest.dir` and `KeyFramesTest.dir`. These are not followed by separate property tags — the values are bundled directly in the payload.

| Tag Name     | Hex     | Properties Included                           | Bit Representation (binary) | Notes |
|--------------|---------|-----------------------------------------------|-----------------------------|-------|
| **Size**     | `0x0130`| Width, Height                                 | `0001 0011 0000`            | Standard size tag (2 + 2 bytes) |
| **Position** | `0x015C`| LocH, LocV                                    | `0001 0101 1100`            | Appears in path-related blocks |
| **Colors**   | `0x0182`| ForeColor (1 byte), BackColor (1 byte)        | `0001 1000 0010`            | Often paired with `0x0190`     |
| **Composite**| `0x0190`| Width, Height, Blend, Ink                     | `0001 1001 0000`            | Frequently used; always 6–7 bytes |
| **Extended Frame Flags** | `0x01FE` | Mode/transition flags (unclear)     | `0001 1111 1110`            | Found in many frame blocks     |
| **Block Control** | `0x0180`| Possibly: Size, Channel count, Offsets? | `0001 1000 0000`            | Payload = 6+ bytes, unknown format |
| **Speed**     | `0x012E` | Movement Speed                         | `0001 0010 1110`            | Seen in global tween block |
| **Ease**      | `0x0120` | EaseIn (byte 1), EaseOut (byte 2)     | `0001 0010 0000`            | Applies per-sprite |
| **Curvature** | `0x01F4` | Curvature/velocity control            | 1–2 bytes                  | Applies globally per sprite |
| **TweenFlags**| `0x01F6` | Tween toggles                         | 1–2 bytes                   | Controls which properties tween |


## 🧪 Bit Flag H

It’s possible that these tag values encode **bitfields**, with each bit corresponding to a sprite property:

| Bit | Value (bit) | Property          |
|-----|-------------|-------------------|
| 0   | `0x01`      | Tweening?         |
| 1   | `0x02`      | Path (Position)   |
| 2   | `0x04`      | Size              |
| 3   | `0x08`      | Rotation          |
| 4   | `0x10`      | Skew              |
| 5   | `0x20`      | Blend             |
| 6   | `0x40`      | ForeColor         |
| 7   | `0x80`      | BackColor         |

### Bitflag Hypothesis:
Several composite tags (such as 0x0190, 0x01FE, and 0x01F6) are believed to encode multiple sprite properties using bitflags, where each bit corresponds to a particular attribute (e.g., Path, Size, Rotation, Skew, Blend, ForeColor, BackColor). This hypothesis helps explain the grouping of related properties within single tag payloads and should be considered when parsing these tags.


### Example:

`0x0190` → `0001 1001 0000`  
Possibly means:
- Bit 4 → **Skew**
- Bit 5 → **Blend**
- Bit 7 → **Size**

→ **Size + Blend + Skew**, which matches reality.


# Director Keyframe Blocks: Header and Nested Block Structure

This chapter details the structure of Director keyframe blocks across various sample files. It includes header field descriptions, nested block organization, and file-specific variations.

---

## Keyframe Files Overview

| File                        | Header Length (Bytes) | actualSize (Bytes) | SpriteChannelCount | SpriteSize | HighestFrameNumber | FirstBlockSize | Notes                      |
|-----------------------------|----------------------|--------------------|--------------------|------------|--------------------|-------|----------------------------|
| 5spritesTest                | 20                   | 670                | 150                | 48         | 15                 | 54    | Standard multi-block       |
| KeyFramesTestMultiple.dir   | 20                   | 1908               | 150                | 48         | 44                 | 66    | Large block, multi-frames  |
| KeyFramesTest.dir           | 20                   | 834                | 150                | 48         | 30                 | 66    | Mid-size, nested inner blocks |
| Dir_With_One_Img_Sprite_Hallo | 20                | 138                | 150                | 48         | 30                 | 54    | Smallest file              |
| KeyFrames_Lenear5.dir       | 20                   | 302                | 150                | 48         | 55                 | 8     | Linear frames, small FirstBlockSize |

---

## Keyframe Block Header Structure (20 bytes)

| Offset | Field                 | Type     | Meaning                        | Typical Values      |
|--------|------------------------|----------|--------------------------------|---------------------|
| 0x00   | actualSize            | int32    | Total size of keyframe block   | Varies              |
| 0x04   | unkA1                 | int8     | Constant?                     | 0                   |
| 0x05   | unkA2                 | int8     | Constant?                     | 0                   |
| 0x06   | unkA3                 | int8     | Constant?                     | 0                   |
| 0x07   | unkA4                 | int8     | Unknown, often 20             | always 20 it seems  |
| 0x08   | HighestFrameNumber    | int32    | Highest frame number          | Varies              |
| 0x0C   | unkB1                 | int8     | Constant?                     | 0                   |
| 0x0D   | unkB2                 | int8     | Unknown                      | 13                  |
| 0x0E   | spriteSize            | int16    | Size of each sprite block     | 48                  |
| 0x10   | unkC1                 | int8     | Constant?                     | 3                   |
| 0x11   | unkC2                 | int8     | Unknown                      | -18                 |
| 0x12   | SpriteChannelCount    | int16    | Number of sprite channels     | 150                 |
| 0x14   | FirstBlockSize        | int16    | Size of first inner block     | Varies (e.g., 54-610) |

---

## Nested Block Structure Overview

#### Note:
Each fixed keyframe block of 48 bytes is typically followed by approximately 6 bytes of control data (flags or padding) before the end marker (00 00 00 08). This control byte section is important for understanding the full block structure and should be parsed accordingly.

### Common pattern:
```
[Header] (20 bytes)
[Inner Block 1] (size = main block size bytes)
├─ Frame skip / continuation markers (optional)
├─ Fixed-size keyframe block (48 bytes)
├─ Control bytes
└─ End marker (00 00 00 08)
[Inner Block 2] (size = next block size)
└─ ...
```

---

## File-specific Nested Block Diagrams

### 5spritesTest.dir
```
[Header: 20 bytes]
[Inner Block 1: 54 bytes]
├─ Fixed keyframe block (48 bytes)
├─ Control bytes (6 bytes)
└─ End marker (00 00 00 08)
[Inner Block 2: ...]
```

### KeyFramesTestMultiple.dir
```
[Header: 20 bytes]
[Inner Block 1: 66 bytes]
├─ Frame skip markers
├─ Fixed keyframe block (48 bytes)
├─ Control bytes
└─ End marker (00 00 00 08)
[Inner Block 2: ...]
```

### KeyFramesTest.dir
```
[Header: 20 bytes]
[Inner Block 1: 66 bytes] 
├─ Frame skip markers (e.g., skip 6 frames) 
├─ Fixed keyframe block (48 bytes) 
├─ Control bytes 
└─ End marker (00 00 00 08) 
```
[Inner Block 2: ...]



### Dir_With_One_Img_Sprite_Hallo
```
[Header: 20 bytes]
[Inner Block 1: 54 bytes]
├─ Fixed keyframe block (48 bytes)
├─ Control bytes
└─ End marker (00 00 00 08)
```

### KeyFrames_Lenear5.dir
```
[Header: 20 bytes]
[Inner Block 1: 54 bytes]
├─ Frame skip markers (00 02 sequences)
├─ Fixed keyframe block (48 bytes)
├─ Control bytes
└─ End marker (00 00 00 08)
[Inner Block 2: ...]

```

---

## Notes

- **Frame skip markers (`00 02 ...`)** appear before keyframe blocks to indicate frame advancement or continuation.
- The **end marker `00 00 00 08`** clearly marks the end of a keyframe data block.
- `FirstBlockSize` from the header matches the size of the first nested block, enabling precise parsing.
- The **fixed-size keyframe block is always 48 bytes**, consistent across files.
- Control bytes (typically 6 bytes) contain flags or padding after the keyframe block.

---



## 🔁 AdvanceFrame (Tag 0x0136)

This tag indicates a keyframe or tween continuation. It appears frequently in alternating sequences between real keyframes and tweened steps.

### Byte 4: Flags

| Byte Value | Meaning                                |
|------------|----------------------------------------|
| `0x01`     | Real keyframe                          |
| `0x81`     | Tween-only continuation                |
| `0x08`     | Final step / no delta? Often a terminator |
| `0x02`     | Advance 1 or 2 frames                  |
| `0x81 + 02`| Tween + advance                        |

### Examples

```hex
00 02 01 36 81 00 00 02 00 02 00 02 00 08
; Tween frame (Flag=0x81), steps=[2,2,2], ends with 0x08
```


### Control Marker: `00 08`

This byte appears **after sequences of AdvanceFrame** (tag `0x0136`) and **marks the end of a logical tween or frame sequence**. It's **required** to finalize the frame progression block.

Example:

```hex
... 00 02 01 36 81 00 00 02 00 02 00 02 00 08
                              ^^^^^^^^^^^^
                            End-of-frame marker
```










# Problems


## 🧩 Director Score File Anomalies (Keyframe & Channel Decoding)

### 1. ❓ Unknown Block Inside STARTKEYFRAME List

In `Animation_types.dir`, a non-keyframe block appears **in the middle** of an otherwise aligned 48-byte keyframe sequence:

```
00 02 03 D6 01 00 00 30 04 B0
```

- **10 bytes total**
- Begins like a tag: `00 02 03 D6` → possibly tag `0x03D6`
- Followed by values: `01 00 00 30 04 B0` — likely control or pointer data
- Appears at offset where a 48-byte keyframe would be expected

🔎 **Hypothesis**: This may be a jump table, channel pointer redirect, or offset-based redirection that replaces a normal keyframe.

---

### 2. 🟠 Repeated `0x0120` Bytes (Not Tags)

In both files, standalone `01 20` bytes are found:
- Before keyframe sequences
- Repeated multiple times
- Sometimes between blocks
- Not part of `00 02` or `00 04` tag structure

🧩 **They are not tags.**

#### Examples:
```
01 26
01 20
01 20
-- followed by --
10 00 FF 00 ...
```

🔎 **Hypothesis**: These may be block alignment markers, padding words, or undocumented structural separators in the score file format.

---

### 3. ⚠️ Missing Channel 10 in File 1

In `Animation_types.dir`, channel 10 (`tag = 0x01F6`) is completely missing:

| Tag     | Internal Channel | Present in File 1 |
|---------|------------------|--------------------|
| `0x0136` | 6               | ✅ Yes             |
| `0x0166` | 7               | ✅ Yes             |
| `0x0196` | 8               | ✅ Yes             |
| `0x01C6` | 9               | ✅ Yes             |
| `0x01F6` | 10              | ❌ **Missing**     |
| `0x0226` | 11              | ✅ Yes             |

In contrast, in `SpriteLock.dir`, channel 10 **is present**:
```
00 02 01 F6 81 00
```

🔎 **Conclusion**: Channels can be skipped. The decoding logic must allow for:
- Gaps between sprite channels
- Possible structural reasons for omission (e.g., unknown blocks acting as replacements)

---

### ✅ Summary & Action Points

- `0x0120` and similar standalone values must be logged but not decoded as tags
- 10-byte `0x03D6` blocks may explain missing channel data — more examples needed
- Keyframe parsing must tolerate mid-block interruptions
- Channel assignment cannot assume contiguous sprite use

🧪 These anomalies suggest the Director score file format includes control, segmentation, or optimization logic not yet fully mapped.
