from ..common.stream import ReadStream, Endianness
from .ray_score_parse_context import RayScoreParseContext
from .rays_score_reader import RaysScoreReader
from .ray_sprite_factory import RaySpriteFactory

class RaysScoreFrameParserV2:
    def __init__(self, logger=None):
        self.logger = logger
        self.reader = RaysScoreReader(logger)

    def parse_score(self, stream: ReadStream):
        stream.endianness = Endianness.BIG
        ctx = RayScoreParseContext(self.logger)
        header = self.reader.read_main_header(stream)
        # This simplified parser only reads a single sprite block
        sprite = self.reader.create_channel_sprite(stream)
        sprite.sprite_number = 6
        ctx.add_sprite(sprite)
        return ctx.sprites
