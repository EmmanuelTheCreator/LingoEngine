from ..chunks.rays_chunk import RaysChunk, ChunkType
from ..common.stream import ReadStream, Endianness
from .data import RaySprite, RayScoreHeader, RayScoreIntervalDescriptor, RayScoreKeyFrame, RayTweenFlags
from . import ray_score_tags_v2 as tags
from .ray_sprite_factory import RaySpriteFactory

class RaysScoreReader:
    """Very small subset reader for score chunks."""
    def __init__(self, logger=None):
        self.logger = logger

    def read_main_header(self, stream: ReadStream) -> RayScoreHeader:
        header = RayScoreHeader()
        header.total_length = stream.read_uint32()
        header.header_type = stream.read_uint32()
        header.offsets_offset = stream.read_uint32()
        header.entry_count = stream.read_uint32()
        header.notation_base = stream.read_uint32()
        header.entry_size_sum = stream.read_uint32()
        return header

    def parse_tween_flags(self, value: int) -> RayTweenFlags:
        return RayTweenFlags(
            tweening_enabled=bool(value & 0x01),
            path=bool(value & 0x02),
            size=bool(value & 0x04),
            rotation=bool(value & 0x08),
            skew=bool(value & 0x10),
            blend=bool(value & 0x20),
            fore_color=bool(value & 0x40),
            back_color=bool(value & 0x80),
        )

    def create_channel_sprite(self, stream: ReadStream) -> RaySprite:
        sp = RaySprite()
        sp.tween_flags = self.parse_tween_flags(stream.read_uint8())
        stream.read_uint16()  # unknown
        sp.ink = stream.read_uint8()
        sp.fore_color = stream.read_uint8()
        sp.back_color = stream.read_uint8()
        sp.member_cast_lib = stream.read_uint16()
        sp.member_num = stream.read_uint16()
        stream.read_uint16()
        sp.sprite_properties_offset = stream.read_uint16()
        sp.loc_v = stream.read_int16()
        sp.loc_h = stream.read_int16()
        sp.height = stream.read_int16()
        sp.width = stream.read_int16()
        sp.editable = bool(stream.read_uint8() & 0x40)
        sp.blend = 100 - int(stream.read_uint8() * 100 / 255)
        flip = stream.read_uint8()
        sp.flip_v = bool(flip & 0x04)
        sp.flip_h = bool(flip & 0x02)
        stream.read_bytes(5)
        sp.rotation = stream.read_int32() / 100.0
        sp.skew = stream.read_int32() / 100.0
        return sp
    def apply_known_tag_sprite(self, sprite: RaySprite, tag: tags.ScoreTagV2, data: bytes):
        rs = ReadStream(data, Endianness.BIG)
        if tag == tags.ScoreTagV2.Ease:
            sprite.ease_in = rs.read_uint8()
            sprite.ease_out = rs.read_uint8()
        elif tag == tags.ScoreTagV2.Curvature:
            sprite.curvature = rs.read_uint16()
        elif tag == tags.ScoreTagV2.TweenFlags:
            sprite.tween_flags = self.parse_tween_flags(rs.read_uint8())

    def apply_known_tag_keyframe(self, kf: RayScoreKeyFrame, tag: tags.ScoreTagV2, data: bytes):
        rs = ReadStream(data, Endianness.BIG)
        if tag == tags.ScoreTagV2.Size:
            kf.width = rs.read_int16()
            kf.height = rs.read_int16()
        elif tag == tags.ScoreTagV2.Position:
            kf.loc_h = rs.read_int16()
            kf.loc_v = rs.read_int16()
        elif tag == tags.ScoreTagV2.Colors:
            kf.fore_color = rs.read_uint8()
            kf.back_color = rs.read_uint8()
        elif tag == tags.ScoreTagV2.Ink:
            kf.ink = rs.read_int16()
        elif tag == tags.ScoreTagV2.Rotation:
            kf.rotation = rs.read_int16() / 100.0
        elif tag == tags.ScoreTagV2.Skew:
            kf.skew = rs.read_int16() / 100.0
        elif tag == tags.ScoreTagV2.Composite:
            kf.width = rs.read_int16()
            kf.height = rs.read_int16()
            blend_raw = rs.read_uint8()
            kf.blend = 100 - int(blend_raw * 100 / 255)
            kf.ink = rs.read_uint8()
        elif tag == tags.ScoreTagV2.FrameRect:
            kf.loc_h = rs.read_int16()
            kf.loc_v = rs.read_int16()
            kf.width = rs.read_int16()
            kf.height = rs.read_int16()
