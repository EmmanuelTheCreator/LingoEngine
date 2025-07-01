using System.Collections.Generic;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific window container.
    /// </summary>
    public interface ILingoFrameworkGfxWindow : ILingoFrameworkGfxLayoutNode
    {
        /// <summary>Window title.</summary>
        string Title { get; set; }

        /// <summary>Adds a child node to the window.</summary>
        void AddItem(ILingoFrameworkGfxLayoutNode child);
        void RemoveItem(ILingoFrameworkGfxLayoutNode child);
        IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems();

        /// <summary>Shows the window at its current position.</summary>
        void Popup();
        /// <summary>Centers the window on screen and shows it.</summary>
        void PopupCentered();
        /// <summary>Hides the window.</summary>
        void Hide();
    }
}
