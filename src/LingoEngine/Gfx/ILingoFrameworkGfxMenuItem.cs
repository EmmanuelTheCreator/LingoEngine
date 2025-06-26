namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific representation of a single menu entry.
    /// </summary>
    public interface ILingoFrameworkGfxMenuItem
    {
        /// <summary>Displayed name of the item.</summary>
        string Name { get; set; }
        /// <summary>Whether the item can be selected.</summary>
        bool Enabled { get; set; }
        /// <summary>Whether the item shows a check mark.</summary>
        bool CheckMark { get; set; }
        /// <summary>Keyboard shortcut shown for this item.</summary>
        string? Shortcut { get; set; }
        /// <summary>Raised when the item is activated by the user.</summary>
        event System.Action? Activated;
    }
}
