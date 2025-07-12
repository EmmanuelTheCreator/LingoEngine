from enum import Enum

class ScoreTagV2(Enum):
    Size = 0x0130
    Position = 0x015C
    Ease = 0x0120
    AdvanceFrame = 0x0136
    PathPart = 0x0166
    Ink = 0x0196
    Rotation = 0x019E
    Skew = 0x01A2
    Colors = 0x0212
    Composite = 0x0190
    BlockControl = 0x0180
    FrameRect = 0x01EC
    Curvature = 0x01F4
    Flags = 0x01FE
    FlagsControl = 0x01FC
    TweenFlags = 0x01F6
    Unknown012A = 0x012A
    Unknown013E = 0x013E
    Unknown0142 = 0x0142

class ScoreTagMain(Enum):
    NextByteIsSpriteIf10Hex = 0x0120
    ThisIsASpriteBlock = 0x1000

BLOCK_END = 0x0008
SPRITE_BLOCK_PREFIX = 0x0030
KEYFRAME_CREATE_FLAG = 0x8100
KEYFRAME_NO_CREATE_FLAG = 0x0100

def try_parse_tag_main(raw):
    try:
        return ScoreTagMain(raw)
    except ValueError:
        return None

def try_parse_tag(raw):
    try:
        return ScoreTagV2(raw)
    except ValueError:
        return None

def get_data_length(tag: ScoreTagV2) -> int:
    return {
        ScoreTagV2.Size: 4,
        ScoreTagV2.Position: 4,
        ScoreTagV2.Ease: 2,
        ScoreTagV2.AdvanceFrame: 2,
        ScoreTagV2.PathPart: 2,
        ScoreTagV2.Ink: 2,
        ScoreTagV2.Rotation: 2,
        ScoreTagV2.Skew: 2,
        ScoreTagV2.Colors: 2,
        ScoreTagV2.Composite: 6,
        ScoreTagV2.FrameRect: 8,
        ScoreTagV2.Curvature: 2,
        ScoreTagV2.Flags: 2,
        ScoreTagV2.FlagsControl: 2,
        ScoreTagV2.TweenFlags: 1,
        ScoreTagV2.BlockControl: 2,
    }.get(tag, 0)
