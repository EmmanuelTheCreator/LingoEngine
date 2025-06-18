namespace LingoEngine.Director.LGodot.Scores
{
    public class DirGodotScoreGfxValues
    {
        public int ChannelHeight {get; set; }= 16;
        public int FrameWidth {get; set; }= 9;
        public int LeftMargin { get; set; } = 0;
        public int ChannelLabelWidth { get; set; } = 54;
        public int ChannelInfoWidth { get; set; } 
        public int ExtraMargin { get; set; }  = 20;

        public DirGodotScoreGfxValues()
        {
            ChannelInfoWidth = ChannelLabelWidth + 16;
        }

    }
}
