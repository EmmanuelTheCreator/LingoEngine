namespace LingoEngine.SDL2;
using LingoEngine.Core;
using LingoEngine.SDL2.SDLL;
using LingoEngine.SDL2.Core;
using LingoEngine.Movies;

public class SdlRootContext : IDisposable
{
    public nint Window { get; }
    public nint Renderer { get; }

    private LingoPlayer _lPlayer;

    public LingoDebugOverlay DebugOverlay { get; set; }
    internal SdlKey Key { get; } = new();
    private bool _f1Pressed;

    public SdlRootContext(nint window, nint renderer)
    {
        Window = window;
        Renderer = renderer;
        
    }
    public void Init(ILingoPlayer player)
    {
        _lPlayer = (LingoPlayer)player;
        DebugOverlay = new LingoDebugOverlay(new SdlDebugOverlay(Renderer), _lPlayer);
    }
    public void Run()
    {
        var clock = (LingoClock)_lPlayer.Clock;
       
        bool running = true;
        uint last = SDL.SDL_GetTicks();
        while (running)
        {
            while (SDL.SDL_PollEvent(out var e) == 1)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    running = false;
                Key.ProcessEvent(e);
            }
            uint now = SDL.SDL_GetTicks();
            float delta = (now - last) / 1000f;
            last = now;
            DebugOverlay.Update(delta);
            bool f1 = _lPlayer.Key.KeyPressed((int)SDL.SDL_Keycode.SDLK_F1);
            if ( f1 && !_f1Pressed)
                DebugOverlay.Toggle();
            _f1Pressed = f1;
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
