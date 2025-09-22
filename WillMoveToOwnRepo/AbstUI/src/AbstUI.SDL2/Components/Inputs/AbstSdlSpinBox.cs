using System;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;
using AbstUI.Styles;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Components.Inputs;

internal class AbstSdlSpinBox : AbstSdlComponent, IAbstFrameworkSpinBox, IFrameworkFor<AbstInputSpinBox>, IHandleSdlEvent, ISdlFocusable, IDisposable, IHasTextBackgroundBorderColor
{
    private readonly AbstSdlInputNumber<float> _number;
    private const int ButtonWidth = 16;

    public AMargin Margin { get; set; } = AMargin.Zero;
    public object FrameworkNode => this;
    public event Action? ValueChanged;
    public event Action? OnCommit;
    public bool Enabled { get; set; } = true;
    public AColor ButtonColor { get; set; } = AbstDefaultColors.InputAccentColor;
    public AColor ButtonBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;

    public float Value
    {
        get => _number.Value;
        set => _number.Value = value;
    }

    public float Min
    {
        get => _number.Min;
        set => _number.Min = value;
    }

    public float Max
    {
        get => _number.Max;
        set => _number.Max = value;
    }

    public float Step
    {
        get => _number.Step;
        set => _number.Step = value;
    }


    public AbstSdlSpinBox(AbstSdlComponentFactory factory) : base(factory)
    {
        _number = new AbstSdlInputNumber<float>(factory);
        _number.ComponentContext.SetParents(ComponentContext);
        _number.ValueChanged += () => ValueChanged?.Invoke();
        _number.OnCommit += () => OnCommit?.Invoke();
        Width = 50;
        Height = 20;
    }

    public AColor TextColor
    {
        get => _number.TextColor;
        set => _number.TextColor = value;
    }

    public AColor BackgroundColor
    {
        get => _number.BackgroundColor;
        set => _number.BackgroundColor = value;
    }

    public AColor BorderColor
    {
        get => _number.BorderColor;
        set => _number.BorderColor = value;
    }
    public virtual bool CanHandleEvent(AbstSDLEvent e)
    {
        return Enabled && (e.IsInside || !e.HasCoordinates);
    }
    public void HandleEvent(AbstSDLEvent e)
    {
        if (!Enabled) return;

        var ev = e.Event;

        if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
        {
            if (e.IsInside && e.ComponentLeft >= Width - ButtonWidth)
            {
                var dir = e.ComponentTop > Height / 2 ? -1 : 1;
                Value += Step * dir;
                OnCommit?.Invoke();
                e.StopPropagation = true;
                ComponentContext.QueueRedraw(this);
                return;
            }
        }

        _number.HandleEvent(e);
        if (e.StopPropagation)
            ComponentContext.QueueRedraw(this);
    }

    private SDL.SDL_Rect GetUpRect() => new SDL.SDL_Rect
    {
        x = (int)(X + Width - ButtonWidth),
        y = (int)Y,
        w = ButtonWidth,
        h = (int)Height / 2
    };

    private SDL.SDL_Rect GetDownRect() => new SDL.SDL_Rect
    {
        x = (int)(X + Width - ButtonWidth),
        y = (int)Y + (int)Height / 2,
        w = ButtonWidth,
        h = (int)Height / 2
    };


    public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        if (!Visibility) return default;

        _number.X = X;
        _number.Y = Y;
        _number.Width = Width - ButtonWidth;
        _number.Height = Height;
        var result = _number.Render(context);

        var renderer = context.Renderer;
        var up = GetUpRect();
        var down = GetDownRect();
        SDL.SDL_SetRenderDrawColor(renderer, ButtonColor.R, ButtonColor.G, ButtonColor.B, ButtonColor.A);
        SDL.SDL_RenderFillRect(renderer, ref up);
        SDL.SDL_RenderFillRect(renderer, ref down);
        SDL.SDL_SetRenderDrawColor(renderer, ButtonBorderColor.R, ButtonBorderColor.G, ButtonBorderColor.B, ButtonBorderColor.A);
        SDL.SDL_RenderDrawRect(renderer, ref up);
        SDL.SDL_RenderDrawRect(renderer, ref down);
        // simple triangles
        SDL.SDL_RenderDrawLine(renderer, up.x + up.w / 2, up.y + 3, up.x + 3, up.y + up.h - 3);
        SDL.SDL_RenderDrawLine(renderer, up.x + up.w / 2, up.y + 3, up.x + up.w - 3, up.y + up.h - 3);
        SDL.SDL_RenderDrawLine(renderer, down.x + 3, down.y + 3, down.x + down.w / 2, down.y + down.h - 3);
        SDL.SDL_RenderDrawLine(renderer, down.x + down.w - 3, down.y + 3, down.x + down.w / 2, down.y + down.h - 3);

        return result;
    }

    public override void Dispose()
    {
        base.Dispose();
        _number.Dispose();
    }

    public bool HasFocus => _number.HasFocus;

    public void SetFocus(bool focus) => _number.SetFocus(focus);
}

