from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import ReadStream
from ..rays_cast_member import RaysMemberType

class RaysCastMemberChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.CastMemberChunk)
        self.type = RaysMemberType.NullMember
        self.info_len = 0
        self.specific_data_len = 0

    def read(self, stream: ReadStream):
        self.type = RaysMemberType(stream.read_uint32())
        self.info_len = stream.read_uint32()
        self.specific_data_len = stream.read_uint32()
        stream.read_bytes(self.info_len)
        stream.read_bytes(self.specific_data_len)

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('type'); writer.write_val(self.type.name)
        writer.end_object()
