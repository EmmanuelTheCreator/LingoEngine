from .rays_list_chunk import RaysListChunk
from .rays_chunk import ChunkType
from ...common.stream import ReadStream, Endianness
from ..rays_subchunk import CastListEntry

class RaysCastListChunk(RaysListChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.CastListChunk)
        self.cast_count = 0
        self.items_per_cast = 0
        self.entries = []

    def read_header(self, stream: ReadStream):
        # Cast list headers are always stored in big endian like the C# reader
        stream.set_endianness(Endianness.BigEndian)
        self.data_offset = stream.read_uint32()
        stream.read_uint16()
        self.cast_count = stream.read_uint16()
        self.items_per_cast = stream.read_uint16()
        stream.read_uint16()
        self.offset_table_len = stream.read_uint16()
        self.items_len = stream.read_uint32()
        if self.dir:
            self.dir._logger.info(
                "CastListChunk: DataOffset=%d, CastCount=%d, ItemsPerCast=%d, OffsetTableLen=%d,ItemsLen=%d, ItemEndianness=%s",
                self.data_offset,
                self.cast_count,
                self.items_per_cast,
                self.offset_table_len,
                self.items_len,
                self.item_endianness.name,
            )

    def read(self, stream: ReadStream):
        super().read(stream)
        for _ in range(self.cast_count):
            self.entries.append(CastListEntry())

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('count'); writer.write_val(len(self.entries))
        writer.end_object()
