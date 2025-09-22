using System;
using System.Numerics;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Components.Containers;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Components.Inputs
{
    internal class AbstSdlColorPicker : AbstSdlComponent, IAbstFrameworkColorPicker, IFrameworkFor<AbstColorPicker>, ISdlFocusable, IHandleSdlEvent, IDisposable
    {

        public bool Enabled { get; set; } = true;
        public AMargin Margin { get; set; } = AMargin.Zero;

        private AColor _color = AColors.White;
        private AColor _renderedColor;
        private bool _focused;
        private nint _texture;
        private int _texW;
        private int _texH;
        private AbstSdlColorPickerPopup? _popup;
        private bool _isOpen;
        public AColor Color 
        {
            get => _color;
            set
            {
                if (!_color.Equals(value))
                {
                    _color = value;
                    ValueChanged?.Invoke();
                    OnCommit?.Invoke();
                }
            }
        }

        public event Action? ValueChanged;
        public event Action? OnCommit;
        public object FrameworkNode => this;

        public bool HasFocus => _focused;
        public void SetFocus(bool focus)
        {
            _focused = focus;
            //if (!focus) ClosePopup();
        }
        public AbstSdlColorPicker(AbstSdlComponentFactory factory) : base(factory)
        {
            Width = 20;
            Height = 20;
        }
        public virtual bool CanHandleEvent(AbstSDLEvent e) => true;
        public void HandleEvent(AbstSDLEvent e)
        {
            if (!Enabled) return;
            var ev = e.Event;
            if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && ev.button.button == SDL.SDL_BUTTON_LEFT)
            {
                bool inside = e.IsInside;
                if (inside)
                {
                    Factory.FocusManager.SetFocus(this);
                    if (_isOpen) ClosePopup();
                    else OpenPopup();
                    e.StopPropagation = true;
                }
                else if (_isOpen && _popup is { } pop)
                {
                    if (!(ev.button.x >= pop.X && ev.button.x <= pop.X + pop.Width &&
                          ev.button.y >= pop.Y && ev.button.y <= pop.Y + pop.Height))
                        ClosePopup();
                }
            }
            else if (ev.type == SDL.SDL_EventType.SDL_KEYDOWN && _isOpen)
            {
                if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE)
                {
                    ClosePopup();
                    e.StopPropagation = true;
                }
            }
        }

        private void OpenPopup()
        {
            if (_popup == null)
            {
                _popup = new AbstSdlColorPickerPopup(Factory);
                _popup.ComponentContext.SetParents(ComponentContext, null);
                _popup.ColorChanged += PopupOnColorChanged;
            }

            _popup.SetColor(Color);
            UpdatePopupPosition();
            _popup.Visibility = true;
            Factory.RootContext.ComponentContainer.Activate(_popup.ComponentContext);
            _isOpen = true;
        }

        private void ClosePopup()
        {
            if (_popup == null) return;
            _popup.Visibility = false;
            Factory.RootContext.ComponentContainer.Deactivate(_popup.ComponentContext);
            _isOpen = false;
        }

        private void PopupOnColorChanged(AColor c)
        {
            Color = c;
            ComponentContext.QueueRedraw(this);
        }

        private void UpdatePopupPosition()
        {
            if (_popup == null) return;
            _popup.PositionBelow(ComponentContext, Height + 2);
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (_isOpen)
                UpdatePopupPosition();
            if (!Visibility)
                return default;

            int w = (int)Width;
            int h = (int)Height;

            if (_texture == nint.Zero || _texW != w || _texH != h || !_renderedColor.Equals(Color))
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);

                _texture = SDL.SDL_CreateTexture(context.Renderer,
                    SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
                    w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

                var prev = SDL.SDL_GetRenderTarget(context.Renderer);
                SDL.SDL_SetRenderTarget(context.Renderer, _texture);

                SDL.SDL_RenderClear(context.Renderer);
                SDL.SDL_Rect r = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                SDL.SDL_SetRenderDrawColor(context.Renderer, Color.R, Color.G, Color.B, Color.A);
                SDL.SDL_RenderFillRect(context.Renderer, ref r);

                SDL.SDL_SetRenderDrawColor(context.Renderer, 0, 0, 0, 255);
                SDL.SDL_RenderDrawRect(context.Renderer, ref r);

                SDL.SDL_SetRenderTarget(context.Renderer, prev);
                _texW = w;
                _texH = h;
                _renderedColor = Color;
            }

            return _texture;
        }

        public override void Dispose()
        {
            if (_texture != nint.Zero)
                SDL.SDL_DestroyTexture(_texture);
            _popup?.Dispose();
            base.Dispose();
        }
    }
}
