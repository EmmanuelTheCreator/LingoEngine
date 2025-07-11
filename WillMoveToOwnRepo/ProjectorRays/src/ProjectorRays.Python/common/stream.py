import struct
from enum import Enum

class Endianness(Enum):
    BIG = '>'
    LITTLE = '<'

class BufferView:
    def __init__(self, data=b"", offset=0, size=None):
        self.data = data
        self.offset = offset
        self.size = len(data) if size is None else size

    @property
    def bytes(self):
        return self.data[self.offset:self.offset + self.size]

    def log_hex(self, length=256, bytes_per_line=16):
        limit = min(self.size, length)
        lines = []
        for i in range(0, limit, bytes_per_line):
            chunk = self.bytes[i:i+bytes_per_line]
            hex_part = ' '.join(f"{b:02X}" for b in chunk)
            lines.append(f"{i:04X}: {hex_part}")
        return '\n'.join(lines)

class ReadStream:
    def __init__(self, data, endianness=Endianness.BIG, pos=0):
        if isinstance(data, BufferView):
            self.data = data.bytes
        else:
            self.data = data
        self.pos = pos
        self.endianness = endianness

    def eof(self):
        return self.pos >= len(self.data)

    def read_bytes(self, n):
        if self.pos + n > len(self.data):
            raise EOFError('Read past end of stream')
        b = self.data[self.pos:self.pos+n]
        self.pos += n
        return b

    def read_uint8(self):
        return self.read_bytes(1)[0]

    def read_uint16(self):
        return struct.unpack(self.endianness.value + 'H', self.read_bytes(2))[0]

    def read_uint32(self):
        return struct.unpack(self.endianness.value + 'I', self.read_bytes(4))[0]

    def read_string(self, length):
        return self.read_bytes(length).decode('utf-8')
