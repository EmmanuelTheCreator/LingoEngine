namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a button control.
    /// </summary>
    public class LingoGfxButton : LingoGfxNodetBase<ILingoFrameworkGfxButton>
    {
        public string Text { get => _framework.Text; set => _framework.Text = value; }
        public bool Enabled { get => _framework.Enabled; set => _framework.Enabled = value; }
        public event Action? Pressed
        {
            add { _framework.Pressed += value; }
            remove { _framework.Pressed -= value; }
        }
    }
}
