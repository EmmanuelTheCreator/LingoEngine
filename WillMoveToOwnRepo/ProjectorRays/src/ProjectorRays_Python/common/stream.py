import struct
from enum import Enum

class Endianness(Enum):
    BigEndian = '>'
    LittleEndian = '<'

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
    def __init__(self, data, endianness=Endianness.BigEndian, pos=0):
        if isinstance(data, BufferView):
            self.data = data.bytes
        else:
            self.data = data
        self.pos = pos
        self.endianness = endianness

    def set_endianness(self, endianness: Endianness):
        self.endianness = endianness

    def _read(self, count: int) -> bytes:
        data = self.data[self.pos:self.pos + count]
        self.pos += count
        return data

    def eof(self):
        return self.pos >= len(self.data)

    def read_bytes(self, count):
        return self._read(count)

    def read_uint8(self):
        return self.read_bytes(1)[0]

    def read_uint16(self):
        data = self._read(2)
        return int.from_bytes(
            data,
            byteorder='little' if self.endianness == Endianness.LittleEndian else 'big'
        )

    def read_uint32(self):
        data = self._read(4)
        return int.from_bytes(
            data,
            byteorder='little' if self.endianness == Endianness.LittleEndian else 'big'
        )

    def read_uint32_raw(self):
        data = self._read(4)
        return int.from_bytes(data, byteorder='big')

    def read_string(self, length):
        return self.read_bytes(length).decode('utf-8')

    # ---- Added helpers to mirror the C# implementation ----

    def seek(self, pos):
        self.pos = max(0, pos)

    def skip(self, n):
        self.pos = min(len(self.data), self.pos + n)

    def read_int8(self):
        return struct.unpack(self.endianness.value + 'b', self._read(1))[0]

    def read_int16(self):
        return struct.unpack(self.endianness.value + 'h', self._read(2))[0]

    def read_int32(self):
        return struct.unpack(self.endianness.value + 'i', self._read(4))[0]

    def read_float32(self):
        return struct.unpack(self.endianness.value + 'f', self._read(4))[0]

    def read_double(self):
        return struct.unpack(self.endianness.value + 'd', self._read(8))[0]

    def read_cstring(self):
        start = self.pos
        while self.pos < len(self.data) and self.data[self.pos] != 0:
            self.pos += 1
        result = self.data[start:self.pos].decode('utf-8')
        if self.pos < len(self.data):
            self.pos += 1
        return result

    def read_pascal_string(self):
        length = self.read_uint8()
        return self.read_string(length)

    def read_var_int(self):
        val = 0
        while True:
            b = self.read_uint8()
            val = (val << 7) | (b & 0x7F)
            if b & 0x80 == 0:
                break
        return val
