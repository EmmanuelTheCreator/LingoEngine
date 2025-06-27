using Godot;
using LingoEngine.Bitmaps;

namespace LingoEngine.LGodot.Bitmaps
{
    public class LingoGodotImageTexture : ILingoImageTexture
    {
        private readonly ImageTexture _imageTexture;
        public ImageTexture Texture => _imageTexture;

        public LingoGodotImageTexture(ImageTexture imageTexture)
        {
            _imageTexture = imageTexture;
        }

        public int Width => _imageTexture.GetWidth();

        public int Height => _imageTexture._GetHeight();
    }
    public class LingoGodotTexture2D : ILingoTexture2D
    {
        private readonly Texture2D _texture;
        public Texture2D Texture => _texture;

        public LingoGodotTexture2D(Texture2D imageTexture)
        {
            _texture = imageTexture;
        }

        public int Width => _texture.GetWidth();

        public int Height => _texture._GetHeight();
    }
}
