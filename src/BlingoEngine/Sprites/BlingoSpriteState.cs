using AbstUI.Primitives;
using BlingoEngine.Members;
using BlingoEngine.Primitives;
using BlingoEngine.Sounds;

namespace BlingoEngine.Sprites;

public class BlingoSpriteState
{
    public string Name { get; set; } = string.Empty;
    //public int SpriteNum { get; set; }
}

public class BlingoSprite2DState : BlingoSpriteState
{
    public BlingoMember? Member { get; set; }
    public int DisplayMember { get; set; }
    public int SpritePropertiesOffset { get; set; }
    public int Ink { get; set; }
    public bool Hilite { get; set; }
    public float Blend { get; set; }
    public float LocH { get; set; }
    public float LocV { get; set; }
    public int LocZ { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Rotation { get; set; }
    public float Skew { get; set; }
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }
    public int Cursor { get; set; }
    public int Constraint { get; set; }
    public bool DirectToStage { get; set; }
    public APoint RegPoint { get; set; }
    public AColor ForeColor { get; set; }
    public AColor BackColor { get; set; }
    public bool Editable { get; set; }
    public bool IsDraggable { get; set; }
    public ARect? MemberSourceRect { get; set; }
}

public class BlingoSprite2DVirtualState : BlingoSpriteState
{
    public BlingoMember? Member { get; set; }
    public int DisplayMember { get; set; }
    public int Ink { get; set; }
    public bool Hilite { get; set; }
    public bool Linked { get; set; }
    public bool Loaded { get; set; }
    public float Blend { get; set; }
    public float LocH { get; set; }
    public float LocV { get; set; }
    public int LocZ { get; set; }
    public float Rotation { get; set; }
    public float Skew { get; set; }
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }
    public int Constraint { get; set; }
    public APoint RegPoint { get; set; }
    public AColor ForeColor { get; set; }
    public AColor BackColor { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}

public class BlingoSpriteSoundState : BlingoSpriteState
{
    public BlingoMemberSound? Sound { get; set; }
}

