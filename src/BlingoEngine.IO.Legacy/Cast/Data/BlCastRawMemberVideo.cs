namespace BlingoEngine.IO.Legacy.Cast.Data
{
    public class BlCastRawMemberVideo : BlCastRawMemberItem
    {
        public BlCastRawMemberVideo()
        {
            MemberType = BlCastRawMemberType.DigitalVideo;
        }

        public string LinkedFileName { get; set; } = "";
        public string LinkedFolder { get; set; } = "";
        public bool PlayVideo { get; set; }
        public float DurationSeconds { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public bool PlayAudio { get; set; }
        public bool StartPause { get; set; }
        public bool EnableLoop { get; set; }
        public int StartValueMs { get; set; }
        public int VideoFps { get; internal set; }
        public BlRawVideoFraming Framing { get; internal set; }

        public enum BlRawVideoPlaybackRate { Sync, EveryFrame, Maximum, Fixed }
        public enum BlRawVideoFraming { Scale, Crop, CropCenter }
    }
}
