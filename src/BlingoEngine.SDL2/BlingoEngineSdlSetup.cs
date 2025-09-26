using AbstUI;
using AbstUI.Core;
using AbstUI.Inputs;
using AbstUI.SDL2;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.SDLL;
using BlingoEngine.Core;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.SDL2.Core;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using static AbstUI.SDL2.SDLL.SDL;


namespace BlingoEngine.SDL2;

public static class SdlSetup
{
    private static bool _engineRegistered = false;
    public static IBlingoEngineRegistration WithBlingoSdlEngine(this IBlingoEngineRegistration reg, string windowTitle, int width, int height, Action<BlingoSdlFactory>? setup = null, Action<IAbstFameworkComponentWinRegistrator>? componentRegistrations = null, float windowScale = 1)
    {
        if (_engineRegistered) return reg; // only register once
        _engineRegistered = true;
        BlingoEngineGlobal.RunFramework = AbstEngineRunFramework.SDL2;
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
        SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "0");
        SDL.SDL_RenderSetLogicalSize(renderer, width, height); // Virtual resolution
        //SDL.SDL_RenderSetIntegerScale(renderer, SDL_bool.SDL_TRUE);
        SDL.SDL_SetWindowSize(window, (int)(width * windowScale), (int)(height * windowScale));
        return reg.WithBlingoSdlEngine(window, renderer, setup, componentRegistrations);
    }
    public static void Dispose()
    {
        SDL_ttf.TTF_Quit();
        SDL.SDL_AudioQuit();
        SDL.SDL_VideoQuit();
        SDL.SDL_Quit();
    }
    public static IBlingoEngineRegistration WithBlingoSdlEngine(this IBlingoEngineRegistration reg, nint sdlWindow, nint sdlRenderer, Action<BlingoSdlFactory>? setup = null, Action<IAbstFameworkComponentWinRegistrator>? componentRegistrations = null)
    {
        BlingoEngineGlobal.RunFramework = AbstEngineRunFramework.SDL2;
        RegisterServices(reg, setup, sdlWindow, sdlRenderer, componentRegistrations);
        return reg;
    }

    private static void RegisterServices(IBlingoEngineRegistration reg, Action<BlingoSdlFactory>? setup, nint sdlWindow, nint sdlRenderer, Action<IAbstFameworkComponentWinRegistrator>? componentRegistrations = null)
    {
        reg
            .ServicesMain(s => s
                    .WithAbstUISdl()
                    .AddSingleton<BlingoSdlRootContext>(provider =>
                        new BlingoSdlRootContext(
                            sdlWindow,
                            sdlRenderer,
                            provider.GetRequiredService<SdlFocusManager>(),
                            provider.GetRequiredService<IAbstGlobalMouse>(),
                            provider.GetRequiredService<IAbstGlobalKey>()))
                    .AddSingleton<ISdlRootComponentContext>(p => p.GetRequiredService<BlingoSdlRootContext>())
                    .AddSingleton<IAbstSDLRootContext>(p => p.GetRequiredService<BlingoSdlRootContext>())
                    .AddSingleton<IBlingoFrameworkFactory, BlingoSdlFactory>()
                )
            .WithFrameworkFactory(setup)
            .AddPreBuildAction(x => x.WithAbstUISdl())
            ;
    }
}

