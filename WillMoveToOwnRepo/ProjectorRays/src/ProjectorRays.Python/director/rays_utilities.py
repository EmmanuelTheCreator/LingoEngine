from ..common.util import RaysUtil

class RaysUtilities:
    human_version = staticmethod(RaysUtil.human_version)

    @staticmethod
    def dump_ascii(data: bytes, offset: int = 0, length: int = 12) -> str:
        end = min(len(data), offset + length)
        chars = [chr(b) if 32 <= b <= 126 else '.' for b in data[offset:end]]
        return ''.join(chars)

    @staticmethod
    def dump_hex_with_ascii(data: bytes, offset: int = 0, length: int = 12, paint_ascii: bool = True) -> str:
        end = min(len(data), offset + length)
        hex_part = ' '.join(f"{b:02X}" for b in data[offset:end])
        if paint_ascii:
            ascii_part = RaysUtilities.dump_ascii(data, offset, length)
            return f"{hex_part}|{ascii_part}|"
        return hex_part

    @staticmethod
    def version_number(ver: int, fver_version_string: str) -> str:
        major = ver // 100
        minor = (ver // 10) % 10
        patch = ver % 10
        if not fver_version_string:
            res = f"{major}.{minor}"
            if patch:
                res += f".{patch}"
            return res
        return fver_version_string

    @staticmethod
    def version_string(ver: int, fver_version_string: str) -> str:
        major = ver // 100
        version_num = RaysUtilities.version_number(ver, fver_version_string)
        if major >= 11:
            return f"Adobe Director {version_num}"
        if major == 10:
            return f"Macromedia Director MX 2004 ({version_num})"
        if major == 9:
            return f"Macromedia Director MX ({version_num})"
        return f"Macromedia Director {version_num}"
