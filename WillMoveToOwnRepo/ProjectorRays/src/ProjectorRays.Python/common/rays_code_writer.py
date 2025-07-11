class RaysCodeWriter:
    """Very small helper for building source code strings."""
    def __init__(self, line_ending: str = "\n", indentation: str = "  "):
        self._parts = []
        self._line_ending = line_ending
        self._indentation = indentation
        self._indent = 0
        self._indent_written = False

    def _write_indent(self):
        if not self._indent_written:
            self._parts.append(self._indentation * self._indent)
            self._indent_written = True

    def write(self, text: str):
        if text:
            self._write_indent()
            self._parts.append(text)

    def write_line(self, text: str = ""):
        if text:
            self._write_indent()
            self._parts.append(text)
        self._parts.append(self._line_ending)
        self._indent_written = False

    def indent(self):
        self._indent += 1

    def unindent(self):
        if self._indent > 0:
            self._indent -= 1

    @property
    def size(self) -> int:
        return len(''.join(self._parts))

    def __str__(self) -> str:
        return ''.join(self._parts)
