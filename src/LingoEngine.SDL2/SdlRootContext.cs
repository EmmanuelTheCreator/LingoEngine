namespace LingoEngineSDL2;

using SDL2;

public class SdlRootContext : IDisposable
{
    public IntPtr Window { get; }
    public IntPtr Renderer { get; }

    public SdlRootContext(IntPtr window, IntPtr renderer)
    {
        Window = window;
        Renderer = renderer;
    }

    public void Dispose()
    {
        if (Renderer != IntPtr.Zero)
        {
            SDL.SDL_DestroyRenderer(Renderer);
        }
        if (Window != IntPtr.Zero)
        {
            SDL.SDL_DestroyWindow(Window);
        }
        SDL_image.IMG_Quit();
        SDL.SDL_Quit();
    }
}
