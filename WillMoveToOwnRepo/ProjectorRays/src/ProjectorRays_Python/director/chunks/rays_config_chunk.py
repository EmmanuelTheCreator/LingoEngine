from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import ReadStream

class RaysConfigChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.ConfigChunk)
        self.director_version = 0
        self.min_member = 0

    def read(self, stream: ReadStream):
        self.director_version = stream.read_uint16()
        self.min_member = stream.read_uint16()

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('directorVersion'); writer.write_val(self.director_version)
        writer.write_key('minMember'); writer.write_val(self.min_member)
        writer.end_object()
