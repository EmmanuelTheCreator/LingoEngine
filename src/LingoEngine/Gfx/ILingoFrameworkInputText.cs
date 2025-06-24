namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific single line text input.
    /// </summary>
    public interface ILingoFrameworkInputText : ILingoFrameworkGfxNode
    {
        string Text { get; set; }
        int MaxLength { get; set; }
        string? Font { get; set; }
    }
}
