namespace BlingoEngine.IO.Legacy.Cast.Data
{
    public class BlCastMemberVideo : BlCastMemberItem
    {
        public BlCastMemberVideo()
        {
            MemberType = BlLegacyCastMemberType.DigitalVideo;
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
    }
}
