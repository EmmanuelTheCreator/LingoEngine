namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific checkbox input.
    /// </summary>
    public interface ILingoFrameworkGfxInputCheckbox : ILingoFrameworkGfxNodeInput
    {
        bool Checked { get; set; }
    }
}
