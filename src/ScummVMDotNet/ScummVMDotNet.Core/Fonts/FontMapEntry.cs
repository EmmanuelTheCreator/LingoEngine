namespace Director.Fonts
{
    public class FontMapEntry
    {
        public ushort ToFont { get; set; }
        public bool RemapChars;
        public Dictionary<ushort, ushort> SizeMap { get; } = new();

        public FontMapEntry(ushort toFont, bool remapChars)
        {
            ToFont = toFont;
            RemapChars = remapChars;
        }
    }
    
}
