using LingoEngine;
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
        SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO);
        SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG);
        var window = SDL.SDL_CreateWindow(windowTitle, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, width, height, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
        var renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
        return reg.WithLingoSdlEngine(window, renderer, setup);
    }
    public static ILingoEngineRegistration WithLingoSdlEngine(this ILingoEngineRegistration reg, nint window, nint renderer, Action<SdlFactory>? setup = null)
    {
        var rootContext = new SdlRootContext(window, renderer);
        RegisterServices(reg, setup, rootContext);
        return reg;
    }

    private static void RegisterServices(ILingoEngineRegistration reg, Action<SdlFactory>? setup, SdlRootContext rootContext)
    {
        reg.Services(s => s
                    .AddSingleton<ILingoFrameworkFactory, SdlFactory>()
                    .AddSingleton<ILingoFontManager, SdlFontManager>()
                    .AddSingleton(rootContext)
                ).WithFrameworkFactory(setup);
    }
}
