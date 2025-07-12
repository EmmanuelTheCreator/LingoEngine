class RayTweenFlags:
    """Represents tweening flags for a score sprite or keyframe."""
    def __init__(self, tweening_enabled=False, path=False, size=False,
                 rotation=False, skew=False, blend=False,
                 fore_color=False, back_color=False):
        self.tweening_enabled = tweening_enabled
        self.path = path
        self.size = size
        self.rotation = rotation
        self.skew = skew
        self.blend = blend
        self.fore_color = fore_color
        self.back_color = back_color

    def to_byte(self):
        val = 0
        if self.tweening_enabled:
            val |= 0x01
        if self.path:
            val |= 0x02
        if self.size:
            val |= 0x04
        if self.rotation:
            val |= 0x08
        if self.skew:
            val |= 0x10
        if self.blend:
            val |= 0x20
        if self.fore_color:
            val |= 0x40
        if self.back_color:
            val |= 0x80
        return val

    def __str__(self):
        flags = []
        if self.path:
            flags.append('Path')
        if self.size:
            flags.append('Size')
        if self.rotation:
            flags.append('Rotation')
        if self.skew:
            flags.append('Skew')
        if self.blend:
            flags.append('Blend')
        if self.fore_color:
            flags.append('ForeColor')
        if self.back_color:
            flags.append('BackColor')
        return f"Tweening: {'On' if self.tweening_enabled else 'Off'} | " + ', '.join(flags)
