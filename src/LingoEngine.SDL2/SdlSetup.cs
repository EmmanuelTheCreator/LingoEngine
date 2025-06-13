using LingoEngine;
using LingoEngine.FrameworkCommunication;
using LingoEngine.FrameworkCommunication.Events;
using LingoEngineSDL2.Core;
using Microsoft.Extensions.DependencyInjection;
using SDL2;

namespace LingoEngineSDL2;

public static class SdlSetup
{

public static ILingoEngineRegistration WithLingoSdlEngine(this ILingoEngineRegistration reg, string windowTitle, int width, int height, Action<SdlFactory>? setup = null)
    {
        SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO);
        SDL_image.IMG_Init((int)SDL_image.IMG_InitFlags.IMG_INIT_PNG);
        var window = SDL.SDL_CreateWindow(windowTitle, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, width, height, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
        var renderer = SDL.SDL_CreateRenderer(window, -1, (int)SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
        var rootContext = new SdlRootContext(window, renderer);
        reg.Services(s => s
            .AddSingleton<ILingoFrameworkFactory, SdlFactory>()
            .AddSingleton<ILingoFontManager, SdlFontManager>()
            .AddSingleton(rootContext)
        ).WithFrameworkFactory(setup);
        return reg;
    }
}
