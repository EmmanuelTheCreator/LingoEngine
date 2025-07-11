namespace LingoEngine.Gfx
{
    /// <summary>
    /// Common interface for all framework input controls.
    /// </summary>
    public interface ILingoFrameworkGfxNodeInput : ILingoFrameworkGfxLayoutNode
    {
        /// <summary>Whether the control is enabled.</summary>
        bool Enabled { get; set; }
        /// <summary>Raised when the value of the input changes.</summary>
        event Action? ValueChanged;
    }
}
