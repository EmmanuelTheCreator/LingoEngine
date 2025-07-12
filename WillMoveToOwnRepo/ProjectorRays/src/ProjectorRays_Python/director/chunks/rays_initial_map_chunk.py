from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import ReadStream
from ..rays_subchunk import MemoryMapEntry

class RaysInitialMapChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.InitialMapChunk)
        self.mmap_offset = 0

    def read(self, stream: ReadStream):
        self.mmap_offset = stream.read_uint32()

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('mmapOffset'); writer.write_val(self.mmap_offset)
        writer.end_object()
