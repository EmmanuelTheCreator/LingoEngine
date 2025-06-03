using System;
using System.Collections.Generic;
using System.Text;

namespace LingoEngine
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
        /// Determines whether the cursor is currently visible.
        /// </summary>
        bool IsCursorVisible { get; }

        /// <summary>
        /// Resets the cursor to the system default.
        /// </summary>
        void ResetCursor();
    }

    public class LingoCursor : ILingoCursor
    {
        public int Cursor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsCursorVisible => throw new NotImplementedException();

        public void HideCursor()
        {
            throw new NotImplementedException();
        }

        public void ResetCursor()
        {
            throw new NotImplementedException();
        }

        public void ShowCursor()
        {
            throw new NotImplementedException();
        }
    }
}
