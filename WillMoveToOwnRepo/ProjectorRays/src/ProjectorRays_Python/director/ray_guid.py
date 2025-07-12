from dataclasses import dataclass
from ..common.stream import ReadStream

@dataclass
class RayGuid:
    data1: int = 0
    data2: int = 0
    data3: int = 0
    data4: bytes = b"\x00" * 8

    def read(self, stream: ReadStream):
        self.data1 = stream.read_uint32()
        self.data2 = stream.read_uint16()
        self.data3 = stream.read_uint16()
        self.data4 = bytes(stream.read_bytes(8))

    def __str__(self) -> str:
        d = self.data4
        return f"{self.data1:08X}-{self.data2:04X}-{self.data3:04X}-{d[0]:02X}{d[1]:02X}-{d[2]:02X}{d[3]:02X}{d[4]:02X}{d[5]:02X}{d[6]:02X}{d[7]:02X}"

    def __eq__(self, other):
        if not isinstance(other, RayGuid):
            return False
        return (self.data1 == other.data1 and self.data2 == other.data2 and
                self.data3 == other.data3 and self.data4 == other.data4)


class LingoGuidConstants:
    FONTMAP_COMPRESSION_GUID = RayGuid(0x8A4679A1, 0x3720, 0x11D0,
                                        b"\x92\x23\x00\xA0\xC9\x08\x68\xB1")
    NULL_COMPRESSION_GUID = RayGuid(0xAC99982E, 0x005D, 0x0D50,
                                     b"\x00\x00\x08\x00\x07\x37\x7A\x34")
    SND_COMPRESSION_GUID = RayGuid(0x7204A889, 0xAFD0, 0x11CF,
                                    b"\xA2\x22\x00\xA0\x24\x53\x44\x4C")
    ZLIB_COMPRESSION_GUID = RayGuid(0xAC99E904, 0x0070, 0x0B36,
                                     b"\x00\x00\x08\x00\x07\x37\x7A\x34")
