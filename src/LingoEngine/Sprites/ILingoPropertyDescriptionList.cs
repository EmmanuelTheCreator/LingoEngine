namespace LingoEngine.Sprites
{
    using LingoEngine.Primitives;

    /// <summary>
    /// Interface for behaviors that expose property description list support.
    /// Mirrors the Lingo handlers getPropertyDescriptionList, getBehaviorDescription,
    /// getBehaviorTooltip, runPropertyDialog and isOKToAttach.
    /// </summary>
    public interface ILingoPropertyDescriptionList
    {
        /// <summary>Returns property descriptions for this behavior.</summary>
        BehaviorPropertiesContainer? GetPropertyDescriptionList();

        /// <summary>Returns a descriptive string for the behavior.</summary>
        string? GetBehaviorDescription();

        /// <summary>Returns the tooltip string for the behavior.</summary>
        string? GetBehaviorTooltip();

        /// <summary>
        /// Called when the user edits properties. Implementations should
        /// update and return the initializer list.
        /// </summary>
        BehaviorPropertiesContainer? RunPropertyDialog(BehaviorPropertiesContainer currentInitializerList);

        /// <summary>
        /// Called before attaching the behavior to a sprite. Should return
        /// <see langword="true"/> if the attachment is allowed.
        /// </summary>
        bool IsOKToAttach(LingoSymbol spriteType, int spriteNum);
    }
}
