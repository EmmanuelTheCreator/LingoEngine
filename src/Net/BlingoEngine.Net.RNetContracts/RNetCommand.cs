namespace BlingoEngine.Net.RNetContracts;

/// <summary>
/// Base type for commands sent over the network.
/// </summary>
public abstract record RNetCommand : IRNetCommand;

/// <summary>
/// Identifies the kind of cast member targeted by a command.
/// </summary>
public enum RNetMemberTypeDto
{
    Unknown,
    Animgif,
    Ole,
    Bitmap,
    Palette,
    Button,
    Picture,
    Cursor,
    QuickTimeMedia,
    DigitalVideo,
    RealMedia,
    DVD,
    Script,
    Empty,
    Shape,
    Field,
    Shockwave3D,
    FilmLoop,
    Sound,
    Flash,
    Swa,
    Flashcomponent,
    Text,
    Font,
    Transition,
    Havok,
    VectorShape,
    Movie,
    WindowsMedia
}

/// <summary>
/// Identifies the kind of sprite targeted by a command.
/// </summary>
public enum RNetSpriteTypeDto
{
    Unknown,
    Sprite2D,
    Tempo,
    ColorPalette,
    FrameScript,
    Transition,
    Sound,
}

/// <summary>Sets a sprite property.</summary>
/// <param name="SpriteNum">Sprite channel number.</param>
/// <param name="BeginFrame">Sprite begin frame.</param>
/// <param name="Prop">Property name.</param>
/// <param name="Value">New value.</param>
/// <param name="SpriteType">The type of sprite being updated.</param>
public sealed record SetSpritePropCmd(int SpriteNum, int BeginFrame, RNetSpriteTypeDto SpriteType, string Prop, string Value) : RNetCommand;

/// <summary>Sets a cast member property.</summary>
/// <param name="CastLibNum">Cast library number.</param>
/// <param name="MemberNum">Member number within the cast library.</param>
/// <param name="MemberType">The type of the cast member.</param>
/// <param name="Prop">Property name.</param>
/// <param name="Value">New value.</param>
public sealed record SetMemberPropCmd(int CastLibNum, int MemberNum, RNetMemberTypeDto MemberType, string Prop, string Value) : RNetCommand;

/// <summary>Sets a cast property.</summary>
/// <param name="CastLibNum">Cast library number.</param>
/// <param name="Prop">Property name.</param>
/// <param name="Value">New value.</param>
public sealed record SetCastPropCmd(int CastLibNum, string Prop, string Value) : RNetCommand;

/// <summary>Changes playback to a specific frame.</summary>
/// <param name="Frame">Target frame number.</param>
public sealed record GoToFrameCmd(int Frame) : RNetCommand;

/// <summary>Rewinds playback to the first frame.</summary>
public sealed record RewindCmd() : RNetCommand;

/// <summary>Pauses playback.</summary>
public sealed record PauseCmd() : RNetCommand;

/// <summary>Resumes playback.</summary>
public sealed record ResumeCmd() : RNetCommand;

