using LingoEngine.Core;
using Microsoft.Extensions.DependencyInjection;
using SDL2;

namespace LingoEngineSDL2;

public static class SdlGameHost
{
    public static void Run(IServiceProvider services, LingoClock clock)
    {
        var context = services.GetRequiredService<SdlRootContext>();
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
        context.Dispose();
    }
}
