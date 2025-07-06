namespace ProjectorRays.director.Scores.Data;

internal record RayScoreHeader
{
    public int ActualSize {get; set; }
    public byte UnkA1 {get; set; }
    public byte UnkA2 {get; set; }
    public byte UnkA3 {get; set; }
    public byte UnkA4 {get; set; }
    public int HighestFrame {get; set; }
    public byte UnkB1 {get; set; }
    public byte UnkB2 {get; set; }
    public short SpriteSize {get; set; }
    public byte UnkC1 {get; set; }
    public byte UnkC2 {get; set; }
    public short ChannelCount {get; set; }
    //public short FirstBlockSize { get; set; }
    public int EntryCount { get; internal set; }
    public int EntrySizeSum { get; internal set; }
    public int NotationBase { get; internal set; }
    public int OffsetsOffset { get; internal set; }
    public int HeaderType { get; internal set; }
    public int TotalLength { get; internal set; }
}


