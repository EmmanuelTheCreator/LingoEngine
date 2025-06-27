using ProjectorRays.Common;

namespace ProjectorRays.Director;

public enum RaysMemberType
{
    NullMember = 0,
    BitmapMember = 1,
    FilmLoopMember = 2,
    TextMember = 3,
    PaletteMember = 4,
    PictureMember = 5,
    SoundMember = 6,
    ButtonMember = 7,
    ShapeMember = 8,
    MovieMember = 9,
    DigitalVideoMember = 10,
    ScriptMember = 11,
    RTEMember = 12,
    FontMember = 13,        // some references use 13
    XrayMember = 14,        // internal metadata
    FieldMember = 15,
}

public class RaysCastMember
{
    public RaysDirectorFile? Dir;
    public RaysMemberType Type;

    public RaysCastMember(RaysDirectorFile? dir, RaysMemberType type)
    {
        Dir = dir;
        Type = type;
    }

    public virtual void Read(ReadStream stream) { }

    public virtual void WriteJSON(RaysJSONWriter json)
    {
        json.StartObject();
        json.EndObject();
    }
}

public enum RaysScriptType
{
    ScoreScript = 1,
    MovieScript = 3,
    ParentScript = 7
}

public class RaysScriptMember : RaysCastMember
{
    public RaysScriptType ScriptType;

    public RaysScriptMember(RaysDirectorFile? dir) : base(dir, RaysMemberType.ScriptMember) {}

    public override void Read(ReadStream stream)
    {
        ScriptType = (RaysScriptType)stream.ReadUint16();
    }

    public override void WriteJSON(RaysJSONWriter json)
    {
        json.StartObject();
        json.WriteField("scriptType", (int)ScriptType);
        json.EndObject();
    }
}

