using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;
using AbstUI.Styles;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Components.Inputs
{
    internal class AbstSdlInputSlider<TValue> : AbstSdlComponent, IAbstFrameworkInputSlider<TValue>, IFrameworkFor<AbstInputSlider<TValue>>, IHandleSdlEvent, ISdlFocusable, IDisposable where TValue : struct
    {

        public AMargin Margin { get; set; } = AMargin.Zero;
        public bool Enabled { get; set; } = true;
        private TValue _value = default!;
        private bool _focused;
        private nint _texture;
        private int _texW;
        private int _texH;
        private TValue _renderedValue = default!;
        private AColor _renderedTrackColor;
        private AColor _renderedKnobColor;
        private AColor _renderedKnobBorderColor;
        private bool _dragging;
        private bool _isHover;
        private bool _pendingCommit;

        public object FrameworkNode => this;
        public TValue Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public TValue MinValue { get; set; } = default!;
        public TValue MaxValue { get; set; } = default!;
        public TValue Step { get; set; } = default!;
        public AColor TrackColor { get; set; } = AbstDefaultColors.InputBorderColor.Lighten(0.5f);
        public AColor KnobColor { get; set; } = AbstDefaultColors.InputAccentColor;
        public AColor KnobBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;
        public event Action? ValueChanged;
        public event Action? OnCommit;




        public AbstSdlInputSlider(AbstSdlComponentFactory factory) : base(factory)
        {
            Width = 150;
            Height = 20;
        }
        public virtual bool CanHandleEvent(AbstSDLEvent e)
        {
            return Enabled && (e.IsInside || (_isHover && e.Event.type == SDL.SDL_EventType.SDL_MOUSEMOTION) || !e.HasCoordinates);
        }
        public void HandleEvent(AbstSDLEvent e)
        {
            if (!Enabled) return;
            var ev = e.Event;
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN when ev.button.button == SDL.SDL_BUTTON_LEFT && e.IsInside:
                    Factory.FocusManager.SetFocus(this);
                    _dragging = true;
                    UpdateValueFromMouse(e.ComponentLeft);
                    e.StopPropagation = true;
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP when ev.button.button == SDL.SDL_BUTTON_LEFT:
                    _dragging = false;
                    if (_pendingCommit)
                    {
                        _pendingCommit = false;
                        OnCommit?.Invoke();
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    _isHover = e.IsInside;
                    if (_dragging)
                    {
                        UpdateValueFromMouse(e.ComponentLeft);
                        e.StopPropagation = true;
                    }
                    break;
            }
        }

        private void UpdateValueFromMouse(float mx)
        {
            float min = Convert.ToSingle(MinValue);
            float max = Convert.ToSingle(MaxValue);
            float step = Convert.ToSingle(Step);
            float t = (mx) / Width;
            if (t < 0) t = 0; if (t > 1) t = 1;
            float val = min + t * (max - min);
            if (step > 0)
                val = min + MathF.Round((val - min) / step) * step;
            var before = _value;
            Value = (TValue)Convert.ChangeType(val, typeof(TValue));
            if (!Equals(before, _value))
            {
                _pendingCommit = true;
            }
            ComponentContext.QueueRedraw(this);
        }

        private bool HitTest(int x, int y) => x >= X && x <= X + Width && y >= Y && y <= Y + Height;

        public bool HasFocus => _focused;
        public void SetFocus(bool focus) => _focused = focus;

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;

            int w = (int)Width;
            int h = (int)Height;

            if (_texture == nint.Zero || w != _texW || h != _texH || !Equals(_renderedValue, _value) ||
                !_renderedTrackColor.Equals(TrackColor) || !_renderedKnobColor.Equals(KnobColor) ||
                !_renderedKnobBorderColor.Equals(KnobBorderColor))
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);

                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                _texW = w;
                _texH = h;
                _renderedValue = _value;
                _renderedTrackColor = TrackColor;
                _renderedKnobColor = KnobColor;
                _renderedKnobBorderColor = KnobBorderColor;

                var prev = SDL.SDL_GetRenderTarget(context.Renderer);
                SDL.SDL_SetRenderTarget(context.Renderer, _texture);

                SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 0);
                SDL.SDL_RenderClear(context.Renderer);

                // draw track
                SDL.SDL_SetRenderDrawColor(context.Renderer, TrackColor.R, TrackColor.G, TrackColor.B, TrackColor.A);
                SDL.SDL_Rect track = new SDL.SDL_Rect { x = 0, y = h / 2 - 2, w = w, h = 4 };
                SDL.SDL_RenderFillRect(context.Renderer, ref track);

                // determine knob position
                float val = Convert.ToSingle(_value);
                float min = Convert.ToSingle(MinValue);
                float max = Convert.ToSingle(MaxValue);
                float t = max - min > 0 ? (val - min) / (max - min) : 0f;
                if (t < 0) t = 0; if (t > 1) t = 1;

                int knobW = Math.Min(10, w);
                int knobX = (int)((w - knobW) * t);
                SDL.SDL_Rect knob = new SDL.SDL_Rect { x = knobX, y = 0, w = knobW, h = h };
                SDL.SDL_SetRenderDrawColor(context.Renderer, KnobColor.R, KnobColor.G, KnobColor.B, KnobColor.A);
                SDL.SDL_RenderFillRect(context.Renderer, ref knob);
                SDL.SDL_SetRenderDrawColor(context.Renderer, KnobBorderColor.R, KnobBorderColor.G, KnobBorderColor.B, KnobBorderColor.A);
                SDL.SDL_RenderDrawRect(context.Renderer, ref knob);

                SDL.SDL_SetRenderTarget(context.Renderer, prev);
            }

            return _texture;
        }

        public override void Dispose()
        {
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
            base.Dispose();
        }
    }
}
