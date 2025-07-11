from .data import RaySprite, RayScoreIntervalDescriptor, RayScoreKeyFrame

class RayScoreParseContext:
    """Maintains state while parsing a score chunk."""
    def __init__(self, logger=None):
        self.logger = logger
        self.sprites = []
        self.frame_data_buffer_view = None
        self.frame_interval_descriptor_buffers = []
        self.behavior_script_buffers = []
        self.descriptors = []
        self.channel_to_descriptor = {}
        self.frame_scripts = []
        self.keyframes = []
        self.current_frame = 0
        self.current_sprite = None
        self.current_sprite_num = 0
        self.block_depth = 0
        self.upcoming_block_size = 0
        self.is_in_advance_frame_mode = False
        self.current_keyframe = None

    def set_current_frame(self, frame: int):
        self.current_frame = frame

    def set_current_sprite(self, sprite: RaySprite):
        self.current_sprite = sprite
        self.current_sprite_num = sprite.sprite_number - 6
        self.current_frame = sprite.start_frame

    def add_keyframe(self, kf: RayScoreKeyFrame):
        self.keyframes.append(kf)
        self.current_keyframe = kf

    def add_sprite(self, sprite: RaySprite):
        self.sprites.append(sprite)

    def get_sprite(self, channel: int, frame: int):
        for sp in self.sprites:
            if sp.sprite_number == channel and sp.start_frame <= frame <= sp.end_frame:
                return sp
        return None

    def get_or_create_sprite(self, channel: int):
        sp = self.get_sprite(channel, self.current_frame)
        if sp:
            self.set_current_sprite(sp)
            return sp
        sp = RaySprite()
        sp.sprite_number = channel
        sp.start_frame = self.current_frame
        sp.end_frame = self.current_frame
        self.sprites.append(sp)
        self.set_current_sprite(sp)
        return sp
