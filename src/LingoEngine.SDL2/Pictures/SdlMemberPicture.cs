using System.Runtime.InteropServices;
using LingoEngine.Pictures;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.SDL2;
using LingoEngine.SDL2.SDLL;
using LingoEngine.Tools;

namespace LingoEngine.SDL2.Pictures;

public class SdlMemberPicture : ILingoFrameworkMemberPicture, IDisposable
{
    private LingoMemberPicture _member = null!;
    private nint _surface = nint.Zero;
    private SDL.SDL_Surface _surfacePtr;
    public byte[]? ImageData { get; private set; }
    public bool IsLoaded { get; private set; }
    public string Format { get; private set; } = "image/unknown";
    public int Width { get; private set; }
    public int Height { get; private set; }
    public SDL.SDL_Surface Texture => _surfacePtr;
    internal nint Surface => _surface;

    internal void Init(LingoMemberPicture member)
    {
        _member = member;
    }

    public void Preload()
    {
        if (IsLoaded)
            return;
        // For some unknown reason Path.Combine is not working :(
        var fullFileName = Directory.GetCurrentDirectory()+ _member.FileName;
        if (!File.Exists(fullFileName))
            return;

        _surface = SDL_image.IMG_Load(fullFileName);
        if (_surface == nint.Zero)
            return;

        _surfacePtr = Marshal.PtrToStructure<SDL.SDL_Surface>(_surface);
        Width = _surfacePtr.w;
        Height = _surfacePtr.h;

        ImageData = File.ReadAllBytes(fullFileName);
        Format = MimeHelper.GetMimeType(_member.FileName);
        _member.Size = ImageData.Length;
        _member.Width = Width;
        _member.Height = Height;
        IsLoaded = true;
    }

    public void Unload()
    {
        if (_surface != nint.Zero)
        {
            SDL.SDL_FreeSurface(_surface);
            _surface = nint.Zero;
        }
        IsLoaded = false;
    }

    public void Erase()
    {
        Unload();
        ImageData = null;
    }

    public void Dispose() { Unload(); }
    public void CopyToClipboard()
    {
        if (ImageData == null) return;
        var base64 = Convert.ToBase64String(ImageData);
        SdlClipboard.SetText("data:" + Format + ";base64," + base64);
    }
    public void ImportFileInto() { }
    public void PasteClipboardInto()
    {
        var data = SdlClipboard.GetText();
        if (string.IsNullOrEmpty(data)) return;
        var parts = data.Split(',', 2);
        if (parts.Length != 2) return;

        var bytes = Convert.FromBase64String(parts[1]);
        GCHandle pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            var rw = SDL.SDL_RWFromMem(pinned.AddrOfPinnedObject(), bytes.Length);
            _surface = SDL_image.IMG_Load_RW(rw, 1);
            if (_surface == nint.Zero)
            {
                Console.WriteLine("IMG_Load_RW failed: " + SDL_image.IMG_GetError());
                return;
            }

            var surf = Marshal.PtrToStructure<SDL.SDL_Surface>(_surface);
            Width = surf.w;
            Height = surf.h;
            ImageData = bytes;
            Format = parts[0].Replace("data:", "");
            _member.Size = bytes.Length;
            _member.Width = Width;
            _member.Height = Height;
            IsLoaded = true;
        }
        finally
        {
            pinned.Free();
        }

    }
}
