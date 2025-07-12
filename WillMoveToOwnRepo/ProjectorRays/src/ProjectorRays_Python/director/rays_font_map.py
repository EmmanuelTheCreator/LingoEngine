import os
from ..common.stream import BufferView

class RaysFontMap:
    @staticmethod
    def get_font_map(version: int) -> BufferView:
        file = {
            True: 'fontmap_D11_5.txt' if version >= 1150 else None
        }
        if version >= 1150:
            fname = 'fontmap_D11_5.txt'
        elif version >= 1100:
            fname = 'fontmap_D11.txt'
        elif version >= 1000:
            fname = 'fontmap_D10.txt'
        elif version >= 900:
            fname = 'fontmap_D9.txt'
        elif version >= 850:
            fname = 'fontmap_D8_5.txt'
        elif version >= 800:
            fname = 'fontmap_D8.txt'
        elif version >= 700:
            fname = 'fontmap_D7.txt'
        else:
            fname = 'fontmap_D6.txt'

        base_dir = os.path.dirname(__file__)
        path = os.path.join(base_dir, '..', 'fontmaps', fname)
        path = os.path.normpath(path)
        if not os.path.exists(path):
            return BufferView()
        with open(path, 'rb') as f:
            data = f.read()
        return BufferView(data, 0, len(data))
