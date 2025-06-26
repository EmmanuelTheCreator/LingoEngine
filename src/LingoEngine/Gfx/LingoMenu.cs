namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper around a framework menu object.
    /// </summary>
    public class LingoMenu : LingoGfxNodeBase<ILingoFrameworkGfxMenu>
    {
        public string Name { get => _framework.Name; set => _framework.Name = value; }
        public void AddItem(LingoMenuItem item) => _framework.AddItem(item.Framework);
        public void ClearItems() => _framework.ClearItems();
    }
}
