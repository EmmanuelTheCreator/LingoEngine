from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import ReadStream

class ScriptNames:
    def __init__(self):
        self.names = []

class RaysScriptNamesChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.ScriptNamesChunk)
        self.names = ScriptNames()

    def read(self, stream: ReadStream):
        while not stream.eof():
            length = stream.read_uint8()
            self.names.names.append(stream.read_string(length))
