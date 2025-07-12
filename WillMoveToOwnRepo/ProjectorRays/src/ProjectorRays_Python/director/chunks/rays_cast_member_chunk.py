from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import ReadStream
from ..rays_cast_member import RaysMemberType

class RaysCastMemberChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.CastMemberChunk)
        self.type = RaysMemberType.NullMember
        self.info_len = 0
        self.specific_data_len = 0
        self.info = None
        self.specific_data = b""
        self.member = None
        self.has_flags1 = False
        self.flags1 = 0
        self.id = 0
        self.script = None
        self.decoded_text = None

    def read(self, stream: ReadStream):
        self.type = RaysMemberType(stream.read_uint32())
        self.info_len = stream.read_uint32()
        self.specific_data_len = stream.read_uint32()
        if self.info_len > 0:
            from .rays_cast_info_chunk import RaysCastInfoChunk
            info_stream = ReadStream(stream.read_bytes(self.info_len), stream.endianness)
            self.info = RaysCastInfoChunk(self.dir)
            self.info.read(info_stream)
        else:
            stream.read_bytes(0)
        self.specific_data = stream.read_bytes(self.specific_data_len)

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('type'); writer.write_val(self.type.name)
        writer.end_object()

    def get_script_id(self) -> int:
        return getattr(self.info, 'script_id', 0) if self.info else 0

    def get_script_text(self) -> str:
        return getattr(self.info, 'script_src_text', '') if self.info else ''

    def set_script_text(self, val: str) -> None:
        if self.info is not None:
            setattr(self.info, 'script_src_text', val)

    def get_name(self) -> str:
        return getattr(self.info, 'name', '') if self.info else ''

    @staticmethod
    def extract_text_from_member(member: 'RaysCastMemberChunk') -> str:
        data = getattr(member, 'specific_data', b"")
        if not data:
            return ''
        if len(data) > 1 and data[0] <= len(data) - 1:
            return data[1:1+data[0]].decode('utf-8', 'replace')
        pos = data.find(0)
        if pos < 0:
            pos = len(data)
        return data[:pos].decode('utf-8', 'replace')

    def get_text(self) -> str:
        if self.type in (RaysMemberType.TextMember, RaysMemberType.FieldMember):
            return RaysCastMemberChunk.extract_text_from_member(self)
        return ''
