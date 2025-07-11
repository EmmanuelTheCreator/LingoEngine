from .common.json_writer import JSONWriter
from .director.chunks.rays_cast_chunk import RaysCastChunk
from .common.stream import ReadStream, Endianness


def demo():
    writer = JSONWriter()
    writer.start_object()
    writer.write_key('message')
    writer.write_val('Hello from ProjectorRays Python')

    # simple stream demo
    data = b'\x00\x00\x00\x01\x00\x00\x00\x02'
    rs = ReadStream(data, Endianness.BIG)
    chunk = RaysCastChunk()
    chunk.read(rs)
    writer.write_key('castChunk')
    chunk_writer = JSONWriter()
    chunk.write_json(chunk_writer)
    writer.write_val(chunk_writer._root)
    writer.end_object()
    print(writer.to_string())

if __name__ == '__main__':
    demo()
