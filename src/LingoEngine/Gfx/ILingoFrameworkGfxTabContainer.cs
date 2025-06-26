using System.Collections.Generic;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific container organizing children into tabs.
    /// </summary>
    public interface ILingoFrameworkGfxTabContainer : ILingoFrameworkGfxNode
    {
        /// <summary>Adds a new tab containing the specified node.</summary>
        void AddTab(string title, ILingoFrameworkGfxNode content);
        /// <summary>Removes all tabs and their content.</summary>
        void ClearTabs();
    }
}
