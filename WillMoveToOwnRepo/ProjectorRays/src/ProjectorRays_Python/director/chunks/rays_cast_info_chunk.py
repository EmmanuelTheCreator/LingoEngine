from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import ReadStream

class RaysCastInfoChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.CastInfoChunk)
        self.name = ''
        self.script_id = 0

    def read(self, stream: ReadStream):
        self.script_id = stream.read_uint32()

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('scriptId'); writer.write_val(self.script_id)
        writer.end_object()
