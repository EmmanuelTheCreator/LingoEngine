using Director.Graphics;
using Director.Primitives;

namespace Director
{

    public class DirectorApp
    {
        public static DirectorApp Instance { get; } = new DirectorApp();

        public WindowManager WindowManager { get; } = new WindowManager();

        // Shorthand property for compatibility with g_director->_wm
        public WindowManager _wm => WindowManager;

        private readonly Dictionary<CastMemberID, (byte[] Palette, int Length)> _palettes = new();

        public PixelFormat PixelFormat { get; set; } = PixelFormat.CreateFormatCLUT8();

        public CastMemberID CurrentPaletteId { get; private set; } = new();
        public bool DesktopEnabled { get; internal set; }
        public object CenterStage { get; internal set; }
        public ushort WmHeight { get; internal set; }
        public ushort WmWidth { get; internal set; }
        public CastMemberID LastPalette { get; internal set; }

        public byte[] GetPalette()
        {
            return _palettes.TryGetValue(CurrentPaletteId, out var entry) ? entry.Palette : Array.Empty<byte>();
        }

        public int GetPaletteSize()
        {
            return _palettes.TryGetValue(CurrentPaletteId, out var entry) ? entry.Length : 0;
        }

        public void AddPalette(CastMemberID id, byte[] palette, int length)
        {
            _palettes[id] = (palette, length);
        }

        public void SetPalette(CastMemberID id)
        {
            if (_palettes.ContainsKey(id))
                CurrentPaletteId = id;
        }
        /// <summary>
        /// Returns true if a palette with the given CastMemberID exists.
        /// </summary>
        public bool HasPalette(CastMemberID id)
        {
            return _palettes.ContainsKey(id);
        }
        private DirectorApp() { }
    }
    public class WindowManager
    {
        public PixelFormat PixelFormat { get; private set; } = new PixelFormat(1, 0, 0, 0, 0, 0, 0, 0, 0);

        private byte[] _palette = new byte[256 * 3];

        public byte[] GetPalette() => _palette;

        public int GetPaletteSize() => _palette.Length;
    }
}

