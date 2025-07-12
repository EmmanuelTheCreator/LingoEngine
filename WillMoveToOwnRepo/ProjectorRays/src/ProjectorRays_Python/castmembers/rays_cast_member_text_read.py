from dataclasses import dataclass, field
from typing import List

from ..common.stream import ReadStream, Endianness, BufferView
from .ray_color import RayColor


@dataclass
class TextStyleRun:
    start: int = 0
    length: int = 0
    font_name: str = ""
    font_size: int = 0
    bold: bool = False
    italic: bool = False
    underline: bool = False
    text: str = ""
    font_id: int = 0
    fore_color: RayColor = field(default_factory=RayColor)
    back_color: RayColor = field(default_factory=RayColor)


class XmedChunkParser:
    @staticmethod
    def parse(view: BufferView) -> List[TextStyleRun]:
        rs = ReadStream(view, Endianness.LittleEndian)
        if rs.read_string(4) != "DEMX":
            raise ValueError("Invalid XMED chunk header")
        rs.pos = 128
        runs: List[TextStyleRun] = []
        while not rs.eof():
            pos = rs.pos
            marker = rs.read_uint8()
            if marker == 0x35 and not rs.eof() and rs.data[rs.pos] == ord(','):
                rs.pos = pos
                rtf_text = b""
                while not rs.eof():
                    ch = rs.read_uint8()
                    if ch == 0:
                        break
                    rtf_text += bytes([ch])
                parts = rtf_text.decode('latin1').split(',', 1)
                text = parts[1] if len(parts) > 1 else ''
                runs.append(TextStyleRun(text=text, fore_color=RayColor(0, 8, 255)))
                break
        return runs


class RaysCastMemberTextRead:
    def __init__(self):
        self.text = ""
        self.styles: List[TextStyleRun] = []
        self.text_parts: List[str] = []

    @staticmethod
    def from_xmed_chunk(view: BufferView, logger=None) -> 'RaysCastMemberTextRead':
        result = RaysCastMemberTextRead()
        if logger is not None:
            hex_dump = view.log_hex(view.size)
            logger.info("XMED all : %s", hex_dump.replace('\n', ' '))
        result.styles = XmedChunkParser.parse(view)
        result.text = ''.join(r.text for r in result.styles)
        return result
