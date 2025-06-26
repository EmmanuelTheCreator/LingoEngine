namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper around a framework tab container.
    /// </summary>
    public class LingoGfxTabContainer : LingoGfxNodeBase<ILingoFrameworkGfxTabContainer>
    {
        public void AddTab(LingoGfxTabItem item)
            => _framework.AddTab(item.Title, item.Content.Framework<ILingoFrameworkGfxNode>());

        public void ClearTabs() => _framework.ClearTabs();
    }
}
