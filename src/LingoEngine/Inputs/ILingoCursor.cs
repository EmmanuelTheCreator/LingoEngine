
using LingoEngine.Pictures;

namespace LingoEngine.Inputs
{
    /// <summary>
    /// Provides access to and control over the mouse cursor.
    /// Lingo equivalent: the cursor, cursor(n), and mouse-related display control.
    /// </summary>
    public interface ILingoCursor
    {
        /// <summary>
        /// Gets or sets the current cursor shape by ID (1–280 depending on platform and version).
        /// Lingo: the cursor
        /// </summary>
        int Cursor { get; set; }

        /// <summary>
        /// Sets a custom image for the mouse cursor
        /// </summary>
        LingoMemberPicture? Image { get; set; }

        /// <summary>
        /// Determines whether the cursor is currently visible.
        /// </summary>
        bool IsCursorVisible { get; }

        /// <summary>
        /// Shows the system cursor.
        /// Lingo: showCursor
        /// </summary>
        void ShowCursor();

        /// <summary>
        /// Hides the system cursor.
        /// Lingo: hideCursor
        /// </summary>
        void HideCursor();



        /// <summary>
        /// Resets the cursor to the system default.
        /// </summary>
        void ResetCursor();
    }
}
