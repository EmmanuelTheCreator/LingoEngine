from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import ReadStream

class RaysScriptChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.ScriptChunk)
        self.script_data = b""
        self.member = None

    def read(self, stream: ReadStream):
        self.script_data = stream.read_bytes(len(stream.data) - stream.pos)

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('length'); writer.write_val(len(self.script_data))
        writer.end_object()
