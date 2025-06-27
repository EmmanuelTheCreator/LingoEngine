namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific spinbox input.
    /// </summary>
    public interface ILingoFrameworkGfxSpinBox : ILingoFrameworkGfxNodeInput
    {
        float Value { get; set; }
        float Min { get; set; }
        float Max { get; set; }
    }
}
