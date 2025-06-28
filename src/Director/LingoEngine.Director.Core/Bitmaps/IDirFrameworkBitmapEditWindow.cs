using LingoEngine.Director.Core.Pictures;
using LingoEngine.Director.Core.Windows;

namespace LingoEngine.Director.Core.Bitmaps
{
    public interface IDirFrameworkBitmapEditWindow : IDirFrameworkWindow 
    {
        bool SelectTheTool(PainterToolType tool);
        bool DrawThePixel(int x, int y);
    }
}
