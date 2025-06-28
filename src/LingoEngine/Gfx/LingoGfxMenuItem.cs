namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a single menu item.
    /// </summary>
    public class LingoGfxMenuItem
    {
        private ILingoFrameworkGfxMenuItem _framework = null!;
        internal ILingoFrameworkGfxMenuItem Framework => _framework;

        public void Init(ILingoFrameworkGfxMenuItem framework) => _framework = framework;

        public string Name { get => _framework.Name; set => _framework.Name = value; }
        public bool Enabled { get => _framework.Enabled; set => _framework.Enabled = value; }
        public bool CheckMark { get => _framework.CheckMark; set => _framework.CheckMark = value; }
        public string? Shortcut { get => _framework.Shortcut; set => _framework.Shortcut = value; }
        public event System.Action? Activated
        {
            add { _framework.Activated += value; }
            remove { _framework.Activated -= value; }
        }
    }
}
