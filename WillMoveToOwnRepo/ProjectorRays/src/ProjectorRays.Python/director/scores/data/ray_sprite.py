class UnknownTag:
    def __init__(self, tag, data):
        self.tag = tag
        self.data = data


class RaySprite:
    """Descriptor of a sprite on the score timeline."""
    def __init__(self):
        self.start_frame = 0
        self.end_frame = 0
        self.sprite_number = 0
        self.member_cast_lib = 0
        self.member_num = 0
        self.sprite_properties_offset = 0
        self.loc_h = 0
        self.loc_v = 0
        self.width = 0
        self.height = 0
        self.rotation = 0.0
        self.skew = 0.0
        self.ink = 0
        self.fore_color = 0
        self.back_color = 0
        self.score_color = 0
        self.blend = 0
        self.flip_h = False
        self.flip_v = False
        self.editable = False
        self.moveable = False
        self.trails = False
        self.is_locked = False
        self.ease_in = 0
        self.ease_out = 0
        self.curvature = 0
        self.tween_flags = None
        self.behaviors = []
        self.keyframes = []
        self.loc_z = 0
        self.extra_values = []
