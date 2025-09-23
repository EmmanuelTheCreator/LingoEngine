using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2.Bitmaps;
using AbstUI.Components.Buttons;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.Core;
using AbstUI.Styles;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Components.Buttons
{
    internal class AbstSdlButton : AbstSdlComponent, IAbstFrameworkButton, IFrameworkFor<AbstButton>, IHandleSdlEvent, ISdlFocusable, IDisposable, IHasButtonBorderStates
    {
        private ISdlFontLoadedByUser? _font;
        private nint _texture;
        private string _renderedText = string.Empty;
        private int _texW;
        private int _texH;
        private bool _pressed;
        private bool _focused;
        private readonly SdlFontManager _fontManager;
        private bool _isDirty = true;
        private string _text = string.Empty;
        private AColor _textColor = AColor.FromRGB(50, 50, 50);
        private AColor _borderColor = AbstDefaultColors.Button_Border_Normal;
        private AColor _borderHoverColor = AbstDefaultColors.Button_Border_Hover;
        private AColor _borderPressedColor = AbstDefaultColors.Button_Border_Pressed;
        private AColor _backgroundColor = AbstDefaultColors.Button_Bg_Normal;
        private AColor _backgroundHoverColor = AbstDefaultColors.Button_Bg_Hover;
        private AColor _backgroundPressedColor = AbstDefaultColors.Button_Bg_Pressed;
        private SdlTexture2D? _iconTexture;
        private IAbstUITextureUserSubscription? _iconTextureSub;
        private bool _isHover;
        private IAbstTexture2D? _iconTextureOnOri;

        public AColor BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value; RequestRedraw();
            }
        }
        public AColor BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value; RequestRedraw();
            }
        }
        public AColor BackgroundHoverColor
        {
            get => _backgroundHoverColor;
            set
            {
                _backgroundHoverColor = value; RequestRedraw();
            }
        }
        public AColor BorderHoverColor
        {
            get => _borderHoverColor;
            set { _borderHoverColor = value; RequestRedraw(); }
        }
        public AColor BorderPressedColor
        {
            get => _borderPressedColor;
            set { _borderPressedColor = value; RequestRedraw(); }
        }
        public AColor BackgroundPressedColor
        {
            get => _backgroundPressedColor;
            set { _backgroundPressedColor = value; RequestRedraw(); }
        }
        public AColor TextColor { get => _textColor; set { _textColor = value; RequestRedraw(); } }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public string Text { get => _text; set { _text = value; RequestRedraw(); } }
        public bool Enabled { get; set; } = true;
     
        public IAbstTexture2D? IconTexture
        {
            get => _iconTexture;
            set
            {
                if (_iconTextureOnOri == value) return;
                _iconTextureOnOri = value;
                _iconTextureSub?.Release();
                if (value is SdlTexture2D img && img.Width != 0 && img.Height != 0)
                {
                    _iconTexture = (SdlTexture2D)img.Clone(ComponentContext.Renderer);
                    _iconTextureSub = img.AddUser(this);
                }
                else
                    _iconTexture = null;
                RequestRedraw();
            }
        }

      
        public object FrameworkNode => this;

        public event Action? Pressed;


        public AbstSdlButton(AbstSdlComponentFactory factory) : base(factory)
        {
            _fontManager = factory.FontManagerTyped;
            Width = 20;
            Height = 20;
        }
        private void EnsureResources(AbstSDLRenderContext ctx)
        {
            if (_font == null)
                _font = _fontManager.GetDefaultFont<IAbstSdlFont>().Get(this, 12);
        }
        private void RequestRedraw()
        {
            _isDirty = true;
            ComponentContext.QueueRedraw(this);
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return nint.Zero;
            if (!_isDirty) return _texture;


            EnsureResources(context);
            int w = (int)Width;
            int h = (int)Height;
            if (_texture == nint.Zero || _texW != w || _texH != h || _renderedText != Text || _isDirty)
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);
                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

                var prevTarget = SDL.SDL_GetRenderTarget(context.Renderer);
                SDL.SDL_SetRenderTarget(context.Renderer, _texture);

                var bg = _pressed ? _backgroundPressedColor : _isHover ? _backgroundHoverColor : _backgroundColor;
                SDL.SDL_SetRenderDrawColor(context.Renderer, bg.R, bg.G, bg.B, bg.A);
                SDL.SDL_RenderClear(context.Renderer);

                int tw = 0, th = 0, tx = 0, ty = 0;
                bool hasText = !string.IsNullOrWhiteSpace(Text);
                if (hasText)
                {
                    SDL_ttf.TTF_SizeUTF8(_font!.FontHandle, Text, out tw, out th);
                    int baseline = (h - (SDL_ttf.TTF_FontAscent(_font.FontHandle) - SDL_ttf.TTF_FontDescent(_font.FontHandle))) / 2
                                   + SDL_ttf.TTF_FontAscent(_font.FontHandle);
                    ty = baseline - SDL_ttf.TTF_FontAscent(_font.FontHandle);
                }

                if (_iconTexture != null && hasText)
                {
                    SDL.SDL_QueryTexture(_iconTexture.Handle, out _, out _, out int iw, out int ih);
                    int totalW = iw + 4 + tw;
                    int startX = (w - totalW) / 2;
                    SDL.SDL_Rect idst = new SDL.SDL_Rect { x = startX, y = (h - ih) / 2, w = iw, h = ih };
                    SDL.SDL_RenderCopy(context.Renderer, _iconTexture.Handle, nint.Zero, ref idst);
                    tx = startX + iw + 4;
                }
                else if (_iconTexture != null)
                {
                    SDL.SDL_QueryTexture(_iconTexture.Handle, out _, out _, out int iw, out int ih);
                    SDL.SDL_Rect idst = new SDL.SDL_Rect { x = (w - iw) / 2, y = (h - ih) / 2, w = iw, h = ih };
                    SDL.SDL_RenderCopy(context.Renderer, _iconTexture.Handle, nint.Zero, ref idst);
                }
                else if (hasText)
                {
                    tx = (w - tw) / 2;
                }

                if (hasText)
                {
                    nint textSurf = SDL_ttf.TTF_RenderUTF8_Blended(_font!.FontHandle, Text, TextColor.ToSDLColor());
                    nint textTex = SDL.SDL_CreateTextureFromSurface(context.Renderer, textSurf);
                    SDL.SDL_FreeSurface(textSurf);
                    SDL.SDL_SetTextureBlendMode(textTex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                    SDL.SDL_Rect tdst = new SDL.SDL_Rect { x = tx, y = ty, w = tw, h = th };
                    SDL.SDL_RenderCopy(context.Renderer, textTex, nint.Zero, ref tdst);
                    SDL.SDL_DestroyTexture(textTex);
                }

                var border = _pressed ? _borderPressedColor : _isHover ? _borderHoverColor : _borderColor;
                SDL.SDL_SetRenderDrawColor(context.Renderer, border.R, border.G, border.B, border.A);
                SDL.SDL_Rect rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                SDL.SDL_RenderDrawRect(context.Renderer, ref rect);

                SDL.SDL_SetRenderTarget(context.Renderer, prevTarget);
                _renderedText = Text;
                _texW = w;
                _texH = h;
            }

            _isDirty = false;
            return _texture;
        }

        public virtual bool CanHandleEvent(AbstSDLEvent e)
        {
            return Enabled && (e.IsInside || (_isHover && e.Event.type == SDL.SDL_EventType.SDL_MOUSEMOTION));
        }

        public virtual void HandleEvent(AbstSDLEvent e)
        {
            if (!Enabled) return;

            var ev = e.Event;
            if (!e.IsInside)
            {
                if (_isHover)
                {
                    _isHover = false;
                    RequestRedraw();
                }
                return;
            }
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    if (ev.button.button == SDL.SDL_BUTTON_LEFT)
                    {
                        Factory.FocusManager.SetFocus(this);
                        _pressed = true;
                        e.StopPropagation = true;
                        RequestRedraw();
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    if (_pressed && ev.button.button == SDL.SDL_BUTTON_LEFT)
                    {
                        Pressed?.Invoke();
                        _pressed = false;
                        e.StopPropagation = true;
                        RequestRedraw();
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    if (!_isHover)
                    {
                        _isHover = true;
                        RequestRedraw();
                    }
                    break;

            }
        }

        private bool HitTest(int mx, int my)
            => mx >= X && mx <= X + Width && my >= Y && my <= Y + Height;

        public bool HasFocus => _focused;

        public void SetFocus(bool focus)
        {
            _focused = focus;
            RequestRedraw();
        }

        public void Invoke() => Pressed?.Invoke();

        public override void Dispose()
        {
            if (_texture != nint.Zero)
                SDL.SDL_DestroyTexture(_texture);
            _iconTextureSub?.Release();
            _font?.Release();
            base.Dispose();
        }

        public void SetBorderStateColors(AColor normal, AColor hover, AColor pressed)
        {
            _borderColor = normal;
            _borderHoverColor = hover;
            _borderPressedColor = pressed;
            RequestRedraw();
        }
    }
}
