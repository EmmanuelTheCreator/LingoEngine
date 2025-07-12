from .rays_list_chunk import RaysListChunk
from .rays_chunk import ChunkType
from ...common.stream import ReadStream
from ..rays_subchunk import KeyTableEntry

class RaysKeyTableChunk(RaysListChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.KeyTableChunk)
        self.entries = []

    def read(self, stream: ReadStream):
        stream.set_endianness(self.dir.endianness if self.dir else stream.endianness)
        self.entry_size = stream.read_uint16()
        stream.skip(2)
        self.count = stream.read_uint32()
        self.used = stream.read_uint32()
        for _ in range(self.used):
            entry = KeyTableEntry()
            entry.read(stream)
            self.entries.append(entry)
        if self.dir:
            self.dir._logger.info(
                "KeyTableChunk: EntrySize=%d, Count=%d, Used=%d, ParsedEntries=%d",
                self.entry_size,
                self.count,
                self.used,
                len(self.entries),
            )

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('count'); writer.write_val(len(self.entries))
        writer.end_object()
