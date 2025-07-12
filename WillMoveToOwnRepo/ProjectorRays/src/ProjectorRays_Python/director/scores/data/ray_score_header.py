class RayScoreHeader:
    """Holds header values for a score chunk."""
    def __init__(self):
        self.actual_size = 0
        self.unk_a1 = 0
        self.unk_a2 = 0
        self.unk_a3 = 0
        self.unk_a4 = 0
        self.highest_frame = 0
        self.unk_b1 = 0
        self.unk_b2 = 0
        self.sprite_size = 0
        self.unk_c1 = 0
        self.unk_c2 = 0
        self.channel_count = 0
        self.entry_count = 0
        self.entry_size_sum = 0
        self.notation_base = 0
        self.offsets_offset = 0
        self.header_type = 0
        self.total_length = 0
