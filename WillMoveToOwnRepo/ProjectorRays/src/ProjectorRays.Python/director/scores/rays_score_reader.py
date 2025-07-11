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

    def read_main_tag(self, data: int, stream: ReadStream, ctx):
        return tags.try_parse_tag_main(data)

    # ------------------------------------------------------------------
    # Additional methods ported from the C# implementation
    # ------------------------------------------------------------------

    def read_all_intervals(self, entry_count: int, stream: ReadStream, ctx):
        from ..common.stream import BufferView
        s = ReadStream(BufferView(stream.data, 0, len(stream.data)), stream.endianness, stream.pos)
        offsets = [s.read_int32() for _ in range(entry_count + 1)]
        entries_start = s.pos
        if entry_count < 1:
            return
        size = offsets[1] - offsets[0]
        absolute_start = entries_start + offsets[0]
        ctx.set_frame_data_buffer_view(stream.data, absolute_start, size)
        interval_order = []
        if entry_count >= 2:
            size = offsets[2] - offsets[1]
            absolute_start2 = entries_start + offsets[1]
            order_view = BufferView(stream.data, absolute_start2, offsets[2] - offsets[1])
            os = ReadStream(order_view, Endianness.BIG)
            if len(os.data) >= 4:
                count = os.read_int32()
                for i in range(count):
                    if os.pos + 4 <= len(os.data):
                        interval_order.append(os.read_int32())

        entry_indices = interval_order if interval_order else None
        if entry_indices is None:
            entry_indices = []
            i = 3
            while i + 2 < len(offsets):
                entry_indices.append(i)
                i += 3
        ctx.reset_frame_descriptor_buffers()
        for primary_idx in entry_indices:
            if primary_idx + 2 >= len(offsets):
                continue
            size = offsets[primary_idx + 1] - offsets[primary_idx]
            absolute_start2 = entries_start + offsets[primary_idx]
            ctx.add_frame_descriptor_buffer(BufferView(stream.data, absolute_start2, size))
            sec_size = offsets[primary_idx + 2] - offsets[primary_idx + 1]
            if sec_size > 0:
                absolute_start3 = entries_start + offsets[primary_idx + 1]
                ctx.add_behavior_script_buffer(BufferView(stream.data, absolute_start3, sec_size))

    def read_header(self, stream: ReadStream, header: RayScoreHeader):
        header.actual_size = stream.read_int32()
        header.unk_a1 = stream.read_uint8()
        header.unk_a2 = stream.read_uint8()
        header.unk_a3 = stream.read_uint8()
        header.unk_a4 = stream.read_uint8()
        header.highest_frame = stream.read_int32()
        header.unk_b1 = stream.read_uint8()
        header.unk_b2 = stream.read_uint8()
        header.sprite_size = stream.read_int16()
        header.unk_c1 = stream.read_uint8()
        header.unk_c2 = stream.read_uint8()
        header.channel_count = stream.read_int16()

    def read_frame_interval_descriptor(self, index: int, stream: ReadStream):
        if len(stream.data) < 44:
            return None
        desc = RayScoreIntervalDescriptor()
        desc.start_frame = stream.read_int32()
        desc.end_frame = stream.read_int32()
        desc.unknown1 = stream.read_int32()
        desc.unknown2 = stream.read_int16()
        flags = stream.read_int16()
        desc.flip_h = bool(flags & 0x01)
        desc.flip_v = bool(flags & 0x40)
        desc.editable = bool(flags & 0x20)
        desc.moveable = bool(flags & 0x10)
        desc.trails = bool(flags & 0x08)
        desc.is_locked = bool(flags & 0x04)

        desc.channel = stream.read_int32()
        desc.unknown_always_one = stream.read_int16()
        desc.unkown_a = stream.read_int16()
        desc.unkown_b = stream.read_int16()
        desc.unknown_e1 = stream.read_uint8()
        desc.unknown_fd = stream.read_uint8()
        desc.unknown7 = stream.read_int16()
        desc.unknown8 = stream.read_int32()
        while stream.pos + 4 <= len(stream.data):
            desc.extra_values.append(stream.read_int32())
        return desc

    def read_frame_descriptors(self, ctx):
        ctx.reset_frame_descriptors()
        for ind, buf in enumerate(ctx.frame_interval_descriptor_buffers):
            ps = ReadStream(buf, Endianness.BIG)
            descriptor = self.read_frame_interval_descriptor(ind, ps)
            if descriptor is not None:
                ctx.add_frame_descriptor(descriptor)

    def read_behaviors(self, ctx):
        for ind, buf in enumerate(ctx.behavior_script_buffers):
            ps = ReadStream(buf, Endianness.BIG)
            behaviour_refs = self.read_behaviors_block(ind, ps)
            if ind < len(ctx.descriptors):
                ctx.descriptors[ind].behaviors.extend(behaviour_refs)
            ctx.add_frame_script(behaviour_refs)

    def read_behaviors_block(self, ind: int, stream: ReadStream):
        from .data import RaysBehaviourRef
        behaviours = []
        while stream.pos + 8 <= len(stream.data):
            cl = stream.read_int16()
            cm = stream.read_int16()
            stream.read_int32()
            behaviours.append(RaysBehaviourRef(cl, cm))
        return behaviours
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

