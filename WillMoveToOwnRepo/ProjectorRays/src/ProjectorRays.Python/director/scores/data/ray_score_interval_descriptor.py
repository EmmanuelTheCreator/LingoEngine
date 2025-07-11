class RayScoreIntervalDescriptor:
    """Frame descriptor for sprite timeline intervals."""
    def __init__(self):
        self.start_frame = 0
        self.end_frame = 0
        self.unknown1 = 0
        self.unknown2 = 0
        self.sprite_number = 0
        self.unknown_always_one = 0
        self.unkown_a = 0
        self.unkown_b = 0
        self.unknown_e1 = 0
        self.unknown_fd = 0
        self.unknown7 = 0
        self.unknown8 = 0
        self.extra_values = []
        self.channel = 0
        self.behaviors = []
        self.flip_h = False
        self.flip_v = False
        self.editable = False
        self.moveable = False
        self.trails = False
        self.is_locked = False
