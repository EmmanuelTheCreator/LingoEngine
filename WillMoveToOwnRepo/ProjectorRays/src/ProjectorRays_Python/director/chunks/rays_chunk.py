from enum import Enum
from ...common.rays_json_writer import RaysJSONWriter
from ...common.stream import ReadStream

class ChunkType(Enum):
    CastChunk = 'CastChunk'
    CastListChunk = 'CastListChunk'
    CastMemberChunk = 'CastMemberChunk'
    CastInfoChunk = 'CastInfoChunk'
    ConfigChunk = 'ConfigChunk'
    InitialMapChunk = 'InitialMapChunk'
    KeyTableChunk = 'KeyTableChunk'
    MemoryMapChunk = 'MemoryMapChunk'
    ScriptChunk = 'ScriptChunk'
    ScriptContextChunk = 'ScriptContextChunk'
    ScriptNamesChunk = 'ScriptNamesChunk'
    ScoreChunk = 'ScoreChunk'
    XmedChunk = 'XmedChunk'
    StyledText = 'StyledText'

class RaysChunk:
    def __init__(self, dir=None, chunk_type=None):
        self.dir = dir
        self.chunk_type = chunk_type
        self.writable = False
        self.data: bytes = b""

    def read(self, stream: ReadStream):
        """Default reader that consumes all remaining bytes."""
        remaining = len(stream.data) - stream.pos
        if remaining > 0:
            self.data = stream.read_bytes(remaining)

    def write_json(self, writer: RaysJSONWriter):
        writer.start_object()
        writer.write_key('chunkType')
        writer.write_val(self.chunk_type.value if isinstance(self.chunk_type, ChunkType) else self.chunk_type)
        writer.end_object()
