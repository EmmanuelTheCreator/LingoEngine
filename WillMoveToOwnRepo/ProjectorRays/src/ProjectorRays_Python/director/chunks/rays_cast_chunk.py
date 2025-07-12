from .rays_chunk import RaysChunk, ChunkType

class RaysCastChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.CastChunk)
        self.member_ids = []
        self.name = ""
        self.members = {}
        self.lctx = None

    def read(self, stream):
        while not stream.eof():
            member_id = stream.read_uint32()
            self.member_ids.append(member_id)

    def write_json(self, writer):
        writer.start_object()
        writer.write_key('memberIDs')
        writer.start_array()
        for mid in self.member_ids:
            writer.write_val(mid)
        writer.end_array()
        writer.end_object()

    def populate(self, cast_name: str, cast_id: int, min_member: int) -> None:
        self.name = cast_name
        if not self.dir or not self.dir.key_table:
            return
        for entry in self.dir.key_table.entries:
            if (entry.cast_id == cast_id and
                    entry.fourcc in (self.dir.FOURCC('L','c','t','x'),
                                     self.dir.FOURCC('L','c','t','X')) and
                    self.dir.chunk_exists(entry.fourcc, entry.section_id)):
                self.lctx = self.dir.get_chunk(entry.fourcc, entry.section_id)
                break

        for idx, section_id in enumerate(self.member_ids):
            if section_id <= 0:
                continue
            chunk_id = -1
            for key_entry in self.dir.key_table.entries:
                if (key_entry.fourcc == self.dir.FOURCC('C','A','S','t') and
                        key_entry.cast_id == cast_id and
                        key_entry.resource_id == section_id):
                    chunk_id = key_entry.section_id
                    break
            if chunk_id < 0:
                chunk_id = section_id
            if not self.dir.chunk_exists(self.dir.FOURCC('C','A','S','t'), chunk_id):
                continue
            member = self.dir.get_chunk(self.dir.FOURCC('C','A','S','t'), chunk_id)
            member.id = idx + min_member
            self.members[member.id] = member
            script_id = getattr(member, 'get_script_id', lambda: 0)()
            if script_id and self.dir.chunk_exists(self.dir.FOURCC('L','s','c','r'), script_id):
                script_chunk = self.dir.get_chunk(self.dir.FOURCC('L','s','c','r'), script_id)
                member.script = script_chunk
                script_chunk.member = member
