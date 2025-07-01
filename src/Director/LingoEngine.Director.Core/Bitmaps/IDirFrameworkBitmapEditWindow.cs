using LingoEngine.Director.Core.Windowing;

namespace LingoEngine.Director.Core.Bitmaps
{
    public interface IDirFrameworkBitmapEditWindow : IDirFrameworkWindow 
    {
        bool SelectTheTool(PainterToolType tool);
        bool DrawThePixel(int x, int y);
    }
}
