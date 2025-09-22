using System;
using System.Numerics;
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
    internal class AbstSdlInputCheckbox : AbstSdlComponent, IAbstFrameworkInputCheckbox, IFrameworkFor<AbstInputCheckbox>, IHandleSdlEvent, ISdlFocusable, IDisposable
    {

        public bool Enabled { get; set; } = true;
        private bool _checked;
        private bool _focused;
        private nint _texture;
        private int _texW;
        private int _texH;
        private bool _prevChecked;
        private AColor _renderedBorderColor;
        private AColor _renderedBackgroundColor;
        private AColor _renderedCheckColor;
        private bool _isHover;

        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    ValueChanged?.Invoke();
                    OnCommit?.Invoke();
                }
            }
        }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public AColor BorderColor { get; set; } = AbstDefaultColors.InputBorderColor;
        public AColor BackgroundColor { get; set; } = AbstDefaultColors.Input_Bg;
        public AColor CheckColor { get; set; } = AbstDefaultColors.InputAccentColor;
        public event Action? ValueChanged;
        public event Action? OnCommit;
        public object FrameworkNode => this;
        public AbstSdlInputCheckbox(AbstSdlComponentFactory factory) : base(factory)
        {
            Width = 20;
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
            if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && ev.button.button == SDL.SDL_BUTTON_LEFT && e.IsInside)
            {
                Factory.FocusManager.SetFocus(this);
                Checked = !Checked;
                e.StopPropagation = true;
                return;
            }
            if (e.Event.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
                _isHover = e.IsInside;
        }


        public bool HasFocus => _focused;
        public void SetFocus(bool focus) => _focused = focus;

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;

            int w = (int)Width;
            int h = (int)Height;

            // create texture if needed
            if (_texture == nint.Zero || w != _texW || h != _texH || _prevChecked != _checked ||
                !_renderedBorderColor.Equals(BorderColor) || !_renderedBackgroundColor.Equals(BackgroundColor) ||
                !_renderedCheckColor.Equals(CheckColor))
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);

                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                _texW = w;
                _texH = h;
                _prevChecked = _checked;
                _renderedBorderColor = BorderColor;
                _renderedBackgroundColor = BackgroundColor;
                _renderedCheckColor = CheckColor;

                var prev = SDL.SDL_GetRenderTarget(context.Renderer);
                SDL.SDL_SetRenderTarget(context.Renderer, _texture);

                // clear with transparent background
                SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 0);
                SDL.SDL_RenderClear(context.Renderer);

                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };

                SDL.SDL_SetRenderDrawColor(context.Renderer, BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
                SDL.SDL_RenderFillRect(context.Renderer, ref rect);

                if (_checked)
                {
                    SDL.SDL_SetRenderDrawColor(context.Renderer, CheckColor.R, CheckColor.G, CheckColor.B, CheckColor.A);
                    SDL.SDL_RenderFillRect(context.Renderer, ref rect);
                }

                SDL.SDL_SetRenderDrawColor(context.Renderer, BorderColor.R, BorderColor.G, BorderColor.B, BorderColor.A);
                SDL.SDL_RenderDrawRect(context.Renderer, ref rect);

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
