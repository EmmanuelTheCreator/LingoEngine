using Godot;
using LingoEngine.Bitmaps;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.LGodot.Helpers;
using LingoEngine.Pictures;
using LingoEngine.Tools;
using Microsoft.Extensions.Logging;

namespace LingoEngine.LGodot.Pictures
{
    public class LingoGodotMemberBitmap : ILingoFrameworkMemberBitmap, IDisposable
    {
        private LingoMemberBitmap _lingoMemberPicture;
        private ImageTexture? _imageTexture;
        private ILingoImageTexture? _imageTextureLingo;
        private Image? _image;
        private readonly ILogger _logger;

        public ILingoImageTexture? Texture => _imageTextureLingo;
        public ImageTexture? TextureGodot
        {
            get
            {
                if (!IsLoaded) Preload();
                return _imageTexture;
            }
        }

        public byte[]? ImageData { get; private set; }

        public bool IsLoaded { get; private set; }
        /// <summary>
        /// Optional MIME type or encoding format (e.g., "image/png", "image/jpeg")
        /// </summary>
        public string Format { get; private set; } = "image/unknown";

        public int Width { get; private set; }

        public int Height { get; private set; }

#pragma warning disable CS8618 
        public LingoGodotMemberBitmap(ILogger<LingoGodotMemberBitmap> logger)
#pragma warning restore CS8618 
        {
            _logger = logger;
        }

        internal void Init(LingoMemberBitmap lingoInstance)
        {
            _lingoMemberPicture = lingoInstance;
            CreateTexture();
        }
        /// <summary>
        /// Creates an ImageTexture from the ImageData byte array.
        /// </summary>
        public void CreateTexture()
        {
            // Create a new Image object
            _image = new Image();
            string filePath = GodotHelper.EnsureGodotUrl(_lingoMemberPicture.FileName);

            // Load the image from the byte array (assuming PNG/JPEG format)
            var texture = ResourceLoader.Load<Texture2D>(filePath);

            if (texture == null)
            {
                _logger.LogWarning($"Failed to load Texture2D at {filePath}");
                return;
            }

            Image image;
            try
            {
                image = texture.GetImage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Texture.GetImage() failed at {filePath}. Is 'Keep -> Image' enabled in import settings?");
                return;
            }

            if (image.IsEmpty())
            {
                _logger.LogError($"Image is empty: {filePath}");
                return;
            }
            _image = image;
            _imageTexture = ImageTexture.CreateFromImage(_image);
            if (_imageTexture == null) return;
            _imageTextureLingo = new LingoGodotImageTexture(_imageTexture);

            UpdateImageData(_image);
        }

        

        private void UpdateImageData(Image image)
        {
            Width = image.GetWidth();
            Height = image.GetHeight();
            ImageData = image.GetData();

            Format = MimeHelper.GetMimeType(_lingoMemberPicture.FileName);
            _lingoMemberPicture.Size = ImageData.Length;
            _lingoMemberPicture.Width = Width;
            _lingoMemberPicture.Height = Height;
        }
        public void Erase()
        {
            Unload();
            _image?.Dispose();
            ImageData = null;
            IsLoaded = false;
        }


        public void Preload()
        {
            if (IsLoaded) return;
            if (_image == null) CreateTexture();
            IsLoaded = true;
            if (_image == null || _image.IsEmpty())
                return;
        }

        public void Unload()
        {
            _imageTexture?.Dispose();
            _imageTexture = null;
            _imageTextureLingo = null;
            IsLoaded = false;
        }

        public void Dispose()
        {
            _image?.Dispose();
            _imageTexture?.Dispose();
        }
        public void CopyToClipboard()
        {
            if (_image == null) CreateTexture();
            if (ImageData == null) return;
            var base64 = Convert.ToBase64String(ImageData);
            DisplayServer.ClipboardSet("data:" + Format + ";base64," + base64);
        }
        public void PasteClipboardInto()
        {
            _image = DisplayServer.ClipboardGetImage();
            UpdateImageData(_image);
        }

        public void ImportFileInto()
        {
        }

        public Image GetImageCopy()
        {
            if (_image == null)
                throw new InvalidOperationException("Image not loaded.");

            return _image.Duplicate() as Image
                ?? throw new InvalidOperationException("Failed to duplicate image.");
        }
        public void ApplyImage(Image editedImage)
        {
            _image?.Dispose(); // Dispose old image
            _image = editedImage.Duplicate() as Image ?? throw new InvalidOperationException("Failed to copy edited image.");
            _imageTexture?.Dispose();

            _imageTexture = ImageTexture.CreateFromImage(_image);
            UpdateImageData(_image); // Also updates width, height, and member size

            // Update the member's image data directly
            _lingoMemberPicture.SetImageData(_image.GetData());
        }

        public void SetImageData(byte[] bytes) => this.ImageData = bytes;

       
    }
}
