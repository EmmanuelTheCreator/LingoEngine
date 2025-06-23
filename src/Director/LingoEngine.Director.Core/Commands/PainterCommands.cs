using LingoEngine.Commands;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Commands
{
    public enum PainterToolType
    {
        Pencil,
        PaintBrush,
        Eraser,
        Fill,
        Line,
        Rectangle,
        Circle,
        ColorPicker,
        SelectLasso,
        Hand,
        Zoom,
        Text,
    }

    /// <summary>
    /// Command to activate a specific painter tool by enum.
    /// </summary>
    public sealed record PainterToolSelectCommand(PainterToolType Tool) : ILingoCommand;
    /// <summary>
    /// Command to draw a single pixel at a given canvas coordinate with the currently selected color.
    /// </summary>
    public sealed record PainterDrawPixelCommand(int X, int Y) : ILingoCommand;
    /// <summary>
    /// Command to flood fill starting at the given canvas coordinate.
    /// </summary>
    public sealed record PainterFillCommand(int X, int Y) : ILingoCommand;
    public sealed record PainterChangeForegroundColorCommand(LingoColor color) : ILingoCommand;
    public sealed record PainterChangeBackgroundColorCommand(LingoColor color) : ILingoCommand;
}
