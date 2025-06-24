

namespace LingoEngine.IO.Data.DTO;

public class LingoSpriteDTO
{
    public string Name { get; set; } = string.Empty;
    public int SpriteNum { get; set; }
    public int MemberNum { get; set; }
    public int DisplayMember { get; set; }
    public int SpritePropertiesOffset { get; set; }
    public bool Puppet { get; set; }
    public bool Lock { get; set; }
    public bool Visibility { get; set; }
    public float LocH { get; set; }
    public float LocV { get; set; }
    public int LocZ { get; set; }
    public float Rotation { get; set; }
    public float Skew { get; set; }
    public LingoPointDTO RegPoint { get; set; }
    public int Ink { get; set; }
    public LingoColorDTO ForeColor { get; set; }
    public LingoColorDTO BackColor { get; set; }
    public float Blend { get; set; }
    public bool Editable { get; set; }
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }
    public int ScoreColor { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public int BeginFrame { get; set; }
    public int EndFrame { get; set; }

    public LingoSpriteAnimatorDTO? Animator { get; set; }
}
