using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a color picker input.
    /// </summary>
    public class LingoGfxColorPicker : LingoGfxInputBase<ILingoFrameworkGfxColorPicker>
    {
        public LingoColor Color { get => _framework.Color; set => _framework.Color = value; }
    }
}
