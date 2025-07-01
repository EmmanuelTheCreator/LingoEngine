using LingoEngine.Primitives;
using System;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific window container.
    /// </summary>
    public interface ILingoFrameworkGfxWindow : ILingoFrameworkGfxLayoutNode
    {
        /// <summary>Window title.</summary>
        string Title { get; set; }
        LingoColor BackgroundColor { get; set; }
        bool IsPopup { get; set; }

        /// <summary>Raised when the window is opened.</summary>
        event Action? OnOpen;
        /// <summary>Raised when the window is closed.</summary>
        event Action? OnClose;
        /// <summary>Raised when the window is resized. Parameters are new width and height.</summary>
        event Action<float, float>? OnResize;


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
