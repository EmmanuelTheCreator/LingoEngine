from .rays_chunk import RaysChunk, ChunkType

class RaysCastChunk(RaysChunk):
    def __init__(self, dir=None):
        super().__init__(dir, ChunkType.CastChunk)
        self.member_ids = []

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
