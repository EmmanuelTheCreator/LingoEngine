using LingoEngine.Bitmaps;

namespace LingoEngine.Director.Core.Icons
{
    public abstract class DirectorIconManager<TLingoIconSheet> : IDirectorIconManager
        where TLingoIconSheet : ILingoIconSheet
    {

        private readonly List<TLingoIconSheet> _sheets = new();
        private readonly Dictionary<DirectorIcon, ILingoImageTexture> _iconCache = new();


        public void LoadSheet(string path, int itemCount, int iconWidth, int iconHeight, int horizontalSpacing = 0)
        {
            var sheet = OnLoadSheet(path, itemCount, iconWidth, iconHeight, horizontalSpacing);
            if (sheet == null) return;
            _sheets.Add(sheet);
        }

        protected abstract TLingoIconSheet? OnLoadSheet(string path, int itemCount, int iconWidth, int iconHeight, int horizontalSpacing = 0);


        public ILingoImageTexture Get(DirectorIcon icon)
        {
            if (_iconCache.TryGetValue(icon, out var cached))
                return cached;

            int index = (int)icon;
            int currentIndex = 0;
            foreach (var sheet in _sheets)
            {
                int count = sheet.IconCount;
                if (index < currentIndex + count)
                {
                    int localIndex = index - currentIndex;
                    int x = localIndex * (sheet.IconWidth + sheet.HorizontalSpacing);
                    var lingoTexture = OnGetTextureImage(sheet, x);
                    if (lingoTexture == null)
                        continue;

                    _iconCache[icon] = lingoTexture;
                    return lingoTexture;
                }
                currentIndex += count;
            }

            throw new ArgumentOutOfRangeException(nameof(icon), "Icon index out of range.");
        }
        protected abstract ILingoImageTexture? OnGetTextureImage(TLingoIconSheet sheet, int x);

    }
}
