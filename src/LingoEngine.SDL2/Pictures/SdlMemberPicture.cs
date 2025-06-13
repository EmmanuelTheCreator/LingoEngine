using System.Runtime.InteropServices;
using LingoEngine.Pictures;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.Tools;
using SDL2;

namespace LingoEngineSDL2.Pictures;

public class SdlMemberPicture : ILingoFrameworkMemberPicture, IDisposable
{
    private LingoMemberPicture _member = null!;
    private IntPtr _surface = IntPtr.Zero;
    public byte[]? ImageData { get; private set; }
    public bool IsLoaded { get; private set; }
    public string Format { get; private set; } = "image/unknown";
    public int Width { get; private set; }
    public int Height { get; private set; }

    internal void Init(LingoMemberPicture member)
    {
        _member = member;
    }

    public void Preload()
    {
        if (IsLoaded)
            return;
        if (!File.Exists(_member.FileName))
            return;

        _surface = SDL_image.IMG_Load(_member.FileName);
        if (_surface == IntPtr.Zero)
            return;

        var surf = Marshal.PtrToStructure<SDL.SDL_Surface>(_surface);
        Width = surf.w;
        Height = surf.h;

        ImageData = File.ReadAllBytes(_member.FileName);
        Format = MimeHelper.GetMimeType(_member.FileName);
        _member.Size = ImageData.Length;
        _member.Width = Width;
        _member.Height = Height;
        IsLoaded = true;
    }

    public void Unload()
    {
        if (_surface != IntPtr.Zero)
        {
            SDL.SDL_FreeSurface(_surface);
            _surface = IntPtr.Zero;
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
            if (_surface == IntPtr.Zero)
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
