namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper around a framework menu object.
    /// </summary>
    public class LingoGfxMenu : LingoGfxNodeBase<ILingoFrameworkGfxMenu>
    {
        public void AddItem(LingoGfxMenuItem item) => _framework.AddItem(item.Framework);
        public void ClearItems() => _framework.ClearItems();
        public void Popup() => _framework.Popup();
        public void PositionPopup(LingoGfxButton button)
            => _framework.PositionPopup(button.Framework<ILingoFrameworkGfxButton>());
    }
}
