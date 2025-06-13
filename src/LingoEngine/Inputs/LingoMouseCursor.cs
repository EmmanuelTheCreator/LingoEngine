namespace LingoEngine.Inputs
{
    /// <summary>
    /// Represents standard Lingo-compatible mouse cursor types.
    /// </summary>
    public enum LingoMouseCursor
    {
        /// <summary>
        /// Default arrow pointer.
        /// </summary>
        Arrow = 0,

        /// <summary>
        /// Crosshair cursor.
        /// </summary>
        Cross = 1,

        /// <summary>
        /// Watch or hourglass (waiting) cursor.
        /// </summary>
        Watch = 2,

        /// <summary>
        /// Text selection cursor (I-beam).
        /// </summary>
        IBeam = 3,

        /// <summary>
        /// Four-way resize or move cursor.
        /// </summary>
        SizeAll = 4,

        /// <summary>
        /// Diagonal resize cursor (NorthWest to SouthEast).
        /// </summary>
        SizeNWSE = 5,

        /// <summary>
        /// Diagonal resize cursor (NorthEast to SouthWest).
        /// </summary>
        SizeNESW = 6,

        /// <summary>
        /// Horizontal resize cursor (West to East).
        /// </summary>
        SizeWE = 7,

        /// <summary>
        /// Vertical resize cursor (North to South).
        /// </summary>
        SizeNS = 8,

        /// <summary>
        /// Upward pointing arrow cursor.
        /// </summary>
        UpArrow = 9,

        /// <summary>
        /// Invisible or blank cursor.
        /// </summary>
        Blank = 10,

        /// <summary>
        /// Finger or hand pointer, usually for links or interactive elements.
        /// </summary>
        Finger = 11,

        /// <summary>
        /// Drag-and-drop indicator cursor.
        /// </summary>
        Drag = 12,

        /// <summary>
        /// Help or question mark cursor.
        /// </summary>
        Help = 13,

        /// <summary>
        /// Alternative wait cursor.
        /// </summary>
        Wait = 14,

        /// <summary>
        /// Indicates a custom cursor image should be used.
        /// </summary>
        Custom = 100
    }
}
