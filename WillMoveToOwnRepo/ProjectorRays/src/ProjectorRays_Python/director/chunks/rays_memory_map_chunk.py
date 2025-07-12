from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import ReadStream
from ..rays_subchunk import MemoryMapEntry


class RaysMemoryMapChunk(RaysChunk):
    """Mirror the .NET implementation for memory map parsing."""

    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.MemoryMapChunk)
        self.map_array = []
        self.header_len = 0
        self.entry_len = 0
        self.count_max = 0
        self.count_used = 0
        self.junk_head = 0
        self.junk_head2 = 0
        self.free_head = 0

    def read(self, stream: ReadStream):
        stream.set_endianness(self.dir.endianness if self.dir else stream.endianness)
        self.header_len = stream.read_uint16()
        self.entry_len = stream.read_uint16()
        self.count_max = stream.read_uint32()
        self.count_used = stream.read_uint32()
        self.junk_head = stream.read_int32()
        self.junk_head2 = stream.read_int32()
        self.free_head = stream.read_int32()
        for _ in range(self.count_used):
            entry = MemoryMapEntry()
            entry.read(stream)
            self.map_array.append(entry)
        if self.dir:
            self.dir._logger.info(
                "MemoryMapChunk: Entries=%d, HeaderLen=%d, EntryLen=%d, CountMax=%d, MapArray.Count=%d",
                self.count_used,
                self.header_len,
                self.entry_len,
                self.count_max,
                len(self.map_array),
            )

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('count'); writer.write_val(len(self.map_array))
        writer.end_object()
