from .rays_list_chunk import RaysListChunk
from .rays_chunk import ChunkType
from ...common.stream import ReadStream
from ..rays_subchunk import CastListEntry

class RaysCastListChunk(RaysListChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.CastListChunk)
        self.cast_count = 0
        self.items_per_cast = 0
        self.entries = []

    def read_header(self, stream: ReadStream):
        self.data_offset = stream.read_uint32()
        stream.read_uint16()
        self.cast_count = stream.read_uint16()
        self.items_per_cast = stream.read_uint16()
        stream.read_uint16()
        self.offset_table_len = stream.read_uint16()
        self.items_len = stream.read_uint32()

    def read(self, stream: ReadStream):
        super().read(stream)
        for _ in range(self.cast_count):
            self.entries.append(CastListEntry())

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('count'); writer.write_val(len(self.entries))
        writer.end_object()
