from typing import List, Dict
from ..common.stream import ReadStream, Endianness, BufferView
from ..common.json_writer import JSONWriter
from .chunks.rays_chunk import RaysChunk, ChunkType
from .chunks.rays_cast_chunk import RaysCastChunk
from .scores import RaysScoreChunk
from .rays_subchunk import KeyTableEntry
from .ray_guid import RayGuid

class ChunkInfo:
    def __init__(self):
        self.id = 0
        self.fourcc = 0
        self.length = 0
        self.uncompressed_len = 0
        self.offset = 0
        self.compression_id = RayGuid()

class RaysDirectorFile:
    def __init__(self, name: str = ""):
        self.name = name
        self.endianness = Endianness.BIG
        self.casts: List[RaysCastChunk] = []
        self.score: RaysScoreChunk | None = None
        self.chunk_info_map: Dict[int, ChunkInfo] = {}
        self.stream: ReadStream | None = None

    @staticmethod
    def FOURCC(a: str, b: str, c: str, d: str) -> int:
        return (ord(a) << 24) | (ord(b) << 16) | (ord(c) << 8) | ord(d)

    def read(self, stream: ReadStream) -> bool:
        self.stream = stream
        stream.endianness = Endianness.BIG
        self.endianness = stream.endianness
        return True

    def write_json(self, writer: JSONWriter):
        writer.start_object()
        writer.write_key('name'); writer.write_val(self.name)
        writer.end_object()
