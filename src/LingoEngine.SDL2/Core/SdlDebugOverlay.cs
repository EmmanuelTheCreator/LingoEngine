using LingoEngine.SDL2.SDLL;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.SDL2.Core;

internal class SdlDebugOverlay : ILingoFrameworkDebugOverlay
{
    private readonly nint _renderer;
    private nint _font;

    public SdlDebugOverlay(nint renderer)
    {
        _renderer = renderer;
        if (SDL_ttf.TTF_Init() != 0)
            throw new Exception($"TTF_Init failed: {SDL.SDL_GetError()}");
        string fullFileName = GEtFileName();
        _font = SDL_ttf.TTF_OpenFont(fullFileName, 12);
        if (_font == IntPtr.Zero)
            throw new Exception($"TTF_OpenFont failed: {SDL.SDL_GetError()}");
    }

    public void Begin() { }
    public void RenderLine(int x, int y, string text)
    {
        RenderText(text, x, y);
    }

    private static string GEtFileName()
    {
        return Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Fonts" + Path.DirectorySeparatorChar + "Tahoma.ttf";
    }

    public void RenderText(string text, int x, int y)
    {
        SDL.SDL_Color white = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };

        IntPtr surface = SDL_ttf.TTF_RenderUTF8_Blended(_font, text, white);
        if (surface == IntPtr.Zero)
            throw new Exception($"TTF_RenderUTF8_Blended failed: {SDL.SDL_GetError()}");

        IntPtr texture = SDL.SDL_CreateTextureFromSurface(_renderer, surface);
        if (texture == IntPtr.Zero)
            throw new Exception($"SDL_CreateTextureFromSurface failed: {SDL.SDL_GetError()}");

        SDL.SDL_QueryTexture(texture, out _, out _, out int w, out int h);
        SDL.SDL_Rect dstRect = new SDL.SDL_Rect { x = x, y = y, w = w, h = h };

        SDL.SDL_RenderCopy(_renderer, texture, IntPtr.Zero, ref dstRect);

        SDL.SDL_DestroyTexture(texture);
        SDL.SDL_FreeSurface(surface);
    }
    public void End() { }
    public void Dispose()
    {
        if (_font != IntPtr.Zero)
        {
            SDL_ttf.TTF_CloseFont(_font);
            _font = IntPtr.Zero;
        }

        SDL_ttf.TTF_Quit();
    }
}
