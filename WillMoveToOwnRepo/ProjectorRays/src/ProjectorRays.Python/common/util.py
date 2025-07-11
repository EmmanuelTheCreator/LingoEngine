from enum import Enum

class RaysUtil:
    @staticmethod
    def fourcc_to_string(fourcc: int) -> str:
        chars = [chr((fourcc >> 24) & 0xFF),
                 chr((fourcc >> 16) & 0xFF),
                 chr((fourcc >> 8) & 0xFF),
                 chr(fourcc & 0xFF)]
        return ''.join(chars)

    @staticmethod
    def human_version(ver: int) -> int:
        if ver >= 1951: return 1200
        if ver >= 1922: return 1150
        if ver >= 1921: return 1100
        if ver >= 1851: return 1000
        if ver >= 1700: return 850
        if ver >= 1410: return 800
        if ver >= 1224: return 700
        if ver >= 1218: return 600
        if ver >= 1201: return 500
        if ver >= 1117: return 404
        if ver >= 1115: return 400
        if ver >= 1029: return 310
        if ver >= 1028: return 300
        return 200
