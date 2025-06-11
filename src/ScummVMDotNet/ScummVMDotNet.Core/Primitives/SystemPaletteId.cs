namespace Director.Primitives
{
    /// <summary>
    /// Represents known system palette IDs used by Director movies.
    /// </summary>
    public enum SystemPaletteId
    {
        /// <summary>Default system palette for Macintosh.</summary>
        ClutSystemMac = 1,

        /// <summary>Default system palette for Windows.</summary>
        ClutSystemWin = 2,

        /// <summary>Default game palette used when no palette is specified.</summary>
        ClutGameDefault = 3,

        /// <summary>Placeholder for 16-bit high color mode (non-CLUT).</summary>
        ClutHighColor = 4,

        /// <summary>Custom palette associated with the stage or cast.</summary>
        ClutStage = 5,

        /// <summary>User-defined or imported custom palette.</summary>
        ClutCustom = 6,

        /// <summary>Palette that applies to the entire movie.</summary>
        ClutMovie = 7,

        /// <summary>Reserved for internal use by the engine.</summary>
        ClutInternal = 8
    }
}
