from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import ReadStream
from ..rays_subchunk import MemoryMapEntry

class RaysInitialMapChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.InitialMapChunk)
        self.version = 0
        self.mmap_offset = 0
        self.director_version = 0

    def read(self, stream: ReadStream):
        self.version = stream.read_uint32()
        self.mmap_offset = stream.read_uint32()
        self.director_version = stream.read_uint32()
        stream.skip(8)
        if self.dir:
            self.dir._logger.info(
                "InitialMapChunk: Version=%d, MmapOffset=%d, DirectorVersion=%d",
                self.version,
                self.mmap_offset,
                self.director_version,
            )

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('version'); writer.write_val(self.version)
        writer.write_key('mmapOffset'); writer.write_val(self.mmap_offset)
        writer.write_key('directorVersion'); writer.write_val(self.director_version)
        writer.end_object()
