using Godot;

namespace LingoEngine.Director.Godot.Gfx
{
    public class IconManager
    {
        // todo : Media\Icons\AssetInfoMapIcon12_Color.png
        private class IconSheet
        {
            public Image Image {get;set;}
            public int IconWidth {get;set;}
            public int IconHeight { get; set; }
            public int IconCount => Image.GetWidth() / IconWidth;
        }

        private readonly List<IconSheet> _sheets = new();

        public void AddBitmap(Image image, int iconWidth, int iconHeight)
        {
            _sheets.Add(new IconSheet
            {
                Image = image,
                IconWidth = iconWidth,
                IconHeight = iconHeight
            });
        }

        public Texture2D GetIcon(int globalIndex)
        {
            int currentIndex = 0;
            foreach (var sheet in _sheets)
            {
                int count = sheet.IconCount;
                if (globalIndex < currentIndex + count)
                {
                    int localIndex = globalIndex - currentIndex;
                    int x = localIndex * sheet.IconWidth;

                    // Copy the region into a new Image
                    var subImage = Image.CreateEmpty(sheet.IconWidth, sheet.IconHeight, false, sheet.Image.GetFormat());
                    subImage.BlitRect(sheet.Image, new Rect2I(x, 0, sheet.IconWidth, sheet.IconHeight), Vector2I.Zero);

                    return ImageTexture.CreateFromImage(subImage);
                }
                currentIndex += count;
            }

            throw new IndexOutOfRangeException("Icon index out of range.");
        }

    }

}
