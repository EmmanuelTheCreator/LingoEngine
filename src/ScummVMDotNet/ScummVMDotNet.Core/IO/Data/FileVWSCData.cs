namespace Director.IO.Data
{
    /// <summary>
    /// The Score consists of two main parts: notation and sprite properties. The notation has instructions on where to put certain buffers in the memory 
    /// for them to be interpreted properly by Director.
    /// 
    /// Basically, there were so many null bytes between actual useful data in these buffers that the engineers at Macromedia thought it’d be a good idea 
    /// to describe the offset and size of everything that isn’t a null byte instead of saving the whole buffer to a file.
    /// </summary>
    public class FileVWSCData
    {
        public int FramesEndOffset { get; internal set; }
        public int FramesSize { get; internal set; }
        public short FramesType { get; internal set; }
        public short ChannelSize { get; internal set; }
        public short LastChannelMax { get; internal set; }
        public short LastChannel { get; internal set; }
        public List<FrameData> Frames { get; internal set; } = new();
        public class SpriteData
        {
            public int Ink { get; internal set; }
            public byte ForeColor { get; internal set; }
            public byte BackColor { get; internal set; }
            public uint DisplayMember { get; internal set; }
            public int LocV { get; internal set; }
            public int LocH { get; internal set; }
            public int Height { get; internal set; }
            public int Width { get; internal set; }
            public int Editable { get; internal set; }
            public byte Blend { get; internal set; }
            public int FlipV { get; internal set; }
            public int FlipH { get; internal set; }
            public float Rotation { get; internal set; }
            public float Skew { get; internal set; }
            public int ScoreColor { get; internal set; }
        }
        public class FrameData
        {
            public List<ChannelData> Channels { get; internal set; } = new();
            public short FrameEnd { get; internal set; }
            public List<int> PropertiesOffsets { get; internal set; } = new();
            public int StartFrame { get; internal set; }
            public int EndFrame { get; internal set; }
            public int ChannelNum { get; internal set; }
            public int Number { get; internal set; }
        }
        public class ChannelData
        {
            public short Size { get; internal set; }
            public short Offset { get; internal set; }
            public short Buffer { get; internal set; }


            public int InkFlag { get; internal set; }
            public bool MultipleMembers { get; internal set; }
        }

    }
}
