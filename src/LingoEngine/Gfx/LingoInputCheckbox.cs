namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a checkbox input.
    /// </summary>
    public class LingoInputCheckbox : LingoInputBase<ILingoFrameworkInputCheckbox>
    {

        public bool Checked { get => _framework.Checked; set => _framework.Checked = value; }
    }
}
