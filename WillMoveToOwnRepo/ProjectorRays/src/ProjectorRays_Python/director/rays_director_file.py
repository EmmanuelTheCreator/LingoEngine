from typing import List, Dict
import logging
import struct

from ..common.stream import BufferView, Endianness, ReadStream
from ..common.rays_json_writer import RaysJSONWriter
from .chunks.rays_chunk import RaysChunk, ChunkType
from .chunks.rays_cast_chunk import RaysCastChunk
from .chunks.rays_cast_member_chunk import RaysCastMemberChunk
from .chunks.rays_cast_list_chunk import RaysCastListChunk
from .chunks.rays_config_chunk import RaysConfigChunk
from .chunks.rays_initial_map_chunk import RaysInitialMapChunk
from .chunks.rays_key_table_chunk import RaysKeyTableChunk
from .chunks.rays_memory_map_chunk import RaysMemoryMapChunk
from .chunks.rays_script_chunk import RaysScriptChunk
from .chunks.rays_script_context_chunk import RaysScriptContextChunk
from .chunks.rays_script_names_chunk import RaysScriptNamesChunk
from .chunks.rays_xmed_chunk import RaysXmedChunk
from .rays_font_map import RaysFontMap
from .scores import RaysScoreChunk
from .rays_subchunk import KeyTableEntry
from .ray_guid import RayGuid, LingoGuidConstants

class ChunkInfo:
    def __init__(self):
        self.id = 0
        self.fourcc = 0
        self.length = 0
        self.uncompressed_len = 0
        self.offset = 0
        self.compression_id = RayGuid()

class RaysDirectorFile:
    def __init__(self, name: str = "", logger=None):
        self.name = name
        self._logger = logger or logging.getLogger(__name__)
        self.endianness = Endianness.BigEndian
        self.casts: List[RaysCastChunk] = []
        self.score: RaysScoreChunk | None = None
        self.chunk_info_map: Dict[int, ChunkInfo] = {}
        self.chunk_ids_by_fourcc: Dict[int, List[int]] = {}
        self.deserialized_chunks: Dict[int, RaysChunk] = {}
        self._cached_chunk_bufs: Dict[int, bytes] = {}
        self._cached_chunk_views: Dict[int, BufferView] = {}
        self.stream: ReadStream | None = None
        self.key_table: RaysKeyTableChunk | None = None
        self.config: RaysConfigChunk | None = None
        self.initial_map: RaysInitialMapChunk | None = None
        self.memory_map: RaysMemoryMapChunk | None = None
        self.version: int = 0
        self.codec: int = 0
        self.dot_syntax: bool = False
        self.afterburned: bool = False
        self._ils_body_offset: int = 0
        self._ils_buf: bytes = b""
        self.fver_version_string: str = ""

    @staticmethod
    def FOURCC(a: str, b: str, c: str, d: str) -> int:
        return (ord(a) << 24) | (ord(b) << 16) | (ord(c) << 8) | ord(d)

    def write_json(self, writer: RaysJSONWriter):
        writer.start_object()
        writer.write_key('name'); writer.write_val(self.name)
        writer.end_object()

    # ------------------------------------------------------------------
    # Basic helpers ported from the C# implementation
    # ------------------------------------------------------------------

    def chunk_exists(self, fourcc: int, chunk_id: int) -> bool:
        info = self.chunk_info_map.get(chunk_id)
        return info is not None and info.fourcc == fourcc

    def _add_chunk_info(self, info: ChunkInfo) -> None:
        self.chunk_info_map[info.id] = info
        self.chunk_ids_by_fourcc.setdefault(info.fourcc, []).append(info.id)

    def _get_first_chunk_info(self, fourcc: int) -> ChunkInfo | None:
        ids = self.chunk_ids_by_fourcc.get(fourcc)
        if ids:
            return self.chunk_info_map.get(ids[0])
        return None

    def get_chunk(self, fourcc: int, chunk_id: int) -> RaysChunk:
        if chunk_id in self.deserialized_chunks:
            return self.deserialized_chunks[chunk_id]
        view = self._get_chunk_data(fourcc, chunk_id)
        chunk = self._make_chunk(fourcc, view)
        self.deserialized_chunks[chunk_id] = chunk
        return chunk

    def _get_chunk_data(self, fourcc: int, chunk_id: int) -> BufferView:
        if self.stream is None:
            raise IOError("No stream loaded")
        info = self.chunk_info_map.get(chunk_id)
        if info is None:
            raise IOError(f"Could not find chunk {chunk_id}")
        if info.fourcc != fourcc:
            raise IOError(
                f"Expected chunk {chunk_id} to be {fourcc:x} but is {info.fourcc:x}")
        import zlib
        if self.afterburned:
            self.stream.seek(info.offset + self._ils_body_offset)
            if info.length == 0 and info.uncompressed_len == 0:
                return BufferView()
            if self._compression_implemented(info.compression_id):
                comp = self.stream.read_bytes(info.length)
                if info.compression_id == LingoGuidConstants.ZLIB_COMPRESSION_GUID:
                    data = zlib.decompress(comp)
                else:
                    data = comp
                return BufferView(data, 0, len(data))
            if info.compression_id == LingoGuidConstants.FONTMAP_COMPRESSION_GUID:
                return RaysFontMap.get_font_map(self.version)
            data = self.stream.read_bytes(info.length)
            return BufferView(data, 0, len(data))
        else:
            self.stream.seek(info.offset)
            data = self._read_chunk_data(fourcc, info.length)
            return data

    def _make_chunk(self, fourcc: int, view: BufferView) -> RaysChunk:
        mapping = {
            self.FOURCC('i', 'm', 'a', 'p'): RaysInitialMapChunk,
            self.FOURCC('m', 'm', 'a', 'p'): RaysMemoryMapChunk,
            self.FOURCC('C', 'A', 'S', '*'): RaysCastChunk,
            self.FOURCC('C', 'A', 'S', 't'): RaysCastMemberChunk,
            self.FOURCC('K', 'E', 'Y', '*'): RaysKeyTableChunk,
            self.FOURCC('L', 'c', 't', 'x'): RaysScriptContextChunk,
            self.FOURCC('L', 'c', 't', 'X'): RaysScriptContextChunk,
            self.FOURCC('L', 'n', 'a', 'm'): RaysScriptNamesChunk,
            self.FOURCC('L', 's', 'c', 'r'): RaysScriptChunk,
            self.FOURCC('V', 'W', 'C', 'F'): RaysConfigChunk,
            self.FOURCC('D', 'R', 'C', 'F'): RaysConfigChunk,
            self.FOURCC('M', 'C', 's', 'L'): RaysCastListChunk,
            self.FOURCC('V', 'W', 'S', 'C'): RaysScoreChunk,
            self.FOURCC('X', 'M', 'E', 'D'): RaysXmedChunk,
        }
        cls = mapping.get(fourcc)
        if cls is None:
            chunk = RaysChunk(self, ChunkType(fourcc))
        else:
            chunk = cls(self)
        rs = ReadStream(view, self.endianness)
        chunk.read(rs)
        return chunk

# ------------------------------------------------------------------
# Additional logic ported from the C# implementation
# ------------------------------------------------------------------

    def _read_chunk(self, fourcc: int, length: int | None = None) -> RaysChunk:
        view = self._read_chunk_data(fourcc, length)
        return self._make_chunk(fourcc, view)

    def _read_chunk_data(self, fourcc: int, length: int | None) -> BufferView:
        if self.stream is None:
            raise IOError("No stream loaded")
        offset = self.stream.pos
        valid_fourcc = self.stream.read_uint32()
        valid_len = self.stream.read_uint32()
        if length is None:
            length = valid_len
        if valid_fourcc != fourcc or valid_len != length:
            raise IOError(
                f"At offset {offset} expected {fourcc:x} len {length} "
                f"but got {valid_fourcc:x} len {valid_len}")
        data = self.stream.read_bytes(length)
        return BufferView(data, 0, length)

    def _read_memory_map(self) -> None:
        self.initial_map = self._read_chunk(self.FOURCC('i', 'm', 'a', 'p'))
        self.deserialized_chunks[1] = self.initial_map
        if self.stream is None:
            raise IOError("No stream loaded")
        self.stream.seek(self.initial_map.mmap_offset)
        self.memory_map = self._read_chunk(self.FOURCC('m', 'm', 'a', 'p'))
        self.deserialized_chunks[2] = self.memory_map
        memory_map_entries = []
        for i, entry in enumerate(self.memory_map.map_array):
            if entry.fourcc in (self.FOURCC('f', 'r', 'e', 'e'),
                                self.FOURCC('j', 'u', 'n', 'k')):
                continue
            tag = struct.pack(">I", entry.fourcc).decode('ascii', errors='replace')
            memory_map_entries.append({
                'fourcc': entry.fourcc,
                'tag': tag,
                'offset': entry.offset,
                'size': entry.length,
                'flags': entry.flags,
                'chunk_id': i,
            })
            info = ChunkInfo()
            info.id = i
            info.fourcc = entry.fourcc
            info.length = entry.length
            info.uncompressed_len = entry.length
            info.offset = entry.offset
            info.compression_id = LingoGuidConstants.NULL_COMPRESSION_GUID
            self._add_chunk_info(info)

        for entry in memory_map_entries:
            fourcc_int = entry['fourcc']
            tag = entry['tag']
            offset = entry['offset']
            size = entry['size']
            flags = entry['flags']
            chunk_id = entry['chunk_id']

            fourcc_hex = f"{fourcc_int:08X}"
            self._logger.info(
                "MemoryMap: FourCC=%s (\"%s\"), Offset=%d, Size=%d, Flags=%d",
                tag,
                fourcc_hex,
                offset,
                size,
                flags,
            )
            self._logger.info(
                "Registering chunk %s with ID %d", tag, chunk_id
            )

    def _read_afterburner_map(self) -> bool:
        import zlib
        s = self.stream
        if s is None:
            raise IOError("No stream loaded")
        if s.read_uint32() != self.FOURCC('F', 'v', 'e', 'r'):
            return False
        fver_len = s.read_var_int()
        start = s.pos
        fver_version = s.read_var_int()
        if fver_version >= 0x401:
            s.read_var_int(); s.read_var_int()
        if fver_version >= 0x501:
            length = s.read_uint8()
            self.fver_version_string = s.read_string(length)
        end = s.pos
        if end - start != fver_len:
            s.seek(start + fver_len)

        if s.read_uint32() != self.FOURCC('F', 'c', 'd', 'r'):
            return False
        fcdr_len = s.read_var_int()
        fcdr_buf = zlib.decompress(s.read_bytes(fcdr_len))
        fcdr = ReadStream(fcdr_buf, self.endianness)
        comp_count = fcdr.read_uint16()
        comp_ids = []
        for _ in range(comp_count):
            cid = RayGuid()
            cid.read(fcdr)
            comp_ids.append(cid)
        for _ in range(comp_count):
            fcdr.read_cstring()

        if s.read_uint32() != self.FOURCC('A', 'B', 'M', 'P'):
            return False
        abmp_len = s.read_var_int()
        abmp_end = s.pos + abmp_len
        abmp_comp_type = s.read_var_int()
        abmp_uncomp_len = s.read_var_int()
        abmp_buf = zlib.decompress(s.read_bytes(abmp_end - s.pos))
        abmp = ReadStream(abmp_buf, self.endianness)
        abmp.read_var_int()
        abmp.read_var_int()
        res_count = abmp.read_var_int()
        for _ in range(res_count):
            res_id = abmp.read_var_int()
            offset = abmp.read_var_int()
            comp_size = abmp.read_var_int()
            uncomp_size = abmp.read_var_int()
            comp_type = abmp.read_var_int()
            tag = abmp.read_uint32()

            info = ChunkInfo()
            info.id = res_id
            info.fourcc = tag
            info.length = comp_size
            info.uncompressed_len = uncomp_size
            info.offset = offset
            info.compression_id = comp_ids[comp_type]
            self._add_chunk_info(info)

        if 2 not in self.chunk_info_map:
            return False
        if s.read_uint32() != self.FOURCC('F', 'G', 'E', 'I'):
            return False
        ils_info = self.chunk_info_map[2]
        s.read_var_int()
        self._ils_body_offset = s.pos
        self._ils_buf = zlib.decompress(s.read_bytes(ils_info.length))
        ils_stream = ReadStream(self._ils_buf, self.endianness)
        while not ils_stream.eof():
            res_id = ils_stream.read_var_int()
            info = self.chunk_info_map[res_id]
            data = ils_stream.read_bytes(info.length)
            self._cached_chunk_views[res_id] = BufferView(data, 0, len(data))
        return True

    def _read_key_table(self) -> bool:
        info = self._get_first_chunk_info(self.FOURCC('K', 'E', 'Y', '*'))
        if info is None:
            return False
        self.key_table = self.get_chunk(info.fourcc, info.id)
        return True

    def _read_config(self) -> bool:
        info = (self._get_first_chunk_info(self.FOURCC('D', 'R', 'C', 'F')) or
                self._get_first_chunk_info(self.FOURCC('V', 'W', 'C', 'F')))
        if info is None:
            return False
        self.config = self.get_chunk(info.fourcc, info.id)
        from .rays_utilities import RaysUtilities
        self.version = RaysUtilities.human_version(self.config.director_version)
        self.dot_syntax = self.version >= 700
        return True

    def _read_casts(self) -> bool:
        internal_cast = True
        if self.version >= 500:
            info = self._get_first_chunk_info(self.FOURCC('M', 'C', 's', 'L'))
            if info is not None:
                cast_list = self.get_chunk(info.fourcc, info.id)
                for entry in cast_list.entries:
                    section_id = -1
                    for key_entry in self.key_table.entries:
                        if (key_entry.cast_id == entry.id and
                                key_entry.fourcc == self.FOURCC('C', 'A', 'S', '*')):
                            section_id = key_entry.section_id
                            break
                    if section_id > 0:
                        cast = self.get_chunk(self.FOURCC('C', 'A', 'S', '*'), section_id)
                        cast.populate(entry.name, entry.id, entry.min_member)
                        self.casts.append(cast)
                return True
            else:
                internal_cast = False
        info = self._get_first_chunk_info(self.FOURCC('C', 'A', 'S', '*'))
        if info is not None and info.fourcc == self.FOURCC('C', 'A', 'S', '*'):
            cast = self.get_chunk(self.FOURCC('C', 'A', 'S', '*'), info.id)
            cast.populate('Internal' if internal_cast else 'External', 1024,
                          self.config.min_member)
            self.casts.append(cast)
        return True

    def _read_score(self) -> None:
        info = self._get_first_chunk_info(self.FOURCC('V', 'W', 'S', 'C'))
        if info is not None:
            self.score = self.get_chunk(info.fourcc, info.id)

    def read(self, stream: ReadStream) -> bool:
        self.stream = stream
        magic = stream.read_uint32_raw()
        if magic == 0x52494658:  # 'RIFX'
            self.endianness = Endianness.BigEndian
        elif magic == 0x58464952:  # 'XFIR'
            self.endianness = Endianness.LittleEndian
        else:
            raise ValueError(f"Unknown file format magic: 0x{magic:08X}")
        stream.set_endianness(self.endianness)
        self._logger.info("File endianness: %s", self.endianness.name)
        stream.read_uint32()  # meta length
        self.codec = stream.read_uint32()
        if self.codec in (self.FOURCC('M', 'V', '9', '3'), self.FOURCC('M', 'C', '9', '5')):
            self._read_memory_map()
        elif self.codec in (self.FOURCC('F', 'G', 'D', 'M'), self.FOURCC('F', 'G', 'D', 'C')):
            self.afterburned = True
            if not self._read_afterburner_map():
                return False
        else:
            return False
        if not self._read_key_table():
            return False
        if not self._read_config():
            return False
        if not self._read_casts():
            return False
        self._read_score()
        return True

    def get_script(self, script_id: int) -> RaysScriptChunk | None:
        if not self.chunk_exists(self.FOURCC('L', 's', 'c', 'r'), script_id):
            return None
        return self.get_chunk(self.FOURCC('L', 's', 'c', 'r'), script_id)

    def get_script_names(self, names_id: int) -> RaysScriptNamesChunk | None:
        if not self.chunk_exists(self.FOURCC('L', 'n', 'a', 'm'), names_id):
            return None
        return self.get_chunk(self.FOURCC('L', 'n', 'a', 'm'), names_id)

    def parse_scripts(self) -> None:
        for cast in self.casts:
            ctx = getattr(cast, 'lctx', None)
            if ctx and hasattr(ctx, 'context'):
                parse = getattr(ctx.context, 'parse_scripts', None)
                if callable(parse):
                    parse()

    def restore_script_text(self) -> None:
        for cast in self.casts:
            ctx = getattr(cast, 'lctx', None)
            if ctx is None or not hasattr(ctx, 'context'):
                continue
            scripts = getattr(ctx.context, 'scripts', None)
            if not scripts:
                continue
            for sid, script in scripts.items():
                if not self.chunk_exists(self.FOURCC('L', 's', 'c', 'r'), sid):
                    continue
                script_chunk = self.get_chunk(self.FOURCC('L', 's', 'c', 'r'), sid)
                member = getattr(script_chunk, 'member', None)
                if member and hasattr(script, 'script_text'):
                    text = script.script_text('\n', self.dot_syntax)
                    set_text = getattr(member, 'set_script_text', None)
                    if callable(set_text):
                        set_text(text)

    @staticmethod
    def _compression_implemented(guid: RayGuid) -> bool:
        return (guid == LingoGuidConstants.ZLIB_COMPRESSION_GUID or
                guid == LingoGuidConstants.SND_COMPRESSION_GUID)

    @staticmethod
    def _is_always_big_endian(fourcc: int) -> bool:
        return fourcc in (
            RaysDirectorFile.FOURCC('C', 'A', 'S', '*'),
            RaysDirectorFile.FOURCC('C', 'A', 'S', 't'),
            RaysDirectorFile.FOURCC('M', 'C', 's', 'L'),
        )


def parse_memory_map_chunk(data: bytes, mmap_offset: int):
    """Lightweight helper used by early prototypes to inspect memory maps."""
    base = mmap_offset + 8  # Skip 4 byte chunk ID and size

    header_len = struct.unpack_from("<H", data, base)[0]
    entry_len = struct.unpack_from("<H", data, base + 2)[0]
    count_max = struct.unpack_from("<I", data, base + 4)[0]
    count_used = struct.unpack_from("<I", data, base + 8)[0]

    print(
        f"MemoryMapChunk: Entries={count_used}, "
        f"HeaderLen={header_len}, EntryLen={entry_len}, CountMax={count_max}"
    )

    entries = []
    entry_base = base + header_len
    for i in range(count_used):
        entry_data = data[entry_base + i * entry_len : entry_base + (i + 1) * entry_len]
        tag = entry_data[0:4].decode("ascii", errors="replace")
        offset = int.from_bytes(entry_data[4:8], "little")
        size = int.from_bytes(entry_data[8:12], "little")
        entries.append((tag, offset, size))
        print(f"Registering chunk {repr(tag)} with ID {i}")

    return entries


def parse_key_table_chunk(data: bytes, key_offset: int, little_endian: bool = True):
    """Helper used by prototypes to inspect KEY* chunks."""
    base = key_offset + 8  # Skip 4 byte chunk ID and size
    fmt_prefix = '<' if little_endian else '>'

    entry_size = struct.unpack_from(fmt_prefix + 'H', data, base)[0]
    # skip 2 bytes unused
    count_max = struct.unpack_from(fmt_prefix + 'I', data, base + 4)[0]
    count_used = struct.unpack_from(fmt_prefix + 'I', data, base + 8)[0]

    print(
        f"KeyTableChunk: EntrySize={entry_size}, Count={count_max}, "
        f"Used={count_used}, ParsedEntries={count_used}"
    )

    entries = []
    pos = base + 12
    for _ in range(count_used):
        fourcc = struct.unpack_from(fmt_prefix + 'I', data, pos)[0]
        member_id = struct.unpack_from(fmt_prefix + 'H', data, pos + 4)[0]
        member_type = struct.unpack_from(fmt_prefix + 'H', data, pos + 6)[0]
        cast_lib = struct.unpack_from(fmt_prefix + 'I', data, pos + 8)[0]
        entries.append((fourcc, member_id, member_type, cast_lib))
        pos += entry_size

    for i, (member_id, member_type, cast_lib) in enumerate(
        [(e[1], e[2], e[3]) for e in entries]
    ):
        print(f"  Entry {i}: ID={member_id}, Type={member_type}, CastLib={cast_lib}")

    return entries

