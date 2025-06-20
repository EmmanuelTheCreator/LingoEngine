using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.SDL2.Core;
using LingoEngine.SDL2.SDLL;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.SDL2;

public static class SdlSetup
{

    public static ILingoEngineRegistration WithLingoSdlEngine(this ILingoEngineRegistration reg, string windowTitle, int width, int height, Action<SdlFactory>? setup = null)
    {
        if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_EVENTS | SDL.SDL_INIT_GAMECONTROLLER | SDL.SDL_INIT_AUDIO) < 0)
        {
            Console.WriteLine("Unable to initialize SDL. Error: {0}", SDL.SDL_GetError());
            return reg;
        }
        if (SDL_ttf.TTF_Init() != 0)
        {
            Console.WriteLine($"TTF_Init failed: {SDL.SDL_GetError()}");
            return reg;
        }

        SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG);
        var window = SDL.SDL_CreateWindow(windowTitle, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, width, height, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN |
                                   SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        if (window == IntPtr.Zero)
        {
            Console.WriteLine("Unable to create a window. SDL. Error: {0}", SDL.SDL_GetError());
            return reg;
        }
        var renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
        SDL.SDL_RenderSetLogicalSize(renderer, width, height); // Virtual resolution
        //SDL.SDL_SetWindowResizable(window, SDL.SDL_bool.SDL_TRUE);
        return reg.WithLingoSdlEngine(window, renderer, setup);
    }
    public static void Dispose()
    {
        SDL_ttf.TTF_Quit();
        SDL.SDL_AudioQuit();
        SDL.SDL_VideoQuit();
        SDL.SDL_Quit();
    }
    public static ILingoEngineRegistration WithLingoSdlEngine(this ILingoEngineRegistration reg, nint sdlWindow, nint sdlRenderer, Action<SdlFactory>? setup = null)
    {
        var rootContext = new SdlRootContext(sdlWindow, sdlRenderer);
        RegisterServices(reg, setup, rootContext);
        return reg;
    }

    private static void RegisterServices(ILingoEngineRegistration reg, Action<SdlFactory>? setup, SdlRootContext rootContext)
    {
        reg
            .Services(s => s
                    .AddSingleton<ILingoFrameworkFactory, SdlFactory>()
                    .AddSingleton<ILingoFontManager, SdlFontManager>()
                    .AddSingleton(rootContext)
                ).WithFrameworkFactory(setup);
    }
}
