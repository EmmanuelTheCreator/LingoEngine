namespace LingoEngine
{
    /// <summary>
    /// Used to monitor a user’s keyboard activity.
    /// Mirrors Lingo's _key object functionality for key state and input monitoring.
    /// Example: isCtrlDown = _key.controlDown
    /// </summary>
    public interface ILingoKey
    {
        /// <summary>
        /// Returns TRUE if the Command key is currently pressed (Mac only).
        /// </summary>
        bool CommandDown { get; }

        /// <summary>
        /// Returns TRUE if the Control key is currently pressed.
        /// </summary>
        bool ControlDown { get; }

        /// <summary>
        /// Returns the character corresponding to the most recently pressed key.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Returns the numeric key code for the last key pressed, useful for system-level key identification.
        /// </summary>
        int KeyCode { get; }

        /// <summary>
        /// Returns TRUE if the Option key (Mac) or Alt key (Windows) is currently pressed.
        /// </summary>
        bool OptionDown { get; }

        /// <summary>
        /// Returns TRUE if the Shift key is currently pressed.
        /// </summary>
        bool ShiftDown { get; }

        /// <summary>
        /// Returns TRUE if the specified character key is currently being pressed down.
        /// Equivalent to checking if a specific ASCII character is pressed.
        /// </summary>
        /// <param name="key">The character key to test (e.g., 'a', 'b', '1').</param>
        bool KeyPressed(char key);

        /// <summary>
        /// Returns TRUE if the specified special key is currently pressed.
        /// For example, BACKSPACE, ENTER, TAB, etc.
        /// </summary>
        /// <param name="key">A named key defined in the LingoKeyType enum.</param>
        bool KeyPressed(LingoKeyType key);
    }

    /// <summary>
    /// Enumeration of special keys commonly referenced in Lingo key events.
    /// </summary>
    public enum LingoKeyType
    {
        BACKSPACE,
        ENTER,
        QUOTE,
        RETURN,
        SPACE,
        TAB
    }


    /// <inheritdoc/>
    public class LingoKey : ILingoKey
    {
        public bool ControlDown => false;
        public bool CommandDown => false;
        public bool OptionDown => false;
        public bool ShiftDown => false;
        public bool KeyPressed(LingoKeyType key) => false;
        public bool KeyPressed(char key) => false;
        public string Key => "";
        public int KeyCode => 10;
    }

}
