namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific button control.
    /// </summary>
    public interface ILingoFrameworkGfxButton : ILingoFrameworkGfxLayoutNode
    {
        string Text { get; set; }
        bool Enabled { get; set; }
        event System.Action? Pressed;
    }
}
