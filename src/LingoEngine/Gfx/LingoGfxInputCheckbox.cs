namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a checkbox input.
    /// </summary>
    public class LingoGfxInputCheckbox : LingoGfxInputBase<ILingoFrameworkGfxInputCheckbox>
    {

        public bool Checked { get => _framework.Checked; set => _framework.Checked = value; }
    }
}
