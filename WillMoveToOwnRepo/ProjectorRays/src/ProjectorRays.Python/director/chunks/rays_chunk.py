from enum import Enum
from ...common.json_writer import JSONWriter

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

    def read(self, stream):
        raise NotImplementedError

    def write_json(self, writer: JSONWriter):
        writer.start_object()
        writer.write_key('chunkType')
        writer.write_val(self.chunk_type.value if isinstance(self.chunk_type, ChunkType) else self.chunk_type)
        writer.end_object()
