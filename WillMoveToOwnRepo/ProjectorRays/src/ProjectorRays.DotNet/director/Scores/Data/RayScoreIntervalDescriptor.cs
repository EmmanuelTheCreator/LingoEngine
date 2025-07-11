using static ProjectorRays.director.Scores.RaysScoreChunk;

namespace ProjectorRays.director.Scores.Data;

internal class RayScoreIntervalDescriptor
{
    public int StartFrame { get; internal set; }
    public int EndFrame { get; internal set; }
    public int Unknown1 { get; internal set; }
    public int Unknown2 { get; internal set; }
    public int SpriteNumber { get; internal set; }
    public int UnknownAlwaysOne { get; internal set; }
    public int UnkownA { get; internal set; }
    public int UnkownB { get; internal set; }
    public int UnknownE1 { get; internal set; }
    public int UnknownFD { get; internal set; }
    public int Unknown7 { get; internal set; }
    public int Unknown8 { get; internal set; }
    public List<int> ExtraValues { get; } = new();
    public int Channel { get; internal set; }
    public List<RaysBehaviourRef> Behaviors { get; internal set; } = new List<RaysBehaviourRef>();
    public bool FlipH { get; internal set; }
    public bool FlipV { get; internal set; }
    public bool Editable { get; internal set; }
    public bool Moveable { get; internal set; }
    public bool Trails { get; internal set; }
    public bool IsLocked { get; internal set; }
}

