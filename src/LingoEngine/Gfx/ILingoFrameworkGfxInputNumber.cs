using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific numeric input field.
    /// </summary>
    public interface ILingoFrameworkGfxInputNumber : ILingoFrameworkGfxNodeInput
    {
        float Value { get; set; }
        float Min { get; set; }
        float Max { get; set; }
        LingoNumberType NumberType { get; set; }
        int FontSize { get; set; }
    }
}
