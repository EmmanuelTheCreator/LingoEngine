using LingoEngine.SDL2.SDLL;
using System.Runtime.InteropServices;

namespace LingoEngine.SDL2;

internal static class SdlClipboard
{
    public static void SetText(string text)
    {
        SDL.SDL_SetClipboardText(text);
    }

    public static string GetText()
    {
        // todo
        //if (SDL.SDL_HasClipboardText() == SDL.SDL_bool.SDL_TRUE)
        //{
        //    var ptr = SDL.SDL_GetClipboardText();
        //    var text = Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
        //    SDL.SDL_free(ptr);
        //    return text;
        //}
        return string.Empty;
    }
}
