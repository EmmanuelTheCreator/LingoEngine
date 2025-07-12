from .rays_list_chunk import RaysListChunk
from .rays_chunk import ChunkType
from ...common.stream import ReadStream
from ..rays_subchunk import KeyTableEntry

class RaysKeyTableChunk(RaysListChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.KeyTableChunk)
        self.entries = []

    def read(self, stream: ReadStream):
        super().read(stream)
        for view in self.items:
            rs = ReadStream(view, self.item_endianness)
            entry = KeyTableEntry()
            entry.read(rs)
            self.entries.append(entry)

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('count'); writer.write_val(len(self.entries))
        writer.end_object()
