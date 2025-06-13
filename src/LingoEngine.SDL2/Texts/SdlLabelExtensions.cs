using LingoEngine.FrameworkCommunication.Events;
using LingoEngine.Primitives;

namespace LingoEngineSDL2.Texts;

public static class SdlLabelExtensions
{
    public static object SetLingoFont(this object label, ILingoFontManager manager, string fontName) => label;
    public static object SetLingoFontSize(this object label, int size) => label;
    public static object SetLingoColor(this object label, LingoColor color) => label;
    public static LingoColor GetLingoColor(this object label) => LingoColor.FromRGB(0,0,0);
}
