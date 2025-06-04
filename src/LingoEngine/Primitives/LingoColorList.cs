namespace LingoEngine.Primitives
{
    /// <summary>
    /// Simulates Lingo's colorList: a predefined set of named system colors.
    /// Includes static properties for convenient access and a dictionary for lookup.
    /// </summary>
    public static class LingoColorList
    {
        // Static predefined colors
        public static LingoColor Black => new(0, 0, 0, 0, "black");
        public static LingoColor White => new(255, 255, 255, 255, "white");
        public static LingoColor Red => new(1, 255, 0, 0, "red");
        public static LingoColor Green => new(2, 0, 255, 0, "green");
        public static LingoColor Blue => new(3, 0, 0, 255, "blue");
        public static LingoColor Yellow => new(4, 255, 255, 0, "yellow");
        public static LingoColor Cyan => new(5, 0, 255, 255, "cyan");
        public static LingoColor Magenta => new(6, 255, 0, 255, "magenta");
        public static LingoColor Gray => new(7, 128, 128, 128, "gray");
        public static LingoColor LightGray => new(8, 192, 192, 192, "lightgray");
        public static LingoColor DarkGray => new(9, 64, 64, 64, "darkgray");

        /// <summary>
        /// Dictionary of all predefined Lingo colors by lowercase name.
        /// </summary>
        public static readonly Dictionary<string, LingoColor> Colors = new()
        {
            { "black", Black },
            { "white", White },
            { "red", Red },
            { "green", Green },
            { "blue", Blue },
            { "yellow", Yellow },
            { "cyan", Cyan },
            { "magenta", Magenta },
            { "gray", Gray },
            { "lightgray", LightGray },
            { "darkgray", DarkGray },
        };

        /// <summary>
        /// Attempts to retrieve a color from the color list by name.
        /// </summary>
        public static bool TryGetColor(string name, out LingoColor color) =>
            Colors.TryGetValue(name.ToLowerInvariant(), out color);
    }
}



