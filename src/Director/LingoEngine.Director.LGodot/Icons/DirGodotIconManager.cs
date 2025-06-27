using Godot;
using LingoEngine.Director.Core.Icons;
using LingoEngine.LGodot.Primitives;

namespace LingoEngine.Director.LGodot.Icons
{
    public interface IDirGodotIconManager : IDirectorIconManager
    {
        Texture2D Get(DirectorIcon icon);
    }
    public partial class DirGodotIconManager : IDirGodotIconManager
    {
        private class IconSheet
        {
            public Image Image { get; set; } = null!;
            public int IconWidth { get; set; }
            public int IconHeight { get; set; }
            public int HorizontalSpacing { get; set; }
            public int IconCount { get; set; }
        }

        private readonly List<IconSheet> _sheets = new();
        private readonly Dictionary<DirectorIcon, Texture2D> _iconCache = new();
        private readonly Dictionary<DirectorIcon, LingoIconData> _dataCache = new();



        public void LoadSheet(string path, int itemCount, int iconWidth, int iconHeight, int horizontalSpacing = 0)
        {
            var texture = GD.Load<Texture2D>(path);
            if (texture == null)
                throw new InvalidOperationException($"Failed to load texture: {path}");

            var image = texture.GetImage();
            if (image == null || image.IsEmpty())
                throw new InvalidOperationException($"Failed to get image from texture: {path}");

            _sheets.Add(new IconSheet
            {
                Image = image,
                IconWidth = iconWidth,
                IconHeight = iconHeight,
                HorizontalSpacing = horizontalSpacing,
                IconCount = itemCount
            });
        }


        public Texture2D Get(DirectorIcon icon)
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

                    var sub = Image.CreateEmpty(sheet.IconWidth, sheet.IconHeight, false, sheet.Image.GetFormat());
                    sub.BlitRect(sheet.Image, new Rect2I(x, 0, sheet.IconWidth, sheet.IconHeight), Vector2I.Zero);

                    var tex = ImageTexture.CreateFromImage(sub);
                    _iconCache[icon] = tex;
                    return tex;
                }
                currentIndex += count;
            }

            throw new ArgumentOutOfRangeException(nameof(icon), "Icon index out of range.");
        }

        public LingoIconData GetData(DirectorIcon icon)
        {
            if (_dataCache.TryGetValue(icon, out var data))
                return data;

            var tex = Get(icon);
            var img = tex.GetImage();
            img.Convert(Image.Format.Rgba8);
            var bytes = img.GetData();
            data = new LingoIconData(bytes, img.GetWidth(), img.GetHeight(), img.GetFormat().ToLingoFormat());
            _dataCache[icon] = data;
            return data;
        }



    }
}
