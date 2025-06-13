namespace LingoEngine.SDL2;
using LingoEngine.Core;
using LingoEngine.SDL2.SDLL;

public class SdlRootContext : IDisposable
{
    public nint Window { get; }
    public nint Renderer { get; }

    public SdlRootContext(nint window, nint renderer)
    {
        Window = window;
        Renderer = renderer;
    }
    public void Run(ILingoPlayer player)
    {
        var clock = (LingoClock)((LingoPlayer)player).Clock;
        bool running = true;
        uint last = SDL.SDL_GetTicks();
        while (running)
        {
            while (SDL.SDL_PollEvent(out var e) == 1)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    running = false;
            }
            uint now = SDL.SDL_GetTicks();
            float delta = (now - last) / 1000f;
            last = now;
            clock.Tick(delta);
        }
        Dispose();
    }

    public void Dispose()
    {
        if (Renderer != nint.Zero)
        {
            SDL.SDL_DestroyRenderer(Renderer);
        }
        if (Window != nint.Zero)
        {
            SDL.SDL_DestroyWindow(Window);
        }
        SDL_image.IMG_Quit();
        SDL.SDL_Quit();
    }
}
