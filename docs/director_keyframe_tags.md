# Director Keyframe Tags Documentation

## Core Concepts

This document outlines all currently understood fixed and variable tag formats used in Director's keyframe system. It includes control blocks, tag definitions, and observed channel mappings across various `.dir` files.

---

# 🧱 Director Keyframe Block Header Bytes

This section documents the **header structure** found at the beginning of each Director Score keyframe block (VWSC). These values are typically fixed across files, except for `HighestFrameNumber`, `actualSize`, and occasionally `unkD1`.

---

## 🧾 Header Structure (Offsets 0x00–0x13)

| Offset | Field                 | Type     | Meaning                        | Example Values     |
|--------|------------------------|----------|--------------------------------|---------------------|
| 0x00   | actualSize            | int32    | Total size of the keyframe block | Varies (e.g., 670)  |
| 0x04   | unkA1                 | int8     | Constant? Usually 0             | 0                   |
| 0x05   | unkA2                 | int8     | Constant? Usually 0             | 0                   |
| 0x06   | unkA3                 | int8     | Constant? Usually 0             | 0                   |
| 0x07   | unkA4                 | int8     | Unknown – often 20              | 20                  |
| 0x08   | HighestFrameNumber    | int32    | Highest referenced frame        | 6–55                |
| 0x0C   | unkB1                 | int8     | Constant?                       | 0                   |
| 0x0D   | unkB2                 | int8     | Unknown                         | 13                  |
| 0x0E   | spriteSize            | int16    | bytes count of fixed sprite block      | 48 (always)         |
| 0x10   | unkC1                 | int8     | Constant?                       | 3                   |
| 0x11   | unkC2                 | int8     | Unknown                         | -18                 |
| 0x12   | SpriteChannelCount    | int16    | Max channel slots available     | 150                 |
| 0x14   | unkD1                 | int16    | Possibly tag count or pointer   | 8–610               |

---

## 📂 Sample Files & Header Values

| File                        | actualSize | HighestFrameNumber | SpriteSize | SpriteChannelCount | unkD1 |
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
- `unkD1` **may** point to an offset or block size, possibly related to tag lists — **TBD**.

---

## Control Prefixes (Header Byte Sequences)

| Prefix       | Meaning                                  | Notes                                                                 |
|--------------|-------------------------------------------|-----------------------------------------------------------------------|
| `00 02`      | Repeat/advance tag                        | Most common prefix for reusing a previously used tag                 |
| `00 08`      | End of bytes-'frame'                      | Signals end of a keyframe block or transition to next logical frame |
| `00 30`      | Read keyframe: full 48 bytes              | Triggers fixed-format sprite block parsing (used in main keyframes) |
| `00 0A`/`00 0C`| Variable-length frame descriptor         | Seen in complex frames (often followed by tag `01 EC`)              |
| `00 36`      | Fixed block size = 54 bytes               | Indicates exact size of keyframe payload                             |

---

## Known Keyframe Tags (Bytes 2–3)

These tags appear after the prefix (`00 02`, `00 04`, etc.) and identify the type of operation or sprite property. 

| Tag       | Name           | Byte Size | Meaning                                    | Details                                                             |
|-----------|----------------|-----------|--------------------------------------------|---------------------------------------------------------------------|
| `01 30`   | Size           | 4         | Width + Height                             | 2 bytes each                                                        |
| `01 5C`   | Position       | 4         | LocH + LocV                                | 2 bytes each                                                        |
| `01 20`   | EaseIn/EaseOut | 2         | Ease-In + Ease-Out                         | First byte = EaseIn, Second = EaseOut, 0–255                        |
| `01 36`   | AdvanceFrame   | 2         | Moves to next frame                        | Keyframe continuation                                               |
| `01 66`   | ?              | 2         | Often follows `AdvanceFrame`               | Possibly part of path                                               |
| `01 96`   | Ink            | 2         | Drawing ink effect                         | Sprite ink constant                                                 |
| `01 9E`   | Rotation       | 2         | Fixed-point degrees                        | /100.0 precision                                                    |
| `01 A2`   | Skew           | 2         | Fixed-point skew angle                     | /100.0 precision                                                    |
| `01 EC`   | FrameRect      | 8         | LocH, LocV, Width, Height                  | Complex keyframe frame rectangle                                    |
| `01 FE`   | Flags          | 2         | Frame mode flags?                          | Purpose unknown                                                     |
| `01 FC`   | Flags/Control  | 2         | Possibly puppetSprite or global mode       | Only found in transitions                                           |
| `01 F4`   | Curvature      | 2         | Tween curvature                            | Single byte: 0–255 = 0–100% approx                                  |
| `01 90`   | Combo Tag      | 8         | Width, Height, Blend, Ink                  | Composite tag seen in complex frames                                |
| `01 F6`   | Tween Flags    | 1         | Bitmask flags for tweening properties      | Bit 1=Path, 2=Size, 3=Rotation, 4=Skew, 5=Blend, 6=Foreground 7=Background      |
| `01 80`   | BlockSize      | -         | Number of following bytes                  | Seen after `unkD1`                                                  |
| `01 20`   | ?              | -         | Possibly memory offset or data pointer     | Always follows `01 80`                                              |
| `02 12`   | Colors         | 2         | Foreground + Background                    | 1 byte each                                                         |
| `02 26`   | ?              | 2         | Possibly palette?                          | Unknown                                                             |
| `02 56`   | ?              | 2         | Unknown purpose                            | Unknown                                                             |
| `02 86`   | ?              | 2         | Unknown purpose                            | Unknown                                                             |
| `02 02`   | ?              | 2         | Possibly transition-related                | Appears in complex transitions                                      |

---

PS.: Tweening: The tweening are not keyframe based but sprite based!





## 🧩 Offset Diagram for Composite Tags


| Tag     | Total Size | Offset | Field         | Description                            |
|----------|------------|--------|----------------|----------------------------------------|
| `0x0130` | 4 bytes    | 0–1    | Width          | Sprite width (pixels)                  |
|          |            | 2–3    | Height         | Sprite height (pixels)                 |
| `0x015C` | 4 bytes    | 0–1    | LocH           | Horizontal screen position             |
|          |            | 2–3    | LocV           | Vertical screen position               |
| `0x0182` | 2 bytes    | 0      | ForeColor      | Foreground color (palette index)       |
|          |            | 1      | BackColor      | Background color (palette index)       |
| `0x0190` | 6 bytes    | 0–1    | Width          | Sprite width                           |
|          |            | 2–3    | Height         | Sprite height                          |
|          |            | 4      | Blend          | Blend percentage (byte, 0–255)         |
|          |            | 5      | Ink            | Drawing ink mode (constant index)      |
| `0x01EC` | 8 bytes    | 0–1    | LocH           | Frame rectangle left                   |
|          |            | 2–3    | LocV           | Frame rectangle top                    |
|          |            | 4–5    | Width          | Frame rectangle width                  |
|          |            | 6–7    | Height         | Frame rectangle height                 |
| `0x0212` | 2 bytes    | 0      | ForeColor      | Foreground color index (1 byte)        |
|          |            | 1      | BackColor      | Background color index (1 byte)        |
| `0x01F6` | 1 byte     | 0      | Tween Flags    | Bit flags for tweening fields (see below) |


## 🧭 Director Tag Ranges (Hypothesis)

| Range         | Purpose / Group                 | Examples                              | Notes |
|---------------|----------------------------------|----------------------------------------|-------|
| `0x0100–0x01FF` | Sprite and tweening tags         | `0130` (Size), `015C` (Position), `0120` (Ease), `01F4` (Curvature), `01F6` (TweenFlags) | Most visible and animation-related data |
| `0x0200–0x02FF` | Color, palette, and extensions   | `0212` (Colors), `0226` (Palette?)     | Likely color and palette info |
| `0x0300–0x03FF` | Unknown / special control tags   | `0316` (?), sometimes with `81 00`     | Rare in normal keyframes |
| `0x0400–0x04FF` | Per-channel offsets and repeats  | `04C0`, `0430`, `04B0`                 | Possibly channel bounding or inheritance |
| `0x1000–0x1FFF` | Multi-sprite or multi-frame tags | `1030`, `13C0`, `1CF0`, `1CF6`         | Consistently seen repeating block data |
| `0xFF00–0xFFFF` | Frame/Keyframe header            | `FF00`, etc.                           | Not real tags; used in STARTKEYFRAME headers |



## Sprite Channel Mapping (Also Tags!)

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

These are used to direct the upcoming data to the correct sprite channel.

---

## Reading Logic

- `2 bytes`: Payload length (after the tag)
- `2 bytes`: Tag value (property or sprite channel)
- `X bytes`: Payload data

Special handling is required for composite tags (e.g. `01 90`), frame-level headers (e.g. `01 EC`), and control sequences (`00 08`, `00 0A`, etc).


This table lists all **tags observed to contain multiple sprite properties**, drawn from real data in `5spritesTest.dir` and `KeyFramesTest.dir`. These are not followed by separate property tags — the values are bundled directly in the payload.

| Tag Name     | Hex     | Properties Included                           | Bit Representation (binary) | Notes |
|--------------|---------|-----------------------------------------------|-----------------------------|-------|
| **Size**     | `0x0130`| Width, Height                                 | `0001 0011 0000`            | Standard size tag (2 + 2 bytes) |
| **Position** | `0x015C`| LocH, LocV                                    | `0001 0101 1100`            | Appears in path-related blocks |
| **Colors**   | `0x0182`| ForeColor (1 byte), BackColor (1 byte)        | `0001 1000 0010`            | Often paired with `0x0190`     |
| **Composite**| `0x0190`| Width, Height, Blend, Ink                     | `0001 1001 0000`            | Frequently used; always 6–7 bytes |
| **Extended Frame Flags** | `0x01FE` | Mode/transition flags (unclear)     | `0001 1111 1110`            | Found in many frame blocks     |
| **Block Control** | `0x0180`| Possibly: Size, Channel count, Offsets? | `0001 1000 0000`            | Payload = 6+ bytes, unknown format |

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



### Example:

`0x0190` → `0001 1001 0000`  
Possibly means:
- Bit 4 → **Skew**
- Bit 5 → **Blend**
- Bit 7 → **Size**

→ **Size + Blend + Skew**, which matches reality.


## Observations

- A header byte (`unkD1`) often points to a count of tags or blocks. Not discovered yet.