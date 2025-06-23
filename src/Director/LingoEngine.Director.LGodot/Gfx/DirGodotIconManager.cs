using Godot;

namespace LingoEngine.Director.LGodot.Gfx
{
    public enum DirGodotEditorIcon
    {
        // General_Icons.png (left to right)
        MemberTypeBitmap,
        SelectionNotUsed,
        MemberTypeShape,
        MemberTypeText,
        MemberTypSound,
        MemberTypeMovieClip,
        MemberTypeVideo,
        Transition,
        DirectorIcon,
        ColorPalette,

        Script,
        MovieScript,
        MemberTypeField,
        MemberTypeButton,
        MemberTypeRadioButton,
        MemberTypeCheckbox,
        Xtra,
        PaintCache,
        ParentScript,
        BehaviorScript,


        // Painter_Icons.png (continuing index)
        PaintLasso,
        RectangleSelect,
        Crosshair,
        Eraser,
        Hand,
        Magnifier,
        ColorPicker,
        PaintBucket,
        Text,
        Pencil,
        PaintBrush,
        PaintLineCurve,
        PaintStraightLine,
        PaintSquareFilled,
        PaintSquare,
        PaintCircleFilled,
        PaintCircle,
        PaintFreeLineFilled,
        PaintFreeLine
    }
    public interface IDirGodotIconManager
    {
        Texture2D Get(DirGodotEditorIcon icon);
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
        private readonly Dictionary<DirGodotEditorIcon, Texture2D> _iconCache = new();



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


        public Texture2D Get(DirGodotEditorIcon icon)
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



    }
}
