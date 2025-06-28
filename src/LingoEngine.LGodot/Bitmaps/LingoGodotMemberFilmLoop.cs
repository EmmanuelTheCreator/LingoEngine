using Godot;
using LingoEngine.FilmLoops;
using LingoEngine.Pictures;

namespace LingoEngine.LGodot.Pictures
{
    /// <summary>
    /// Godot framework implementation for film loop members.
    /// </summary>
    public class LingoGodotMemberFilmLoop : ILingoFrameworkMemberFilmLoop, IDisposable
    {
        private LingoMemberFilmLoop _member = null!;
        public bool IsLoaded { get; private set; }
        public byte[]? Media { get; set; }
        public LingoFilmLoopFraming Framing { get; set; } = LingoFilmLoopFraming.Auto;
        public bool Loop { get; set; } = true;

        public LingoGodotMemberFilmLoop()
        {
        }

        internal void Init(LingoMemberFilmLoop member)
        {
            _member = member;
        }

        public void Preload()
        {
            IsLoaded = true;
        }

        public void Unload()
        {
            IsLoaded = false;
        }

        public void Erase()
        {
            Media = null;
            Unload();
        }

        public void ImportFileInto()
        {
            // Placeholder for future import logic
        }

        public void CopyToClipboard()
        {
            if (Media == null) return;
            var base64 = Convert.ToBase64String(Media);
            DisplayServer.ClipboardSet(base64);
        }

        public void PasteClipboardInto()
        {
            var data = DisplayServer.ClipboardGet();
            if (string.IsNullOrEmpty(data)) return;
            try
            {
                Media = Convert.FromBase64String(data);
            }
            catch
            {
                // ignore malformed clipboard data
            }
        }

        public void Dispose()
        {
            Unload();
        }
    }
}
