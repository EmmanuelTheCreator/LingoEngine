using System;
using System.Collections.Generic;
using AbstUI.Components.Buttons;
using AbstUI.Components.Menus;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Components.Buttons;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Styles;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Components.Menus
{
    internal class AbstSdlMenu : AbstSdlComponent, IAbstFrameworkMenu, IFrameworkFor<AbstMenu>, IHandleSdlEvent, IDisposable
    {
        private readonly List<AbstSdlMenuItem> _items = new();
        private readonly SdlFontManager _fontManager;
        private ISdlFontLoadedByUser? _font;
        private nint _texture;
        private bool _isDirty = true;
        private int _texW;
        private int _texH;
        private int _hoverIndex = -1;
        private bool _suppressNextRelease;
        private bool _dragSelecting;

        private const int ItemHeight = 18;

        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public AbstSdlMenu(AbstSdlComponentFactory factory, string name) : base(factory)
        {
            Name = name;
            _fontManager = factory.FontManagerTyped;
            Visibility = false;
            ComponentContext.AlwaysOnTop = true;
            Width = 80;
            Height = 20;
        }

        public void AddItem(IAbstFrameworkMenuItem item)
        {
            if (item is AbstSdlMenuItem sdlItem && !_items.Contains(sdlItem))
            {
                _items.Add(sdlItem);
                _isDirty = true;
                ComponentContext.QueueRedraw(this);
            }
        }

        public void ClearItems()
        {
            if (_items.Count == 0) return;
            _items.Clear();
            _isDirty = true;
            ComponentContext.QueueRedraw(this);
        }

        public void PositionPopup(IAbstFrameworkButton button)
        {
            if (button is AbstSdlButton sdlBtn)
            {
                var buttonPos = GetAbsolutePosition(sdlBtn.ComponentContext);
                var parentCtx = ComponentContext.VisualParent ?? ComponentContext;
                var parentPos = GetAbsolutePosition(parentCtx);
                X = buttonPos.X - parentPos.X;
                Y = buttonPos.Y - parentPos.Y + sdlBtn.Height;
            }
        }

        public void Popup()
        {
            Visibility = true;
            Factory.RootContext.ComponentContainer.Activate(ComponentContext);
            _isDirty = true;
            ComponentContext.QueueRedraw(this);
            _suppressNextRelease = true;
            _dragSelecting = false;
        }

        public void HandleEvent(AbstSDLEvent e)
        {
            ref var ev = ref e.Event;

            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    {
                        int localY = ev.motion.y - (int)Y;
                        int idx = localY / ItemHeight;
                        if (idx < 0 || idx >= _items.Count)
                        {
                            if (_hoverIndex != -1)
                            {
                                _hoverIndex = -1;
                                _isDirty = true;
                                ComponentContext.QueueRedraw(this);
                            }
                        }
                        else if (_hoverIndex != idx)
                        {
                            _hoverIndex = idx;
                            _isDirty = true;
                            ComponentContext.QueueRedraw(this);
                        }
                        _dragSelecting = (ev.motion.state & SDL.SDL_BUTTON_LMASK) != 0;
                        break;
                    }
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    if (ev.button.button == SDL.SDL_BUTTON_LEFT)
                    {
                        _dragSelecting = true;
                        _suppressNextRelease = false;
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    if (ev.button.button == SDL.SDL_BUTTON_LEFT)
                    {
                        if (_suppressNextRelease && !_dragSelecting)
                        {
                            _suppressNextRelease = false;
                            _dragSelecting = false;
                            break;
                        }
                        _suppressNextRelease = false;
                        int localY = ev.button.y - (int)Y;
                        int idx = localY / ItemHeight;
                        _dragSelecting = false;
                        if (idx >= 0 && idx < _items.Count)
                        {
                            var item = _items[idx];
                            if (item.Enabled)
                                item.Invoke();
                        }

                        Visibility = false;
                        Factory.RootContext.ComponentContainer.Deactivate(ComponentContext);
                        e.StopPropagation = true;
                    }
                    break;
            }
        }

        private void EnsureResources(AbstSDLRenderContext context)
        {
            if (_font == null)
                _font = _fontManager.GetDefaultFont<IAbstSdlFont>().Get(this, 12);
        }

        private static (float X, float Y) GetAbsolutePosition(AbstSDLComponentContext context)
        {
            float x = context.X;
            float y = context.Y;
            var parent = context.VisualParent;
            while (parent != null)
            {
                x += parent.X;
                y += parent.Y;
                parent = parent.VisualParent;
            }
            return (x, y);
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility || !_isDirty)
                return default;

            EnsureResources(context);

            int w = 0;
            foreach (var it in _items)
            {
                SDL_ttf.TTF_SizeUTF8(_font!.FontHandle, it.Name, out int tw, out _);
                int sw = 0;
                if (!string.IsNullOrEmpty(it.Shortcut))
                    SDL_ttf.TTF_SizeUTF8(_font.FontHandle, it.Shortcut, out sw, out _);
                int total = tw + sw + 24; // padding + shortcut space
                if (total > w) w = total;
            }
            if (w == 0) w = 50; // minimal width

            int h = _items.Count * ItemHeight;

            Width = w;
            Height = h;

            if (_texture == nint.Zero || _texW != w || _texH != h)
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);
                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                _texW = w;
                _texH = h;
            }

            var prev = SDL.SDL_GetRenderTarget(context.Renderer);
            SDL.SDL_SetRenderTarget(context.Renderer, _texture);

            var bg = AbstDefaultColors.BG_TopMenu;
            SDL.SDL_SetRenderDrawColor(context.Renderer, bg.R, bg.G, bg.B, bg.A);
            SDL.SDL_RenderClear(context.Renderer);

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                int top = i * ItemHeight;
                if (i == _hoverIndex && item.Enabled)
                {
                    var hov = AbstDefaultColors.ListHoverColor;
                    SDL.SDL_SetRenderDrawColor(context.Renderer, hov.R, hov.G, hov.B, hov.A);
                    SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = top, w = w, h = ItemHeight };
                    SDL.SDL_RenderFillRect(context.Renderer, ref rect);
                }

                if (item.CheckMark)
                {
                    SDL_ttf.TTF_SizeUTF8(_font!.FontHandle, "\u2713", out int cw, out int ch);
                    nint chkSurf = SDL_ttf.TTF_RenderUTF8_Blended(_font.FontHandle, "\u2713", AbstDefaultColors.TextColorLabels.ToSDLColor());
                    nint chkTex = SDL.SDL_CreateTextureFromSurface(context.Renderer, chkSurf);
                    SDL.SDL_FreeSurface(chkSurf);
                    SDL.SDL_Rect cdst = new SDL.SDL_Rect { x = 4, y = top + (ItemHeight - ch) / 2, w = cw, h = ch };
                    SDL.SDL_RenderCopy(context.Renderer, chkTex, nint.Zero, ref cdst);
                    SDL.SDL_DestroyTexture(chkTex);
                }

                SDL_ttf.TTF_SizeUTF8(_font!.FontHandle, item.Name, out int tw, out int th);
                nint textSurf = SDL_ttf.TTF_RenderUTF8_Blended(_font.FontHandle, item.Name, AbstDefaultColors.TextColorLabels.ToSDLColor());
                nint textTex = SDL.SDL_CreateTextureFromSurface(context.Renderer, textSurf);
                SDL.SDL_FreeSurface(textSurf);
                int textX = 20;
                SDL.SDL_Rect tdst = new SDL.SDL_Rect { x = textX, y = top + (ItemHeight - th) / 2, w = tw, h = th };
                SDL.SDL_RenderCopy(context.Renderer, textTex, nint.Zero, ref tdst);
                SDL.SDL_DestroyTexture(textTex);

                if (!string.IsNullOrEmpty(item.Shortcut))
                {
                    SDL_ttf.TTF_SizeUTF8(_font.FontHandle, item.Shortcut, out int sw, out int sh);
                    nint scSurf = SDL_ttf.TTF_RenderUTF8_Blended(_font.FontHandle, item.Shortcut, AbstDefaultColors.TextColorLabels.ToSDLColor());
                    nint scTex = SDL.SDL_CreateTextureFromSurface(context.Renderer, scSurf);
                    SDL.SDL_FreeSurface(scSurf);
                    SDL.SDL_Rect sdst = new SDL.SDL_Rect { x = w - sw - 4, y = top + (ItemHeight - sh) / 2, w = sw, h = sh };
                    SDL.SDL_RenderCopy(context.Renderer, scTex, nint.Zero, ref sdst);
                    SDL.SDL_DestroyTexture(scTex);
                }
            }

            SDL.SDL_SetRenderTarget(context.Renderer, prev);

            _isDirty = false;
            return _texture;
        }

        public override void Dispose()
        {
            if (_texture != nint.Zero)
                SDL.SDL_DestroyTexture(_texture);
            _font?.Release();
            base.Dispose();
        }
    }
}
