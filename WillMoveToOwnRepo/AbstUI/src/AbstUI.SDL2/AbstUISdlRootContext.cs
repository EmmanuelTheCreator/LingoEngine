using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Inputs;
using AbstUI.SDL2.SDLL;
using System;

namespace AbstUI.SDL2;

public abstract class AbstUISdlRootContext<TMouse> : IAbstSDLRootContext, ISdlRootComponentContext, IDisposable
    where TMouse : IAbstMouse
{
    private IAbstGlobalMouse _globalMouse = null!;
    private IAbstGlobalKey _globalKey = null!;
    protected AbstSdlGlobalMouse<GlobalSDLAbstMouse, AbstMouseEvent>? _frameworkMouse;
    protected SdlKey? _frameworkKey;
    public int WindowScale { get; set; } = 1;
    public nint Window { get; }
    public nint Renderer { get; }

    public IAbstGlobalMouse GlobalMouse
    {
        get => _globalMouse;
        set
        {
            _globalMouse = value;
            _frameworkMouse = ((GlobalSDLAbstMouse)_globalMouse).Framework<AbstSdlGlobalMouse<GlobalSDLAbstMouse, AbstMouseEvent>>();
        }
    }

    public IAbstGlobalKey GlobalKey
    {
        get => _globalKey;
        set
        {
            _globalKey = value;
            _frameworkKey = ((GlobalSDLAbstKey)_globalKey).Framework;
        }
    }

    public SdlFocusManager FocusManager { get; }
    public AbstSDLComponentContainer ComponentContainer { get; }

    protected SdlDispatcher _dispatcher;

    public AbstSdlComponentFactory Factory { get; set; } = null!;

    public TMouse AbstMouse { get; protected set; } = default!;
    IAbstMouse ISdlRootComponentContext.AbstMouse => AbstMouse;
    public IAbstKey AbstKey { get; protected set; } = null!;

    public AbstUISdlRootContext(nint window, nint renderer, SdlFocusManager focusManager)
    {
        Window = window;
        Renderer = renderer;
        FocusManager = focusManager;
        ComponentContainer = new AbstSDLComponentContainer(focusManager);
        _dispatcher = new SdlDispatcher();
        var ctx = new SdlSynchronizationContext(_dispatcher);
        SynchronizationContext.SetSynchronizationContext(ctx);
    }

    public virtual void Run()
    {
        bool running = true;
        
        uint last = SDL.SDL_GetTicks();
        while (running)
        {
            while (SDL.SDL_PollEvent(out var e) == 1)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    running = false;
                else if (e.type == SDL.SDL_EventType.SDL_WINDOWEVENT &&
                         (e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED ||
                          e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED))
                {
                    ComponentContainer.QueueRedrawAll();
                }
                _dispatcher.Pump(e);    

                _frameworkKey?.ProcessEvent(e);
                _frameworkMouse?.ProcessEvent(e);
                ComponentContainer.HandleEvent(e);
                HandleEvent(e, ref running);
            }

            uint now = SDL.SDL_GetTicks();
            float delta = (now - last) / 1000f;
            last = now;

            Update(delta);
            Render();
        }

        Dispose();
    }

    protected virtual void HandleEvent(SDL.SDL_Event e, ref bool running) { }

    protected virtual void Update(float delta) { }

    protected virtual void Render()
    {
        //SDL.SDL_SetRenderDrawColor(Renderer, 255, 0, 0, 255);
        //SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 100, y = 100, w = 200, h = 150 };
        //SDL.SDL_RenderFillRect(Renderer, ref rect);
      
        ComponentContainer.Render(Factory.CreateRenderContext(null, System.Numerics.Vector2.Zero));
        SDL.SDL_RenderPresent(Renderer);
    }
    

    public virtual void Dispose()
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

    public APoint GetWindowSize()
    {
        SDL.SDL_GetWindowSize(Window, out var w, out var h);
        return new APoint(w, h);
    }
}
