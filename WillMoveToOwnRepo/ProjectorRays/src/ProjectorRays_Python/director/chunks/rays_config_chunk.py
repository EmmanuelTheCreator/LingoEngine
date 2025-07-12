from .rays_chunk import RaysChunk, ChunkType
from ...common.stream import ReadStream

class RaysConfigChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.ConfigChunk)
        self.file_version = 0
        self.director_version = 0
        self.min_member = 0

    def read(self, stream: ReadStream):
        stream.set_endianness(self.dir.endianness if self.dir else stream.endianness)
        length = stream.read_int16()
        self.file_version = stream.read_int16()
        stream.skip(8)  # movie rect
        self.min_member = stream.read_int16()
        stream.skip(2)  # maxMember
        stream.seek(36)
        self.director_version = stream.read_int16()
        stream.seek(length)
        if self.dir:
            self.dir._logger.info(
                "ConfigChunk: FileVersion=%d, MinMember=%d, DirectorVersion=%d",
                self.file_version,
                self.min_member,
                self.director_version,
            )

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('fileVersion'); writer.write_val(self.file_version)
        writer.write_key('minMember'); writer.write_val(self.min_member)
        writer.write_key('directorVersion'); writer.write_val(self.director_version)
        writer.end_object()
