from .rays_list_chunk import RaysListChunk
from .rays_chunk import ChunkType
from ...common.stream import ReadStream
from ..rays_subchunk import MemoryMapEntry

class RaysMemoryMapChunk(RaysListChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.MemoryMapChunk)
        self.map_array = []

    def read(self, stream: ReadStream):
        super().read(stream)
        for view in self.items:
            rs = ReadStream(view, self.item_endianness)
            entry = MemoryMapEntry()
            entry.read(rs)
            self.map_array.append(entry)

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('count'); writer.write_val(len(self.map_array))
        writer.end_object()
