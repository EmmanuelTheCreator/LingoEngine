class RaysFileIO:
    """Simple helpers for reading and writing files."""

    import os

    @staticmethod
    def read_file(path: str, buf: bytearray) -> bool:
        try:
            with open(path, 'rb') as f:
                data = f.read()
            buf.clear()
            buf.extend(data)
            return True
        except OSError:
            return False

    @staticmethod
    def write_file(path: str, contents):
        mode = 'wb' if isinstance(contents, (bytes, bytearray)) else 'w'
        with open(path, mode) as f:
            f.write(contents)

    @staticmethod
    def clean_file_name(file_name: str) -> str:
        invalid = '<>:"/\\|?*'
        return ''.join('_' if ch in invalid else ch for ch in file_name)
