namespace BlingoEngine.IO.Data.DTO.Members;

public class BlingoMemberVideoDTO : BlingoMemberDTO
{
    [Flags]
    public enum VideoPlaybackMode : byte
    {
        SyncToSound = 0x00,
        PausedOn = 0x01,
        VideoOff = 0x02,
        PreloadOn = 0x04,
        EveryFrame = 0x08,
        FixedFps = 0x28,
        MaximumSpeed = 0x18
    }

    [Flags]
    public enum VideoPlaybackFlags : byte
    {
        Default = 0x2A,
        DtsOff = 0x0A,
        AudioOff = 0x22,
        FramingCrop = 0x28,
        FramingCropCenter = 0x29,
        LoopOn = 0x3A
    }
    public string LinkedFileName { get; set; } = "";
    public string LinkedFolder { get; set; } = "";
    public bool PlayVideo { get; set; }
    public float DurationSeconds { get; set; }
    public bool PlayAudio { get; set; }
    public bool StartPause { get; set; }
    public bool EnableLoop { get; set; }
    public int StartValueMs { get; set; }
    public int VideoFps { get; set; }
}

