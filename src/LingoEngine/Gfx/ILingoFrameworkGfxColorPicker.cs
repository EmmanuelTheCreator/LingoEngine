namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific color picker input.
    /// </summary>
    public interface ILingoFrameworkGfxColorPicker : ILingoFrameworkGfxNodeInput
    {
        /// <summary>The currently selected color.</summary>
        Primitives.LingoColor Color { get; set; }
    }
}
