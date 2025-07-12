from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import BufferView, ReadStream, Endianness

class RaysListChunk(RaysChunk):
    def __init__(self, dir=None, chunk_type=ChunkType.CastListChunk):
        super().__init__(dir, chunk_type)
        self.data_offset = 0
        self.offset_table_len = 0
        self.offset_table = []
        self.items_len = 0
        self.item_endianness = Endianness.BigEndian
        self.items = []

    def read(self, stream: ReadStream):
        self.read_header(stream)
        self.read_offset_table(stream)
        self.read_items(stream)

    def read_header(self, stream: ReadStream):
        self.data_offset = stream.read_uint32()
        self.offset_table_len = stream.read_uint16()
        self.items_len = stream.read_uint32()
        self.item_endianness = stream.endianness

    def read_offset_table(self, stream: ReadStream):
        for _ in range(self.offset_table_len):
            self.offset_table.append(stream.read_uint32())

    def read_items(self, stream: ReadStream):
        for i in range(self.offset_table_len):
            start = self.offset_table[i]
            end = self.offset_table[i+1] if i + 1 < self.offset_table_len else self.items_len
            length = end - start
            if length <= 0:
                self.items.append(BufferView())
                continue
            self.items.append(BufferView(stream.read_bytes(length), 0, length))

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('offsetTable')
        writer.start_array()
        for o in self.offset_table:
            writer.write_val(o)
        writer.end_array()
        writer.end_object()
