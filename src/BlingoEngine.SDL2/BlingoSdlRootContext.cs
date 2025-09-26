namespace BlingoEngine.SDL2;
using AbstUI.Inputs;
using AbstUI.SDL2;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Inputs;
using AbstUI.SDL2.SDLL;
using BlingoEngine.Core;
using BlingoEngine.Events;
using BlingoEngine.Inputs;
using BlingoEngine.Movies;
using BlingoEngine.SDL2.Core;
using BlingoEngine.SDL2.Inputs;
using BlingoEngine.SDL2.Stages;
using System;

public class BlingoSdlRootContext : AbstUISdlRootContext<BlingoMouse>
{
    private bool _f1Pressed;
    private BlingoSdlMouse _sdlMouse = null!;
    private BlingoPlayer _lPlayer = null!;
    private BlingoClock _clock = null!;
    public BlingoDebugOverlay DebugOverlay { get; set; } = null!;

    private Lazy<SdlStage> _stage;
    
    public BlingoSdlKey Key { get; set; }
    public IAbstFrameworkMouse Mouse { get; set; }
    internal BlingoSdlFactory BlingoFactory { get; set; } = null!;
    public BlingoSdlRootContext(nint window, nint renderer, SdlFocusManager focusManager, IAbstGlobalMouse globalMouse, IAbstGlobalKey globalKey)
        : base(window, renderer, focusManager)
    {
        GlobalMouse = globalMouse;
        GlobalKey = globalKey;

        _sdlMouse = new BlingoSdlMouse(new Lazy<AbstMouse<BlingoMouseEvent>>(() => (BlingoMouse)AbstMouse));
        Mouse = _sdlMouse;
        AbstMouse = new BlingoMouse(_sdlMouse);

        Key = new BlingoSdlKey();
        var blingoKey = new BlingoKey(Key);
        AbstKey = blingoKey;
        Key.SetKeyObj(blingoKey);
        _stage = new Lazy<SdlStage>(() => _lPlayer.Stage.Framework<SdlStage>());
    }

    public void Init(IBlingoPlayer player)
    {
        _lPlayer = (BlingoPlayer)player;
        _clock = (BlingoClock)_lPlayer.Clock;
        var overlay = new SdlDebugOverlay(BlingoFactory);
        ComponentContainer.Activate(overlay.ComponentContext);
        DebugOverlay = new BlingoDebugOverlay(overlay, _lPlayer);
       
    }

    protected override void HandleEvent(SDL.SDL_Event e, ref bool running)
    {
        Key.ProcessEvent(e);
        _sdlMouse.ProcessEvent(e);
        base.HandleEvent(e, ref running);
    }

    protected override void Update(float delta)
    {
        DebugOverlay.Update(delta);
        DebugOverlay.Render();
        bool f1 = _lPlayer.Key.KeyPressed((int)SDL.SDL_Keycode.SDLK_F1);
        if (f1 && !_f1Pressed)
            DebugOverlay.Toggle();
        _f1Pressed = f1;
        _clock.Tick(delta);

    }

    protected override void Render()
    {
        SDL.SDL_SetRenderTarget(Renderer, nint.Zero);
        SDL.SDL_SetRenderDrawColor(Renderer, 0, 0, 0, 255);
        //SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = 0, w = (int)_stage.Value.Width, h = (int)_stage .Value.Height};
        SDL.SDL_RenderFillRect(Renderer, IntPtr.Zero);
        var stageTexture = _stage.Value.LastTexture;

        SDL.SDL_RenderCopy(Renderer, stageTexture, IntPtr.Zero, IntPtr.Zero);
        ComponentContainer.Render(Factory.CreateRenderContext(null, System.Numerics.Vector2.Zero));
        SDL.SDL_RenderPresent(Renderer);
        
    }
}

