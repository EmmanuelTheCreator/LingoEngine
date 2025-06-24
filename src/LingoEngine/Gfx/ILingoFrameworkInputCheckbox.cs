namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific checkbox input.
    /// </summary>
    public interface ILingoFrameworkInputCheckbox : ILingoFrameworkInput
    {
        bool Checked { get; set; }
    }
}
