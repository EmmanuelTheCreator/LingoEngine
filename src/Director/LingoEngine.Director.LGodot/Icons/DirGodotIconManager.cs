using Godot;
using LingoEngine.Bitmaps;
using LingoEngine.Director.Core.Icons;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.LGodot.Helpers;
using Microsoft.Extensions.Logging;

namespace LingoEngine.Director.LGodot.Icons
{

    public class LingoIconSheetGodot : LingoIconSheet<LingoGodotTexture2D>
    {
        public LingoIconSheetGodot(LingoGodotTexture2D image, int iconWidth, int iconHeight, int horizontalSpacing, int iconCount) : base(image, iconWidth, iconHeight, horizontalSpacing, iconCount)
        {
        }
    }


    public partial class DirGodotIconManager : DirectorIconManager<LingoIconSheetGodot>
    {
        private readonly ILogger _logger;

        public DirGodotIconManager(ILogger<DirGodotIconManager> logger)
        {
            _logger = logger;
        }
        protected override LingoIconSheetGodot? OnLoadSheet(string path, int itemCount, int iconWidth, int iconHeight, int horizontalSpacing = 0)
        {
            string pathResource = GodotHelper.EnsureGodotUrl(path);
            var texture = GD.Load<Texture2D>(pathResource);

            if (texture == null )
            {
                _logger.LogWarning($"Failed to load texture: {path}");
                return null;
            }
            var lingoTexture = new LingoGodotTexture2D(texture);
            return new LingoIconSheetGodot(lingoTexture,iconWidth,iconHeight,horizontalSpacing, itemCount);
        }
        protected override ILingoImageTexture? OnGetTextureImage(LingoIconSheetGodot sheet, int x)
        {
            var texture = sheet.Image.Texture;
            var image = texture.GetImage();
            // Create an image with the correct dimensions for the icon
            var sub = Image.CreateEmpty(sheet.IconWidth, sheet.IconHeight, false, image.GetFormat());
            // Copy the icon region from the sprite sheet
            sub.BlitRect(image, new Rect2I(x, 0, sheet.IconWidth, sheet.IconHeight), Vector2I.Zero);
            var tex = ImageTexture.CreateFromImage(sub);
            var lingoTexture = new LingoGodotImageTexture(tex);
            return lingoTexture;
        }
    }
}
