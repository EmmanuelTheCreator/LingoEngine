namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a state (toggle) button.
    /// </summary>
    public class LingoGfxStateButton : LingoGfxInputBase<ILingoFrameworkGfxStateButton>
    {
        /// <summary>Displayed text on the button.</summary>
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        /// <summary>Icon texture displayed on the button.</summary>
        public Bitmaps.ILingoTexture2D? Texture { get => _framework.Texture; set => _framework.Texture = value; }
        /// <summary>Current toggle state.</summary>
        public bool IsOn { get => _framework.IsOn; set => _framework.IsOn = value; }
    }
}
