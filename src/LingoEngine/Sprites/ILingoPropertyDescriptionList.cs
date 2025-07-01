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
        /// <summary>
        /// This method tells Director what custom properties a behavior has, what type of input they accept, and how they should appear in the Property Inspector. 
        /// It's part of Director’s way of exposing editable fields for behaviors, similar to how Unity or Godot might expose serialized properties in the Inspector.
        /// </summary>
        BehaviorPropertyDescriptionList? GetPropertyDescriptionList();

        /// <summary>Returns a descriptive string for the behavior.</summary>
        string? GetBehaviorDescription();

        /// <summary>Returns the tooltip string for the behavior.</summary>
        string? GetBehaviorTooltip();

        /// <summary>
        /// Called before attaching the behavior to a sprite. Should return
        /// <see langword="true"/> if the attachment is allowed.
        /// </summary>
        bool IsOKToAttach(LingoSymbol spriteType, int spriteNum);
    }
    public interface ILingoPropertyDescriptionListDialog
    {

        /// <summary>
        /// Called when the user edits properties. Implementations should
        /// update and return the initializer list.
        /// </summary>
        BehaviorPropertiesContainer RunPropertyDialog(BehaviorPropertiesContainer currentInitializerList);
    }
}
