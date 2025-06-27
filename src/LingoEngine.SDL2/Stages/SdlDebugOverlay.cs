using LingoEngine.SDL2.SDLL;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.SDL2.Stages;

internal class SdlDebugOverlay : ILingoFrameworkDebugOverlay
{
    private readonly nint _renderer;
    private nint _font;
    private bool _show;
    private SDL.SDL_Color _white;

    public SdlDebugOverlay(nint renderer)
    {
        _renderer = renderer;

        string fullFileName = GetFileName();
        _font = SDL_ttf.TTF_OpenFont(fullFileName, 12);
        if (_font == nint.Zero)
            throw new Exception($"TTF_OpenFont failed: {SDL.SDL_GetError()}");
        _white = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };
    }

    public void Begin() { }

    public void ShowDebugger()
    {
        _show = true;
    }

    public void HideDebugger()
    {
        _show = false;
    }

    public int PrepareLine(int id, string text)
    {
        return id;
    }


    public void SetLineText(int id, string text)
    {
        nint surface = SDL_ttf.TTF_RenderUTF8_Blended(_font, text, _white);
        if (surface == nint.Zero)
            throw new Exception($"TTF_RenderUTF8_Blended failed: {SDL.SDL_GetError()}");

        nint texture = SDL.SDL_CreateTextureFromSurface(_renderer, surface);
        if (texture == nint.Zero)
            throw new Exception($"SDL_CreateTextureFromSurface failed: {SDL.SDL_GetError()}");

        SDL.SDL_QueryTexture(texture, out _, out _, out int w, out int h);
        SDL.SDL_Rect dstRect = new SDL.SDL_Rect { x = 20, y = id * 15, w = w, h = h };

        SDL.SDL_RenderCopy(_renderer, texture, nint.Zero, ref dstRect);

        SDL.SDL_DestroyTexture(texture);
        SDL.SDL_FreeSurface(surface);
    }


    private static string GetFileName() => Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Fonts" + Path.DirectorySeparatorChar + "Tahoma.ttf";


    public void End() { }
    public void Dispose()
    {
        if (_font != nint.Zero)
        {
            SDL_ttf.TTF_CloseFont(_font);
            _font = nint.Zero;
        }


    }


}
