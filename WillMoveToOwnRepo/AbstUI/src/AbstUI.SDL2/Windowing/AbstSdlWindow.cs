using System;
using System.Runtime.InteropServices;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.Core;
using AbstUI.Components;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Containers;
using static AbstUI.SDL2.SDLL.SDL;

namespace AbstUI.SDL2.Windowing;

public class AbstSdlWindow : AbstSdlPanel, IAbstFrameworkWindow, IHandleSdlEvent, IDisposable
{
    protected readonly AbstSdlComponentFactory _componentFactory;
    private IAbstWindowInternal _abstWindow = null!;
    private string _title = string.Empty;
    private bool _isPopup;
    private bool _borderless;

    private ISdlFontLoadedByUser? _font;
    private SDL.SDL_Rect _closeRect;
    private bool _dragging;
    private int _dragOffsetX;
    private int _dragOffsetY;
    internal const int _titleBarHeight = 24;



    public string Title
    {
        get => _title;
        set => _title = value;
    }

    public new float Width
    {
        get => base.Width;
        set
        {
            if (Math.Abs(base.Width - value) > float.Epsilon)
                base.Width = value;
        }
    }

    public new float Height
    {
        get => base.Height;
        set
        {
            if (Math.Abs(base.Height - value) > float.Epsilon)
                base.Height = value;
        }
    }

    public bool IsPopup
    {
        get => _isPopup;
        set => _isPopup = value;
    }

    public bool Borderless
    {
        get => _borderless;
        set => _borderless = value;
    }

    public string WindowCode => _abstWindow.WindowCode;
    public IAbstWindowInternal AWindow => _abstWindow;

    public AColor BackgroundTitleColor { get; set; } 
    public new AColor BackgroundColor
    {
        get => base.BackgroundColor ?? AColors.White;
        set => base.BackgroundColor = value;
    }

    public bool IsOpen => Visibility;

    public bool IsActiveWindow => _abstWindow.IsActivated;

    public IAbstMouse Mouse => _abstWindow.Mouse;

    public IAbstKey AbstKey => _abstWindow.Key;

    private IAbstFrameworkNode? _content;
    public IAbstFrameworkNode? Content
    {
        get => _content;
        set
        {
            if (_content == value) return;
            RemoveAll();
            _content = value;
            if (value is IAbstFrameworkLayoutNode layout)
                AddItem(layout);
            _abstWindow?.SetContentFromFW(value);
        }
    }

    public AbstSdlWindow(AbstSdlComponentFactory factory) : base(factory)
    {
        _componentFactory = factory;
        ClipChildren = true;
        //var mouse = ((IAbstMouseInternal)factory.RootContext.AbstMouse).CreateNewInstance(window);
        //var key = ((AbstKey)factory.RootContext.AbstKey).CreateNewInstance(window);
        //_abstWindow.Init(this, mouse, key);
        Visibility = false;
        BackgroundColor = AColors.White;
        BackgroundTitleColor = AColors.LightGray;
    }

    public virtual void Init(IAbstWindow instance)
    {
        if (instance == _abstWindow) return;
        _abstWindow = (IAbstWindowInternal)instance;
        _abstWindow.Init(this);
        instance.WindowTitleHeight = _titleBarHeight;
        //OnResize(true, (int)Width, (int)Height);
    }

    // TODO :  Resize SDL window.
    private void Resize(bool firstResize, int width, int height)
    {
        OnResize(firstResize, width, height - _titleBarHeight);
        _abstWindow.ResizingContentFromFW(false, width, height - _titleBarHeight);
        // updates sizes because it could be resized to minimum size
        UpateSizeFromAbstWindow();
        ComponentContext.QueueRedraw(this);
    }
    protected virtual void OnResize(bool firstResize, int width, int height)
    {
       
    }


    internal virtual void BringToFront()
        => _componentFactory.RootContext.ComponentContainer.Activate(ComponentContext);

    public virtual void OpenWindow()
    {
        BringToFront();
       
        Visibility = true;
        X = _abstWindow.X;
        Y = _abstWindow.Y;
        _abstWindow.SetPositionFromFW((int)X, (int)Y);
        // updates sizes because it could be resized to minimum size
        Resize(true, (int)Width, (int)Height);
        _abstWindow.RaiseWindowStateChanged(true);
    }

    private void UpateSizeFromAbstWindow()
    {
        Width = ((IAbstWindow)_abstWindow).Width;
        Height = ((IAbstWindow)_abstWindow).Height;
    }

    public virtual void CloseWindow()
    {
        Visibility = false;
        _componentFactory.RootContext.ComponentContainer.Deactivate(ComponentContext);
        _abstWindow.RaiseWindowStateChanged(false);
    }
    public virtual void MoveWindow(int x, int y)
    {
        X = x;
        Y = y;
        _abstWindow.SetPositionFromFW(x, y);
    }

    public virtual void SetPositionAndSize(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        _abstWindow.SetPositionFromFW(x, y);
        SetSize(width, height);
    }

    public virtual APoint GetPosition() => new APoint(X, Y);


    public virtual APoint GetSize() => new APoint(Width, Height);

    public virtual void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
        _abstWindow.ResizingContentFromFW(false, width, height);
    }

    public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        if (!Visibility)
            return default;

        ClipChildren = true;
        int w = (int)Width;
        int h = (int)Height;

        if (_font == null)
            _font = context.SdlFontManager.GetTyped(this, null, 14);

        var prev = SDL.SDL_GetRenderTarget(context.Renderer);

        // Render children
        _xOffset = (int)X;
        _yOffset = (int)(_titleBarHeight + Y);
        // render children first to a texture
        //Console.WriteLine($"WIN off=({(int)X},{(int)Y}) content={Content?.Name}");
        var tex = (nint)base.Render(context);

        SDL.SDL_SetRenderTarget(context.Renderer, tex);

        // Title bg
        SDL.SDL_SetRenderDrawColor(context.Renderer, BackgroundTitleColor.R, BackgroundTitleColor.G, BackgroundTitleColor.B, BackgroundTitleColor.A);
        var bar = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = _titleBarHeight };
        SDL.SDL_RenderFillRect(context.Renderer, ref bar);

        if (!string.IsNullOrEmpty(_title))
        {
            SDL.SDL_Color col = new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 };
            nint surf = SDL_ttf.TTF_RenderUTF8_Blended(_font!.FontHandle, _title, col);
            if (surf != nint.Zero)
            {
                var s = Marshal.PtrToStructure<SDL.SDL_Surface>(surf);
                nint t = SDL.SDL_CreateTextureFromSurface(context.Renderer, surf);
                SDL.SDL_FreeSurface(surf);
                var dst = new SDL.SDL_Rect
                {
                    x = 4,
                    y = (_titleBarHeight - s.h) / 2,
                    w = s.w,
                    h = s.h
                };
                SDL.SDL_RenderCopy(context.Renderer, t, nint.Zero, ref dst);
                SDL.SDL_DestroyTexture(t);
            }
        }

        int btnSize = _titleBarHeight - 4;
        _closeRect = new SDL.SDL_Rect { x = w - btnSize - 2, y = 2, w = btnSize, h = btnSize };
        SDL.SDL_SetRenderDrawColor(context.Renderer, 180, 0, 0, 255);
        SDL.SDL_RenderFillRect(context.Renderer, ref _closeRect);
        SDL.SDL_SetRenderDrawColor(context.Renderer, 255, 255, 255, 255);
        SDL.SDL_RenderDrawLine(context.Renderer, _closeRect.x + 3, _closeRect.y + 3,
            _closeRect.x + _closeRect.w - 3, _closeRect.y + _closeRect.h - 3);
        SDL.SDL_RenderDrawLine(context.Renderer, _closeRect.x + _closeRect.w - 3, _closeRect.y + 3,
            _closeRect.x + 3, _closeRect.y + _closeRect.h - 3);

        SDL.SDL_SetRenderTarget(context.Renderer, prev);

        return tex;
    }

    public override void HandleEvent(AbstSDLEvent e)
    {
        if (!Visibility)
            return;

        switch (e.Event.type)
        {
            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                int lx = e.Event.button.x - (int)X;
                int ly = e.Event.button.y - (int)Y;
                //Console.WriteLine($"Window {WindowCode} mouse down at {lx},{ly}");
                _componentFactory.GetRequiredService<IAbstWindowManager>().SetActiveWindow(WindowCode);

                if (lx >= _closeRect.x && lx <= _closeRect.x + _closeRect.w &&
                    ly >= _closeRect.y && ly <= _closeRect.y + _closeRect.h)
                {
                    CloseWindow();
                    e.StopPropagation = true;
                    return;
                }

                if (ly <= _titleBarHeight)
                {
                    _dragging = true;
                    _dragOffsetX = lx;
                    _dragOffsetY = ly;
                    e.StopPropagation = true;
                }
                break;

            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                _dragging = false;
                break;

            case SDL.SDL_EventType.SDL_MOUSEMOTION:
                if (_dragging)
                {
                    X = e.Event.motion.x - _dragOffsetX;
                    Y = e.Event.motion.y - _dragOffsetY;
                    _abstWindow.SetPositionFromFW((int)X, (int)Y);
                    e.StopPropagation = true;
                }
                break;
        }
        if (!e.StopPropagation)
        {
            //e.OffsetX = -(int)X; // - _xOffset;
            //e.OffsetY = -(int)Y; // - _yOffset;
#if DEBUG
            if (e.Event.type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
            {

            }
#endif
            ContainerHelpers.HandleChildEvents(_children, e, X-Margin.Left, Y - Margin.Top+_titleBarHeight);
            //_xOffset = -(int)X;
            //_yOffset = -(int)(Y + TitleBarHeight);
            //base.HandleEvent(e);
        }
    }

    public override void Dispose()
    {
        _font?.Release();
        base.Dispose();
    }
}
