using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Styles
{
    public class DirectorColors
    {
        public static LingoColor BlueSelectColor = new LingoColor(0, 120, 215);
        public static LingoColor BlueLightSelectColor = new LingoColor(229,241,251);
        public static LingoColor BG_PropWindowBar = new LingoColor(178, 180, 191);    // Top bar of panels
        public static LingoColor BG_WhiteMenus = new LingoColor(240, 240, 240);       // Common window background

        // Windows
        public static LingoColor Window_Title_Line_Under = new LingoColor(178, 180, 191); // THe line just beneath the title of the window 
        public static LingoColor Window_Title_BG = LingoColor.FromHex("#d2e0ed"); // THe line just beneath the title of the window 


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
        public static LingoColor InputSelection = new LingoColor(0, 120, 215);        // Windows blue selection
        public static LingoColor InputSelectionText = new LingoColor(255, 255, 255);  // Text over selection
        public static LingoColor Input_Border = new LingoColor(50, 50, 50);           // Border for text inputs
        public static LingoColor Input_Bg = new LingoColor(255, 255, 255);            // Background for text inputs


        // Tabs
        public static LingoColor BG_Tabs = new LingoColor(157, 172, 191);             // Inactive tabs
        public static LingoColor BG_Tabs_Hover = new LingoColor(120, 133, 150);             // Inactive tabs
        public static LingoColor TabActiveBorder = new LingoColor(130, 130, 130);     // Top/side borders
        public static LingoColor Border_Tabs = new LingoColor(100, 100, 100);         // Tab outline
        public static LingoColor Tab_Selected_TextColor = new LingoColor(0, 0, 0);         // text of the selected tab
        public static LingoColor Tab_Deselected_TextColor = new LingoColor(255, 255, 255); // text of the deselected tab


        // Dividers and lines
        public static LingoColor DividerLines = new LingoColor(190, 190, 190);        // Light panel separators


        // Score grid
        public static LingoColor ScoreGridLineLight = LingoColor.FromHex("f9f9f9");
        public static LingoColor ScoreGridLineDark = LingoColor.FromHex("d0d0d0");


        // Buttons
        public static LingoColor Button_Bg_Normal = new LingoColor(240, 240, 240);      // Classic light gray
        public static LingoColor Button_Bg_Hover = BlueLightSelectColor;                // Hover highlight
        public static LingoColor Button_Bg_Pressed = new LingoColor(204, 204, 204);     // Pressed darker gray
        public static LingoColor Button_Bg_Disabled = new LingoColor(230, 230, 230);    // Disabled gray

        public static LingoColor Button_Border_Normal = new LingoColor(50, 50, 50);     // Standard border
        public static LingoColor Button_Border_Pressed = new LingoColor(100, 100, 100); // Slightly darker on press
        public static LingoColor Button_Border_Disabled = new LingoColor(190, 190, 190);// Faded border
        public static LingoColor Button_Border_Hover = BlueSelectColor;                 // Border hover

        public static LingoColor Button_Text_Normal = new LingoColor(0, 0, 0);
        public static LingoColor Button_Text_Disabled = new LingoColor(130, 130, 130);



        // Popup Window
        public static LingoColor PopupWindow_Background = new LingoColor(255, 255, 255);   // Main background
        public static LingoColor PopupWindow_Border = new LingoColor(160, 160, 160);       // Frame border

        public static LingoColor PopupWindow_Header_BG = new LingoColor(216, 216, 216);    // Header bar (light gray)
        public static LingoColor PopupWindow_Header_Text = new LingoColor(0, 0, 0);        // Title text

        public static LingoColor PopupWindow_CloseButton_BG = new LingoColor(221, 221, 221);
        public static LingoColor PopupWindow_CloseButton_Border = new LingoColor(130, 130, 130);
        public static LingoColor PopupWindow_CloseButton_Hover = new LingoColor(255, 0, 0);

    }
}
