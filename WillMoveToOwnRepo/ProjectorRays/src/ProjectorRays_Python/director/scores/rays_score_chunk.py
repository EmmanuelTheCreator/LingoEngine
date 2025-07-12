from ..chunks.rays_chunk import RaysChunk, ChunkType
from ...common.json_writer import JSONWriter
from ...common.stream import ReadStream
from .rays_score_frame_parser_v2 import RaysScoreFrameParserV2

class RaysScoreChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.ScoreChunk)
        self.sprites = []

    def read(self, stream: ReadStream):
        parser = RaysScoreFrameParserV2()
        self.sprites = parser.parse_score(stream)

    def write_json(self, writer: JSONWriter):
        """Serialize sprite interval data to JSON."""

        writer.start_object()
        writer.write_key("frames")
        writer.start_array()

        for sp in self.sprites:
            writer.start_object()

            writer.write_key("start")
            writer.write_val(sp.start_frame)

            writer.write_key("end")
            writer.write_val(sp.end_frame)

            writer.write_key("sprite")
            writer.write_val(sp.sprite_number)

            writer.write_key("MemberCastLib")
            writer.write_val(sp.member_cast_lib)

            writer.write_key("MemberNum")
            writer.write_val(sp.member_num)

            writer.write_key("spritePropertiesOffset")
            writer.write_val(sp.sprite_properties_offset)

            writer.write_key("locH")
            writer.write_val(sp.loc_h)

            writer.write_key("locV")
            writer.write_val(sp.loc_v)

            writer.write_key("width")
            writer.write_val(sp.width)

            writer.write_key("height")
            writer.write_val(sp.height)

            writer.write_key("rotation")
            writer.write_val(sp.rotation)

            writer.write_key("skew")
            writer.write_val(sp.skew)

            writer.write_key("ink")
            writer.write_val(sp.ink)

            writer.write_key("foreColor")
            writer.write_val(sp.fore_color)

            writer.write_key("backColor")
            writer.write_val(sp.back_color)

            writer.write_key("scoreColor")
            writer.write_val(sp.score_color)

            writer.write_key("blend")
            writer.write_val(sp.blend)

            writer.write_key("flipH")
            writer.write_val(sp.flip_h)

            writer.write_key("flipV")
            writer.write_val(sp.flip_v)

            writer.write_key("editable")
            writer.write_val(sp.editable)

            writer.write_key("extrValues")
            writer.write_val(sp.extra_values)

            writer.end_object()

        writer.end_array()
        writer.end_object()
