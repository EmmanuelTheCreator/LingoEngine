namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific checkbox input.
    /// </summary>
    public interface ILingoFrameworkInputCheckbox : ILingoFrameworkGfxNode
    {
        bool Checked { get; set; }
    }
}
