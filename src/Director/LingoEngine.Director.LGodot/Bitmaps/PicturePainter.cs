

using Godot;

namespace LingoEngine.Director.LGodot.Bitmaps
{

    /// <summary>
    /// Helper for pixel-based painting on an editable ImageTexture.
    /// Used only in the Director editor for editing LingoMemberPicture images.
    /// </summary>
    public class PicturePainter
    {
        private Image _image;
        private ImageTexture _texture;
        private Vector2I _offset = Vector2I.Zero;

        public PicturePainter(Image sourceImage)
        {
            _image = sourceImage.Duplicate() as Image ?? throw new InvalidOperationException("Failed to duplicate image.");

            // In Godot 4.5: use flags to control filtering
            _texture = ImageTexture.CreateFromImage(_image);


        }

        public Texture2D Texture => _texture;

        public void PaintPixel(Vector2I pixel, Color color)
        {
            EnsureCapacity(ref pixel);
            _image.SetPixelv(pixel, color);
        }

        public void ErasePixel(Vector2I pixel)
        {
            PaintPixel(pixel, Colors.Transparent);
        }

        public void EraseBrush(Vector2I pixel, int size)
        {
            PaintBrush(pixel, Colors.Transparent, size);
        }

        public void PaintBrush(Vector2I pixel, Color color, int size)
        {
            int radius = Math.Max(0, size / 2);
            EnsureCapacityForBrush(ref pixel, radius);

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y > radius * radius)
                        continue;
                    _image.SetPixelv(pixel + new Vector2I(x, y), color);
                }
            }
        }

        private void EnsureCapacityForBrush(ref Vector2I center, int radius)
        {
            Vector2I bottomRight = center + new Vector2I(radius, radius);
            Vector2I topLeft = center - new Vector2I(radius, radius);

            int leftPad = Math.Max(0, -topLeft.X);
            int topPad = Math.Max(0, -topLeft.Y);
            int rightPad = Math.Max(0, bottomRight.X - (_image.GetWidth() - 1));
            int bottomPad = Math.Max(0, bottomRight.Y - (_image.GetHeight() - 1));

            if (leftPad == 0 && topPad == 0 && rightPad == 0 && bottomPad == 0)
                return;

            int newWidth = _image.GetWidth() + leftPad + rightPad;
            int newHeight = _image.GetHeight() + topPad + bottomPad;
            var newImage = Image.Create(newWidth, newHeight, false, _image.GetFormat());
            newImage.Fill(Colors.Transparent);
            newImage.BlitRect(_image, new Rect2I(0, 0, _image.GetWidth(), _image.GetHeight()), new Vector2I(leftPad, topPad));
            _image = newImage;
            _offset += new Vector2I(leftPad, topPad);
            center += new Vector2I(leftPad, topPad);
        }

        private void EnsureCapacity(ref Vector2I pixel)
        {
            int leftPad = Math.Max(0, -pixel.X);
            int topPad = Math.Max(0, -pixel.Y);
            int rightPad = Math.Max(0, pixel.X - (_image.GetWidth() - 1));
            int bottomPad = Math.Max(0, pixel.Y - (_image.GetHeight() - 1));

            if (leftPad == 0 && topPad == 0 && rightPad == 0 && bottomPad == 0)
                return;

            int newWidth = _image.GetWidth() + leftPad + rightPad;
            int newHeight = _image.GetHeight() + topPad + bottomPad;
            var newImage = Image.Create(newWidth, newHeight, false, _image.GetFormat());
            newImage.Fill(Colors.Transparent);
            newImage.BlitRect(_image, new Rect2I(0, 0, _image.GetWidth(), _image.GetHeight()), new Vector2I(leftPad, topPad));
            _image = newImage;
            _offset += new Vector2I(leftPad, topPad);
            pixel += new Vector2I(leftPad, topPad);
        }

        public void Commit()
        {
            _texture = ImageTexture.CreateFromImage(_image);
        }

        public void SetState(Image image, Vector2I offset)
        {
            _image = image.Duplicate() as Image
                ?? throw new InvalidOperationException("Failed to duplicate image.");
            _offset = offset;
            _texture = ImageTexture.CreateFromImage(_image);
        }

        public void Dispose()
        {
            // No explicit unlock needed anymore
        }

        public Image GetImage() => _image.Duplicate() as Image ?? throw new InvalidOperationException("Failed to get image.");

        public Vector2I Size => _image.GetSize();
        public Vector2I Offset => _offset;
    }


}
