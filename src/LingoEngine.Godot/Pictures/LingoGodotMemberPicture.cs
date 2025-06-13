using Godot;
using LingoEngine.Pictures;
using LingoEngine.Pictures.LingoEngine;

namespace LingoEngineGodot.Pictures
{
    public class LingoGodotMemberPicture : ILingoFrameworkMemberPicture, IDisposable
    {
        private LingoMemberPicture _lingoMemberPicture;
        private ImageTexture? _imageTexture;
        private Image? _image;

        public ImageTexture? Texture => _imageTexture;
        public byte[]? ImageData {get;private set;}

        public bool IsLoaded {get;private set;}
        /// <summary>
        /// Optional MIME type or encoding format (e.g., "image/png", "image/jpeg")
        /// </summary>
        public string Format {get;private set;} = "image/unknown";

        public int Width {get;private set;}

        public int Height {get;private set;}

#pragma warning disable CS8618 
        public LingoGodotMemberPicture()
#pragma warning restore CS8618 
        {
            
        }

        internal void Init(LingoMemberPicture lingoInstance)
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
            
            
            // Load the image from the byte array (assuming PNG/JPEG format)
            var error = _image.Load($"res://{_lingoMemberPicture.FileName}");
            if (error != Error.Ok)
            {
                GD.PrintErr("Failed to load image data.:"+ _lingoMemberPicture.FileName+":" +error);
                return;
            }
            UpdateImageData(_image);
        }

       

        public string GetMimeType(string filename)
        {
            // Extract file extension from the filename (case-insensitive)
            string extension = Path.GetExtension(filename)?.ToLower()!;

            // Use a switch statement to return the MIME type
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",  // JPEG files usually have the extension ".jpg" or ".jpeg"
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream" // Default MIME type for unknown extensions
            };
        }
        private void UpdateImageData(Image image)
        {
            Width = image.GetWidth();
            Height = image.GetHeight();
            ImageData = image.GetData();

            Format = GetMimeType(_lingoMemberPicture.FileName);
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
            _imageTexture = ImageTexture.CreateFromImage(_image);
            IsLoaded = true;
        }

        public void Unload()
        {
            _imageTexture?.Dispose();
            _imageTexture = null;
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
    }
}
