namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a single line text input.
    /// </summary>
    public class LingoInputText : LingoInputBase<ILingoFrameworkInputText>
    {

        public string Text { get => _framework.Text; set => _framework.Text = value; }
        public int MaxLength { get => _framework.MaxLength; set => _framework.MaxLength = value; }
        public string? Font { get => _framework.Font; set => _framework.Font = value; }

    }
}
