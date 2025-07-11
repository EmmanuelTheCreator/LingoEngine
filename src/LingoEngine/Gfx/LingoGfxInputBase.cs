namespace LingoEngine.Gfx
{
    /// <summary>
    /// Base class for all engine level input controls.
    /// </summary>
    public abstract class LingoGfxInputBase<TFramework> : LingoGfxNodeLayoutBase<TFramework>
        where TFramework : ILingoFrameworkGfxNodeInput
    {
        /// <summary>Whether the control is enabled.</summary>
        public bool Enabled { get => _framework.Enabled; set => _framework.Enabled = value; }

        /// <summary>Event raised when the input value changes.</summary>
        public event System.Action? ValueChanged
        {
            add { _framework.ValueChanged += value; }
            remove { _framework.ValueChanged -= value; }
        }
    }
}
