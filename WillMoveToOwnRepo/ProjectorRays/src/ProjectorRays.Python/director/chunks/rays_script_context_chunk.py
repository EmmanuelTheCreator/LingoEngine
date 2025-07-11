from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import ReadStream

class RaysScriptContextChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.ScriptContextChunk)
        self.context_data = b""

    def read(self, stream: ReadStream):
        self.context_data = stream.read_bytes(len(stream.data) - stream.pos)
