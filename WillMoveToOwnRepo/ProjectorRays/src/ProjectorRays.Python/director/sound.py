from ..common.stream import ReadStream

class Sound:
    @staticmethod
    def decompress_snd(input_stream: ReadStream) -> bytes:
        if input_stream is None or input_stream.eof():
            return b""
        remaining = len(input_stream.data) - input_stream.pos
        return input_stream.read_bytes(remaining)
