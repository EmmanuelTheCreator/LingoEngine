using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Styles
{
    public class DirectorColors
    {
        public static LingoColor BG_PropWindowBar = new LingoColor(178, 180, 191);    // Top bar of panels
        public static LingoColor BG_WhiteMenus = new LingoColor(240, 240, 240);       // Common window background
        public static LingoColor BG_Tabs = new LingoColor(157, 172, 191);             // Inactive tabs

        // Thumbnail of a member
        public static LingoColor Bg_Thumb = new LingoColor(255, 255, 255);
        public static LingoColor Border_Thumb = new LingoColor(64, 64, 64);

        // Text
        public static LingoColor TextColorLabels = new LingoColor(30, 30, 30);        // Property labels
        public static LingoColor TextColorDisabled = new LingoColor(130, 130, 130);   // Grayed-out text
        public static LingoColor TextColorFocused = new LingoColor(20, 20, 20);       // Active focus color

        // Inputs
        public static LingoColor InputText = new LingoColor(30, 30, 30);
        public static LingoColor InputBorder = new LingoColor(30, 30, 30);
        public static LingoColor InputBg = new LingoColor(255, 255, 255);
        public static LingoColor InputSelection = new LingoColor(0, 120, 215);        // Windows blue selection
        public static LingoColor InputSelectionText = new LingoColor(255, 255, 255);  // Text over selection

        // Tabs
        public static LingoColor TabActiveBorder = new LingoColor(130, 130, 130);     // Top/side borders
        public static LingoColor Border_Tabs = new LingoColor(100, 100, 100);         // Tab outline

        // Dividers and lines
        public static LingoColor DividerLines = new LingoColor(190, 190, 190);        // Light panel separators
        // Score grid
        public static LingoColor ScoreGridLineLight = LingoColor.FromHex("f9f9f9");
        public static LingoColor ScoreGridLineDark = LingoColor.FromHex("d0d0d0");
    }
}
