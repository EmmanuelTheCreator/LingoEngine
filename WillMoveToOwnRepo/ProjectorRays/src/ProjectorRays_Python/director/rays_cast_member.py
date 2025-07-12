from enum import Enum
from ..common.rays_json_writer import RaysJSONWriter
from ..common.stream import ReadStream

class RaysMemberType(Enum):
    NullMember = 0
    BitmapMember = 1
    FilmLoopMember = 2
    TextMember = 3
    PaletteMember = 4
    PictureMember = 5
    SoundMember = 6
    ButtonMember = 7
    ShapeMember = 8
    MovieMember = 9
    DigitalVideoMember = 10
    ScriptMember = 11
    RTEMember = 12
    FontMember = 13
    XrayMember = 14
    FieldMember = 15

class RaysCastMember:
    def __init__(self, dir=None, member_type=RaysMemberType.NullMember):
        self.dir = dir
        self.type = member_type

    def read(self, stream: ReadStream):
        """Consume any remaining data for this member."""
        stream.skip(len(stream.data) - stream.pos)

    def write_json(self, writer: RaysJSONWriter):
        writer.start_object()
        writer.write_key('type'); writer.write_val(self.type.name)
        writer.end_object()

class RaysScriptType(Enum):
    ScoreScript = 1
    MovieScript = 3
    ParentScript = 7

class RaysScriptMember(RaysCastMember):
    def __init__(self, dir=None):
        super().__init__(dir, RaysMemberType.ScriptMember)
        self.script_type = RaysScriptType.ScoreScript

    def read(self, stream: ReadStream):
        self.script_type = RaysScriptType(stream.read_uint16())

    def write_json(self, writer: RaysJSONWriter):
        writer.start_object()
        writer.write_key('scriptType'); writer.write_val(self.script_type.value)
        writer.end_object()
