namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific numeric input field.
    /// </summary>
    public interface ILingoFrameworkInputNumber : ILingoFrameworkGfxNode
    {
        float Value { get; set; }
        float Min { get; set; }
        float Max { get; set; }
    }
}
