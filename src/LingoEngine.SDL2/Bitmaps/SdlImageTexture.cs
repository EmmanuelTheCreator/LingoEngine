using LingoEngine.Bitmaps;
using LingoEngine.SDL2.SDLL;

namespace LingoEngine.SDL2.Pictures;

public class SdlImageTexture : ILingoImageTexture
{
    private SDL.SDL_Surface _surfacePtr;
    public SDL.SDL_Surface Ptr => _surfacePtr;
    public int Width { get; set; }

    public int Height { get; set; }

    public SdlImageTexture(SDL.SDL_Surface surfacePtr, int width, int height)
    {
        _surfacePtr = surfacePtr;
        Width = width;
        Height = height;
    }
}
