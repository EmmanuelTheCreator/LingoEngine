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
        /// <summary>Size of the memory handle used by Director.</summary>
        public int MemHandleSize { get; internal set; }
        /// <summary>Header type value, typically -3.</summary>
        public int HeaderType { get; internal set; }
        /// <summary>Offset to the sprite properties offset count from the beginning of the chunk.</summary>
        public int SpritePropertiesOffsetsCountOffset { get; internal set; }
        /// <summary>Number of entries in the sprite properties offset table.</summary>
        public int SpritePropertiesOffsetsCount { get; internal set; }
        /// <summary>Absolute offset to the start of the notation section.</summary>
        public int NotationOffset { get; internal set; }
        /// <summary>Total size of the notation and sprite properties data.</summary>
        public int NotationAndSpritePropertiesSize { get; internal set; }
        /// <summary>Offsets to individual sprite property blocks.</summary>
        public List<short> SpritePropertiesOffsets { get; internal set; } = new();
        /// <summary>Default channel information containing sprite data.</summary>
        public List<ChannelData> ChannelInfo { get; internal set; } = new();
        /// <summary>Offset marking the end of the frames buffer.</summary>
        public int FramesEndOffset { get; internal set; }
        /// <summary>Number of frame definitions in the score.</summary>
        public int FramesSize { get; internal set; }
        /// <summary>Version specific frame type value.</summary>
        public short FramesType { get; internal set; }
        /// <summary>Size in bytes of a channel entry in the frame table.</summary>
        public short ChannelSize { get; internal set; }
        /// <summary>Maximum channel index used by the score.</summary>
        public short LastChannelMax { get; internal set; }
        /// <summary>Index of the last active channel.</summary>
        public short LastChannel { get; internal set; }
        /// <summary>Individual frame descriptors for the score.</summary>
        public List<FrameData> Frames { get; internal set; } = new();
        public class SpriteData
        {
            /// <summary>Ink mode used when drawing the sprite.</summary>
            public int Ink { get; internal set; }
            /// <summary>Foreground color index.</summary>
            public byte ForeColor { get; internal set; }
            /// <summary>Background color index.</summary>
            public byte BackColor { get; internal set; }
            /// <summary>Resource ID of the cast member displayed.</summary>
            public uint DisplayMember { get; internal set; }
            /// <summary>Vertical position on the stage.</summary>
            public int LocV { get; internal set; }
            /// <summary>Horizontal position on the stage.</summary>
            public int LocH { get; internal set; }
            /// <summary>Height of the sprite.</summary>
            public int Height { get; internal set; }
            /// <summary>Width of the sprite.</summary>
            public int Width { get; internal set; }
            /// <summary>Flag indicating if the sprite can be edited.</summary>
            public int Editable { get; internal set; }
            /// <summary>Blend value used when rendering.</summary>
            public byte Blend { get; internal set; }
            /// <summary>Vertical flip flag.</summary>
            public int FlipV { get; internal set; }
            /// <summary>Horizontal flip flag.</summary>
            public int FlipH { get; internal set; }
            /// <summary>Rotation applied to the sprite.</summary>
            public float Rotation { get; internal set; }
            /// <summary>Skew value applied to the sprite.</summary>
            public float Skew { get; internal set; }
            /// <summary>Color used in the score window.</summary>
            public int ScoreColor { get; internal set; }
        }
        public class FrameData
        {
            /// <summary>Channel buffers belonging to this frame.</summary>
            public List<ChannelData> Channels { get; internal set; } = new();
            /// <summary>Number of channels defined for this frame.</summary>
            public short FrameEnd { get; internal set; }
            /// <summary>Offsets to sprite properties used in this frame.</summary>
            public List<int> PropertiesOffsets { get; internal set; } = new();
            /// <summary>Start frame index of the frame interval.</summary>
            public int StartFrame { get; internal set; }
            /// <summary>End frame index of the frame interval.</summary>
            public int EndFrame { get; internal set; }
            /// <summary>Channel index associated with the interval.</summary>
            public int ChannelNum { get; internal set; }
            /// <summary>Sprite number within the score.</summary>
            public int Number { get; internal set; }
        }
        public class ChannelData
        {
            /// <summary>Length of the channel's data buffer.</summary>
            public short Size { get; internal set; }
            /// <summary>Offset from the start of the frame buffer.</summary>
            public short Offset { get; internal set; }
            /// <summary>Index of the buffer holding the channel data.</summary>
            public short Buffer { get; internal set; }
            /// <summary>
            /// Offset into the sprite properties table for this channel.
            /// </summary>
            public ushort SpritePropertiesOffset { get; internal set; }
            /// <summary>Ink flag extracted from the default sprite.</summary>
            public int InkFlag { get; internal set; }
            /// <summary>Indicates that the channel can contain multiple members.</summary>
            public bool MultipleMembers { get; internal set; }
            /// <summary>Default sprite data used by this channel.</summary>
            public SpriteData? Sprite { get; internal set; }
        }

    }
}
