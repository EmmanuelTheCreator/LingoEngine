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
    private bool _resizing;
    private int _resizeStartMouseX;
    private int _resizeStartMouseY;
    private int _resizeStartWidth;
    private int _resizeStartHeight;
    private bool _cursorInResizeArea;
    private readonly IAbstMouse<AbstMouseEvent>? _globalMouse;
    private IAbstMouseSubscription? _globalMouseMoveSubscription;
    private IAbstMouseSubscription? _globalMouseUpSubscription;
    internal const int _titleBarHeight = 24;
    private const int _resizeHandleSize = 16;



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
    public int TitleBarHeight => Borderless?0: _titleBarHeight;

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

        _globalMouse = _componentFactory.GetRequiredService<IAbstGlobalMouse>() as IAbstMouse<AbstMouseEvent>;
        if (_globalMouse != null)
        {
            _globalMouseMoveSubscription = _globalMouse.OnMouseMove(OnGlobalMouseMove);
            _globalMouseUpSubscription = _globalMouse.OnMouseUp(OnGlobalMouseUp);
        }
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
        var titleOffset = Borderless ? 0 : _titleBarHeight;
        Height = ((IAbstWindow)_abstWindow).Height + titleOffset;
    }

    public virtual void CloseWindow()
    {
        if (_resizing || _cursorInResizeArea)
            ResetCursor();
        _resizing = false;
        _dragging = false;
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


    public virtual APoint GetSize()
    {
        var totalHeight = Height;
        var contentHeight = Borderless ? totalHeight : Math.Max(0, totalHeight - _titleBarHeight);
        return new APoint(Width, contentHeight);
    }

    public virtual void SetSize(int width, int height)
    {
        Width = width;
        var titleOffset = Borderless ? 0 : _titleBarHeight;
        Height = height + titleOffset;
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
        _yOffset = (int)((Borderless? 0: _titleBarHeight) + Y);
        // render children first to a texture
        //Console.WriteLine($"WIN off=({(int)X},{(int)Y}) content={Content?.Name}");
        var tex = (nint)base.Render(context);


        if (!Borderless)
        {
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
        }

        return tex;
    }

    private int GetTotalWidth()
    {
        var storedWidth = (int)MathF.Round(Width);
        if (_abstWindow == null)
            return storedWidth;

        var abstWindow = (IAbstWindow)_abstWindow;
        return Math.Max(storedWidth, abstWindow.Width);
    }

    private int GetTotalHeight()
    {
        var storedHeight = (int)MathF.Round(Height);
        if (_abstWindow == null)
            return storedHeight;

        var abstWindow = (IAbstWindow)_abstWindow;
        var titleOffset = Borderless ? 0 : _titleBarHeight;
        return Math.Max(storedHeight, abstWindow.Height + titleOffset);
    }

    private int GetMinimumTotalHeight()
    {
        if (_abstWindow == null)
            return Borderless ? 0 : _titleBarHeight;

        var abstWindow = (IAbstWindow)_abstWindow;
        var titleOffset = Borderless ? 0 : _titleBarHeight;
        return abstWindow.MinimumHeight + titleOffset;
    }

    private bool IsInResizeHandle(int localX, int localY)
    {
        var totalWidth = GetTotalWidth();
        var totalHeight = GetTotalHeight();
        return localX >= totalWidth - _resizeHandleSize && localY >= totalHeight - _resizeHandleSize;
    }

    private void BeginResize(int mouseX, int mouseY)
    {
        _resizing = true;
        _resizeStartMouseX = mouseX;
        _resizeStartMouseY = mouseY;
        _resizeStartWidth = GetTotalWidth();
        _resizeStartHeight = GetTotalHeight();
        SetResizeCursor();
    }

    private void UpdateResize(int mouseX, int mouseY)
    {
        if (!_resizing)
            return;

        var deltaX = mouseX - _resizeStartMouseX;
        var deltaY = mouseY - _resizeStartMouseY;
        var targetWidth = _resizeStartWidth + deltaX;
        var targetHeight = _resizeStartHeight + deltaY;

        ApplyResize(targetWidth, targetHeight);
    }

    private void ApplyResize(int targetWidth, int targetHeight)
    {
        if (_abstWindow == null)
            return;

        var abstWindow = (IAbstWindow)_abstWindow;
        var minWidth = abstWindow.MinimumWidth;
        var minTotalHeight = GetMinimumTotalHeight();

        if (targetWidth < minWidth)
            targetWidth = minWidth;
        if (targetHeight < minTotalHeight)
            targetHeight = minTotalHeight;

        var titleOffset = Borderless ? 0 : _titleBarHeight;
        var contentHeight = targetHeight - titleOffset;
        if (contentHeight < abstWindow.MinimumHeight)
        {
            contentHeight = abstWindow.MinimumHeight;
            targetHeight = contentHeight + titleOffset;
        }

        _abstWindow.ResizingContentFromFW(false, targetWidth, contentHeight);

        Width = abstWindow.Width;
        Height = abstWindow.Height + titleOffset;
        ComponentContext.QueueRedraw(this);
    }

    private void UpdateWindowPosition(int mouseX, int mouseY)
    {
        X = mouseX - _dragOffsetX;
        Y = mouseY - _dragOffsetY;
        _abstWindow.SetPositionFromFW((int)X, (int)Y);
    }

    private void UpdateCursor(bool overResizeHandle)
    {
        if (_globalMouse == null || _resizing)
            return;

        if (overResizeHandle && !_cursorInResizeArea)
        {
            _cursorInResizeArea = true;
            _globalMouse.SetCursor(AMouseCursor.SizeNWSE);
        }
        else if (!overResizeHandle && _cursorInResizeArea)
        {
            _cursorInResizeArea = false;
            _globalMouse.SetCursor(AMouseCursor.Arrow);
        }
    }

    private void SetResizeCursor()
    {
        if (_globalMouse == null)
            return;

        _cursorInResizeArea = true;
        _globalMouse.SetCursor(AMouseCursor.SizeNWSE);
    }

    private void ResetCursor()
    {
        if (_globalMouse == null)
            return;

        _cursorInResizeArea = false;
        _globalMouse.SetCursor(AMouseCursor.Arrow);
    }

    private void OnGlobalMouseMove(AbstMouseEvent e)
    {
        if (Borderless) return;
        if (_dragging)
            UpdateWindowPosition((int)e.MouseH, (int)e.MouseV);

        if (_resizing)
            UpdateResize((int)e.MouseH, (int)e.MouseV);
    }

    private void OnGlobalMouseUp(AbstMouseEvent e)
    {
        if (Borderless) return;
        if (!_dragging && !_resizing)
            return;

        _dragging = false;
        if (_resizing)
        {
            _resizing = false;
            ResetCursor();
        }
        else if (!_cursorInResizeArea)
        {
            ResetCursor();
        }
    }

    public override void HandleEvent(AbstSDLEvent e)
    {
        if (!Visibility)
            return;
        if (Borderless)
            ContainerHelpers.HandleChildEvents(_children, e, X - Margin.Left, Y - Margin.Top );
        else 
        { 
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

                    if (IsInResizeHandle(lx, ly))
                    {
                        BeginResize(e.Event.button.x, e.Event.button.y);
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
                    if (_resizing)
                    {
                        _resizing = false;
                        ResetCursor();
                    }
                    _dragging = false;
                    break;

                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    {
                        int motionLx = e.Event.motion.x - (int)X;
                        int motionLy = e.Event.motion.y - (int)Y;
                        UpdateCursor(IsInResizeHandle(motionLx, motionLy));

                        if (_dragging)
                        {
                            UpdateWindowPosition(e.Event.motion.x, e.Event.motion.y);
                            e.StopPropagation = true;
                        }
                        else if (_resizing)
                        {
                            UpdateResize(e.Event.motion.x, e.Event.motion.y);
                            e.StopPropagation = true;
                        }
                        break;
                    }
            }
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
        _globalMouseMoveSubscription?.Release();
        _globalMouseUpSubscription?.Release();
        base.Dispose();
    }
}
