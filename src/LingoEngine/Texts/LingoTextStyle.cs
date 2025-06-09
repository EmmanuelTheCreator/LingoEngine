namespace LingoEngine.Texts
{
    /// <summary>
    /// Represents Lingo text styles using bitmask values.
    /// You can combine styles using bitwise OR: Bold | Italic | Underline.
    /// </summary>
    [Flags]
    public enum LingoTextStyle
    {
        /// <summary>No style.</summary>
        None = 0,

        /// <summary>Bold text.</summary>
        Bold = 1,

        /// <summary>Italic text.</summary>
        Italic = 2,

        /// <summary>Underlined text.</summary>
        Underline = 4
    }

}



