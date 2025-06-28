namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific button control.
    /// </summary>
    public interface ILingoFrameworkGfxButton : ILingoFrameworkGfxNode
    {
        string Text { get; set; }
        bool Enabled { get; set; }
        event Action? Pressed;
    }
}
