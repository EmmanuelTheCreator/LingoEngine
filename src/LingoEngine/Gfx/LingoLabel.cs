namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper around a framework label.
    /// </summary>
    public class LingoLabel : LingoGfxNodeBase<ILingoFrameworkGfxLabel>
    {
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        public int FontSize { get => _framework.FontSize; set => _framework.FontSize = value; }
        public string? Font { get => _framework.Font; set => _framework.Font = value; }
        public LingoEngine.Primitives.LingoColor FontColor { get => _framework.FontColor; set => _framework.FontColor = value; }
    }
}
