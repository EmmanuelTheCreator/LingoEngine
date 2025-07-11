from ..chunks.rays_chunk import RaysChunk, ChunkType
from ..common.json_writer import JSONWriter
from ..common.stream import ReadStream
from .rays_score_frame_parser_v2 import RaysScoreFrameParserV2

class RaysScoreChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.ScoreChunk)
        self.sprites = []

    def read(self, stream: ReadStream):
        parser = RaysScoreFrameParserV2()
        self.sprites = parser.parse_score(stream)

    def write_json(self, writer: JSONWriter):
        writer.start_object()
        writer.write_key('frames')
        writer.start_array()
        for sp in self.sprites:
            writer.start_object()
            writer.write_key('start')
            writer.write_val(sp.start_frame)
            writer.write_key('end')
            writer.write_val(sp.end_frame)
            writer.write_key('sprite')
            writer.write_val(sp.sprite_number)
            writer.end_object()
        writer.end_array()
        writer.end_object()
