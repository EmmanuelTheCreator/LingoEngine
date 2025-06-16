using ProjectorRays.Common;

namespace ProjectorRays.Director;

public enum MemberType
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
    RTEMember = 12
}

public class CastMember
{
    public DirectorFile? Dir;
    public MemberType Type;

    public CastMember(DirectorFile? dir, MemberType type)
    {
        Dir = dir;
        Type = type;
    }

    public virtual void Read(ReadStream stream) { }

    public virtual void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.EndObject();
    }
}

public enum ScriptType
{
    ScoreScript = 1,
    MovieScript = 3,
    ParentScript = 7
}

public class ScriptMember : CastMember
{
    public ScriptType ScriptType;

    public ScriptMember(DirectorFile? dir) : base(dir, MemberType.ScriptMember) {}

    public override void Read(ReadStream stream)
    {
        ScriptType = (ScriptType)stream.ReadUint16();
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("scriptType", (int)ScriptType);
        json.EndObject();
    }
}

