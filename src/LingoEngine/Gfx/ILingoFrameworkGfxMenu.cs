using System.Collections.Generic;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific menu container capable of holding menu items.
    /// </summary>
    public interface ILingoFrameworkGfxMenu : ILingoFrameworkGfxNode
    {
        /// <summary>Name of the menu.</summary>
        string Name { get; set; }
        /// <summary>Adds an item to the menu.</summary>
        void AddItem(ILingoFrameworkGfxMenuItem item);
        /// <summary>Removes all items from the menu.</summary>
        void ClearItems();
        /// <summary>Positions the popup relative to a button.</summary>
        void PositionPopup(ILingoFrameworkGfxButton button);
        /// <summary>Shows the menu.</summary>
        void Popup();
    }
}
