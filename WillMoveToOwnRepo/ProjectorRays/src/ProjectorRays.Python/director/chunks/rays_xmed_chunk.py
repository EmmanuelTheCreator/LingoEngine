from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import ReadStream

class RaysXmedChunk(RaysChunk):
    def __init__(self, dir=None, chunk_type=ChunkType.XmedChunk):
        super().__init__(dir, chunk_type)
        self.data = b""

    def read(self, stream: ReadStream):
        self.data = stream.read_bytes(len(stream.data) - stream.pos)
