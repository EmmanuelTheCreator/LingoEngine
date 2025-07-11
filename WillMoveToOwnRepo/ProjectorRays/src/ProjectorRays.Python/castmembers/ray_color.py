class RayColor:
    """Simple RGB color helper."""
    def __init__(self, r: int = 0, g: int = 0, b: int = 0):
        self.r = r
        self.g = g
        self.b = b

    @classmethod
    def from_rgb24(cls, rgb24: int) -> 'RayColor':
        return cls((rgb24 >> 16) & 0xFF, (rgb24 >> 8) & 0xFF, rgb24 & 0xFF)

    @classmethod
    def from_rgb555(cls, rgb555: int) -> 'RayColor':
        r = ((rgb555 >> 10) & 0x1F) * 255 // 31
        g = ((rgb555 >> 5) & 0x1F) * 255 // 31
        b = (rgb555 & 0x1F) * 255 // 31
        return cls(r, g, b)

    def to_rgb24(self) -> int:
        return (self.r << 16) | (self.g << 8) | self.b

    def to_rgb555(self) -> int:
        return ((self.r * 31 // 255) << 10) | ((self.g * 31 // 255) << 5) | (self.b * 31 // 255)

    def to_hex(self) -> str:
        return f"#{self.r:02X}{self.g:02X}{self.b:02X}"

    def __str__(self) -> str:
        return f"RGB({self.r}, {self.g}, {self.b})"
