from dataclasses import dataclass, field
from ..common.stream import ReadStream
from ..common.rays_json_writer import RaysJSONWriter

@dataclass
class CastListEntry:
    name: str = ''
    file_path: str = ''
    preload_settings: int = 0
    min_member: int = 0
    max_member: int = 0
    id: int = 0

    def write_json(self, writer: RaysJSONWriter):
        writer.start_object()
        writer.write_key('name'); writer.write_val(self.name)
        writer.write_key('filePath'); writer.write_val(self.file_path)
        writer.write_key('preloadSettings'); writer.write_val(self.preload_settings)
        writer.write_key('minMember'); writer.write_val(self.min_member)
        writer.write_key('maxMember'); writer.write_val(self.max_member)
        writer.write_key('id'); writer.write_val(self.id)
        writer.end_object()

@dataclass
class MemoryMapEntry:
    fourcc: int = 0
    length: int = 0
    offset: int = 0
    flags: int = 0
    unknown0: int = 0
    next: int = 0

    def read(self, stream: ReadStream):
        self.fourcc = stream.read_uint32()
        self.length = stream.read_uint32()
        self.offset = stream.read_uint32()
        self.flags = stream.read_uint16()
        self.unknown0 = stream.read_uint16()
        self.next = stream.read_uint32()

    def write_json(self, writer: RaysJSONWriter):
        writer.start_object()
        writer.write_key('fourCC'); writer.write_val(self.fourcc)
        writer.write_key('len'); writer.write_val(self.length)
        writer.write_key('offset'); writer.write_val(self.offset)
        writer.write_key('flags'); writer.write_val(self.flags)
        writer.write_key('unknown0'); writer.write_val(self.unknown0)
        writer.write_key('next'); writer.write_val(self.next)
        writer.end_object()

@dataclass
class KeyTableEntry:
    fourcc: int = 0
    resource_id: int = 0
    section_id: int = 0
    cast_id: int = 0

    def read(self, stream: ReadStream):
        self.fourcc = stream.read_uint32()
        self.resource_id = stream.read_uint16()
        self.section_id = stream.read_uint16()
        self.cast_id = stream.read_uint32()

    def write_json(self, writer: RaysJSONWriter):
        writer.start_object()
        writer.write_key('fourCC'); writer.write_val(self.fourcc)
        writer.write_key('resourceID'); writer.write_val(self.resource_id)
        writer.write_key('sectionID'); writer.write_val(self.section_id)
        writer.write_key('castID'); writer.write_val(self.cast_id)
        writer.end_object()
