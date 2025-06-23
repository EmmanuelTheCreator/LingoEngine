

using Godot;
using LingoEngine.Core;
using LingoEngine.Director.Core.Commands;

namespace LingoEngine.Director.LGodot.Pictures
{

    /// <summary>
    /// Helper for pixel-based painting on an editable ImageTexture.
    /// Used only in the Director editor for editing LingoMemberPicture images.
    /// </summary>
    public class PicturePainter : 
        ICommandHandler<PainterToolSelectCommand>,
        ICommandHandler<PainterDrawPixelCommand>,
        ICommandHandler<PainterFillCommand>
    {
        private Image _image;
        private ImageTexture _texture;

        public PicturePainter(Image sourceImage)
        {
            _image = sourceImage.Duplicate() as Image ?? throw new InvalidOperationException("Failed to duplicate image.");

            // In Godot 4.5: use flags to control filtering
            _texture = ImageTexture.CreateFromImage(_image);
            

        }

        public Texture2D Texture => _texture;

        public void PaintPixel(Vector2I pixel, Color color)
        {
            _image.SetPixelv(pixel, color);
        }

        public void Commit()
        {
            _texture = ImageTexture.CreateFromImage(_image);
        }

        public void Dispose()
        {
            // No explicit unlock needed anymore
        }

        public Image GetImage() => _image.Duplicate() as Image ?? throw new InvalidOperationException("Failed to get image.");

        public bool Handle(PainterToolSelectCommand command)
        {
            return true;
        }

        public bool Handle(PainterDrawPixelCommand command)
        {
            return true;
        }

        public bool Handle(PainterFillCommand command)
        {
            return true;
        }

        public Vector2I Size => _image.GetSize();
    }


}
