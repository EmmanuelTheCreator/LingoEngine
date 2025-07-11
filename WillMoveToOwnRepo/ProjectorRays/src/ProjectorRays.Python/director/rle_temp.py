class RleTemp:
    @staticmethod
    def rle_decode(width: int, height: int, fullwidth: int, bpp: int, encoded: bytes) -> bytes:
        bytes_to_output = fullwidth * height
        res = bytearray(bytes_to_output)
        in_pos = 0
        out_pos = 0
        while in_pos < len(encoded) and out_pos < bytes_to_output:
            d = encoded[in_pos]
            in_pos += 1
            if d >= 128:
                run_length = 257 - d
                if in_pos >= len(encoded):
                    break
                v = encoded[in_pos]
                in_pos += 1
                for _ in range(run_length):
                    if out_pos >= bytes_to_output:
                        break
                    res[out_pos] = v
                    out_pos += 1
            else:
                lit_length = 1 + d
                res[out_pos:out_pos+lit_length] = encoded[in_pos:in_pos+lit_length]
                in_pos += lit_length
                out_pos += lit_length
        return bytes(res)
