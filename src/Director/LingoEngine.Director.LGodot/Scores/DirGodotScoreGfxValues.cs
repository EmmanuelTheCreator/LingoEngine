using Godot;

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
        public int TopStripHeight { get; set; } = 140;

        public Color ColLineLight = new Color("#f9f9f9");
        public Color ColLineDark = new Color("#d0d0d0");
        public DirGodotScoreGfxValues()
        {
            ChannelInfoWidth = ChannelLabelWidth + 16;
        }

    }
}
