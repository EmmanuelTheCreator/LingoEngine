namespace Director.Fonts
{
    public class FontXPlatformInfo : IDisposable
    {
        public string ToFont = string.Empty;
        public bool RemapChars;
        public Dictionary<ushort, ushort> SizeMap = new();

        public void Dispose()
        {
            SizeMap.Clear();
        }
    }

}


