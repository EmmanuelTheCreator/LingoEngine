from ...common.stream import ReadStream, Endianness, BufferView
from .ray_score_parse_context import RayScoreParseContext
from .rays_score_reader import RaysScoreReader
from .ray_sprite_factory import RaySpriteFactory
from . import ray_score_tags_v2 as tags
from .data import RayScoreIntervalDescriptor, UnknownTag

class RaysScoreFrameParserV2:
    def __init__(self, logger=None):
        self.logger = logger
        self.reader = RaysScoreReader(logger)

    def parse_score(self, stream: ReadStream):
        stream.endianness = Endianness.BigEndian
        ctx = RayScoreParseContext(self.logger)
        header = self.reader.read_main_header(stream)
        self.reader.read_all_intervals(header.entry_count, stream, ctx)
        if ctx.frame_data_buffer_view is None:
            return []
        new_reader = ReadStream(ctx.frame_data_buffer_view, Endianness.BigEndian)
        self.reader.read_header(new_reader, header)
        self.reader.read_frame_descriptors(ctx)
        self.reader.read_behaviors(ctx)
        if self.logger:
            self.logger.debug(
                "Score header: size=%d frames=%d channels=%d",
                header.actual_size,
                header.highest_frame,
                header.channel_count,
            )
        self.parse_block(new_reader, len(new_reader.data), header, ctx)
        self.link_keyframes(ctx)
        return ctx.sprites

    # ------------------------------------------------------------------
    # The methods below are adapted from the C# implementation. They do not
    # implement the full feature set yet but provide structure compatible with
    # the original design.
    # ------------------------------------------------------------------

    def log_full_score_bytes(self, reader_source: ReadStream):
        frame_bytes = reader_source.read_bytes(len(reader_source.data) - reader_source.pos)
        frame_data = ReadStream(frame_bytes, reader_source.endianness)
        buf_view = BufferView(frame_bytes)
        hex_dump = buf_view.log_hex(len(frame_bytes))
        if self.logger:
            self.logger.info("FrameData:" + hex_dump)
        return

    def parse_block(self, stream: ReadStream, length: int, header, ctx: RayScoreParseContext):
        end = stream.pos + length
        ctx.block_depth += 1
        while stream.pos < end and (len(stream.data) - stream.pos) >= 2:
            prefix = stream.read_uint16()
            if prefix == 0:
                continue
            if prefix == tags.BLOCK_END:
                if ctx.is_in_advance_frame_mode:
                    ctx.is_in_advance_frame_mode = False
                    continue
                ctx.block_depth -= 1
                return

            if prefix == header.sprite_size:
                sp = self.parse_sprite_block(stream, header, ctx, header.sprite_size)
                ctx.set_current_sprite(sp)
                continue

            if prefix >= header.sprite_size:
                tag_main = tags.try_parse_tag_main(prefix)
                if tag_main is None:
                    if prefix > 1000 and self.logger:
                        self.logger.error(
                            f"Wrong byte reading in block at {stream.pos - 1}: X0{prefix:02X}({prefix})")
                        return
                    self.parse_block(stream, prefix, header, ctx)
                else:
                    if tag_main == tags.ScoreTagMain.ThisIsASpriteBlock:
                        sp = self.parse_sprite_block(stream, header, ctx, header.sprite_size)
                        ctx.set_current_sprite(sp)
                        continue
                continue

            tag = stream.read_uint16()
            data = stream.read_bytes(prefix) if prefix > 0 else b""
            self.handle_tag(tag, data, ctx)
        ctx.block_depth -= 1

    def parse_sprite_block(self, stream: ReadStream, header, ctx: RayScoreParseContext, length: int = -1):
        sprite = self.reader.create_channel_sprite(stream)
        channel = ctx.current_sprite_num + 6
        desc = ctx.channel_to_descriptor.get(channel)
        if desc is None:
            if self.logger:
                self.logger.error(
                    f"No descriptor found for channel {channel} at frame {ctx.current_frame}. Creating default descriptor.")
            desc = RayScoreIntervalDescriptor()
            desc.start_frame = ctx.current_frame
            desc.end_frame = ctx.current_frame
            desc.channel = channel
        sprite.behaviors.extend(desc.behaviors)
        sprite.extra_values.extend(desc.extra_values)
        sprite.sprite_number = channel
        ctx.add_sprite(sprite)
        return sprite

    def handle_tag(self, tag: int, data: bytes, ctx: RayScoreParseContext):
        if tag == 0x0180:
            if len(data) >= 2:
                ctx.upcoming_block_size = int.from_bytes(data[:2], 'big')
            return

        if tag == 0x0120:
            if len(data) > 0:
                ctx.set_current_sprite(ctx.get_or_create_sprite(data[0] + 6))
            return

        channel = self.try_decode_channel(tag)
        if channel is not None:
            ctx.set_current_sprite(ctx.get_or_create_sprite(channel))
            sprite = ctx.get_sprite(channel, ctx.current_frame)
            if len(data) >= 2:
                flags = int.from_bytes(data[:2], 'big')
                create_keyframe = (flags & 0x8000) != 0
                frames_to_advance = (flags & 0x7F00) >> 8
                if frames_to_advance == 0:
                    frames_to_advance = 1
                ctx.current_frame += frames_to_advance
                if create_keyframe and sprite is not None:
                    keyframe = RaySpriteFactory.create_keyframe(sprite, ctx.current_sprite_num, ctx.current_frame)
                    ctx.add_keyframe(keyframe)
                    ctx.current_keyframe = keyframe
            return

        known = tags.try_parse_tag(tag)
        if known is not None and ctx.current_sprite is not None:
            self.reader.apply_known_tag_sprite(ctx.current_sprite, known, data)

        if ctx.current_keyframe is not None:
            if known is not None:
                self.reader.apply_known_tag_keyframe(ctx.current_keyframe, known, data)
            else:
                ctx.current_keyframe.unknown_tags.append(UnknownTag(tag, data))
                if self.logger:
                    self.logger.info(
                        f"Unknown Spritenum={ctx.current_sprite_num} at frame={ctx.current_frame} : 0x{tag:02X}({tag})")

    def try_decode_channel(self, tag: int):
        if tag >= tags.ScoreTagV2.AdvanceFrame.value and (tag - 0x0136) % 0x30 == 0:
            return ((tag - 0x0136) // 0x30) + 6
        return None

    def link_keyframes(self, ctx: RayScoreParseContext):
        for kf in ctx.keyframes:
            sprite = ctx.get_sprite(kf.sprite_num, kf.frame_num)
            if sprite is not None:
                sprite.keyframes.append(kf)
            elif self.logger:
                self.logger.warning(
                    f"Keyframe for sprite {kf.sprite_num} at frame {kf.frame_num} not found in context. "
                    "This may indicate a parsing error or missing sprite data.")
