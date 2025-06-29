using LingoEngine.Primitives;
using static System.Net.Mime.MediaTypeNames;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a numeric input field.
    /// </summary>
    public class LingoGfxInputNumber : LingoGfxInputBase<ILingoFrameworkGfxInputNumber>
    {
        public float Value { get => _framework.Value; set => _framework.Value = value; }
        public float Min { get => _framework.Min; set => _framework.Min = value; }
        public float Max { get => _framework.Max; set => _framework.Max = value; }

        public LingoNumberType NumberType { get => _framework.NumberType; set => _framework.NumberType = value; }
        public int FontSize { get => _framework.FontSize; set => _framework.FontSize = value; }
    }
}
