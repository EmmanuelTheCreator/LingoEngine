using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Tools;
using AbstUI.SDL2.Styles;
using AbstUI.Styles;
using static AbstUI.SDL2.SDLL.SDL;

namespace AbstUI.SDL2.Components.Containers
{
    public class AbstSdlTabContainer : AbstSdlComponent, IAbstFrameworkTabContainer, IFrameworkFor<AbstTabContainer>, IHandleSdlEvent, ISdlFocusable, IDisposable
    {
        private readonly List<IAbstFrameworkTabItem> _children = new();
        private int _selectedIndex = -1;
        private bool _focused;
        private ISdlFontLoadedByUser? _font;
        private nint _texture;
        private int _texW;
        private int _texH;
        private readonly List<SDL.SDL_Rect> _tabRects = new();
        private readonly List<int> _tabOffsets = new();
        private readonly List<int> _tabWidths = new();
        private readonly Dictionary<AbstSdlTabItem, (float Width, float Height)> _tabContentSizes = new();
        private int _hoverIndex = -1;
        private int _tabHeight = 20;
        private int _scrollOffset;
        private int _tabViewportWidth;
        private int _totalTabWidth;
        private bool _leftArrowVisible;
        private bool _rightArrowVisible;
        private SDL.SDL_Rect _leftArrowRect;
        private SDL.SDL_Rect _rightArrowRect;
        private const int MinTabWidth = 60;
        private const int TabHorizontalPadding = 10;

        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;

        public string? Font { get; set; }
        public bool Enabled { get; set; } = true;
        public int FontSize { get; set; } = 12;
        public AColor TextColor { get; set; } = AbstDefaultColors.Tab_Deselected_TextColor;

        public AColor SelectedTextColor { get; set; } = AbstDefaultColors.Tab_Selected_TextColor;

        public AColor TabHeaderBGColor { get; set; } = AbstDefaultColors.BG_Tabs;
        public AColor TabHeaderBorderColor { get; set; } = AbstDefaultColors.Border_Tabs;

        public AColor TabHeaderSelectedBGColor { get; set; } = AbstDefaultColors.BG_Tabs_Hover;
        public AColor TabHeaderSelectedBorderColor { get; set; } = AbstDefaultColors.TabActiveBorder;

        public AColor TabHeaderHoverBGColor { get; set; } = AbstDefaultColors.BG_Tabs_Hover;
        public AColor TabHeaderHoverBorderColor { get; set; } = AbstDefaultColors.TabActiveBorder;

        public AColor BackgroundColor { get; set; } = AbstDefaultColors.BG_WhiteMenus;
        public AColor BorderColor { get; set; } = AbstDefaultColors.Border_Tabs;
        public int BorderThickness { get; set; } = 1;


        public string? SelectedTabName =>
            _selectedIndex >= 0 && _selectedIndex < _children.Count ? _children[_selectedIndex].Title : null;

        public bool HasFocus => _focused;

        public void SetFocus(bool focus) => _focused = focus;

        public AbstSdlTabContainer(AbstSdlComponentFactory factory) : base(factory)
        {
        }

        public void AddTab(IAbstFrameworkTabItem content)
        {
            _children.Add(content);
            AttachTab(content);
            if (_selectedIndex == -1)
                _selectedIndex = 0;
            _texture = nint.Zero;
            ComponentContext.QueueRedraw(this);
        }

        public void RemoveTab(IAbstFrameworkTabItem content)
        {
            var index = _children.IndexOf(content);
            if (index >= 0)
            {
                DetachTab(content);
                _children.RemoveAt(index);
                if (_selectedIndex >= _children.Count)
                    _selectedIndex = _children.Count - 1;
                if (_hoverIndex == index)
                    _hoverIndex = -1;
                _texture = nint.Zero;
                ComponentContext.QueueRedraw(this);
            }

        }

        public IEnumerable<IAbstFrameworkTabItem> GetTabs() => _children.ToArray();

        public void ClearTabs()
        {
            foreach (var tab in _children)
                DetachTab(tab);
            _children.Clear();
            _selectedIndex = -1;
            _scrollOffset = 0;
            _hoverIndex = -1;
            _tabOffsets.Clear();
            _tabWidths.Clear();
            _tabRects.Clear();
            _tabContentSizes.Clear();
            _totalTabWidth = 0;
            _tabViewportWidth = 0;
            _leftArrowVisible = false;
            _rightArrowVisible = false;
            _texture = nint.Zero;
            ComponentContext.QueueRedraw(this);
        }

        public void SelectTabByName(string tabName)
        {
            var idx = _children.FindIndex(t => t.Title == tabName);
            if (idx >= 0)
            {
                _selectedIndex = idx;
                _texture = nint.Zero;
                ComponentContext.QueueRedraw(this);
            }
        }

        private void AttachTab(IAbstFrameworkTabItem tab)
        {
            if (tab.FrameworkNode is not AbstSdlComponent tabComponent)
                return;

            tabComponent.ComponentContext.SetParents(ComponentContext);
            AttachTabContent(tab, tabComponent);
        }

        private static void AttachTabContent(IAbstFrameworkTabItem tab, AbstSdlComponent tabComponent)
        {
            if (tab.Content?.FrameworkObj.FrameworkNode is AbstSdlComponent contentComponent)
                contentComponent.ComponentContext.SetParents(tabComponent.ComponentContext);
        }

        private void DetachTab(IAbstFrameworkTabItem tab)
        {
            if (tab.FrameworkNode is not AbstSdlComponent tabComponent)
                return;

            if (tab.FrameworkNode is AbstSdlTabItem tabItem)
                _tabContentSizes.Remove(tabItem);

            if (tab.Content?.FrameworkObj.FrameworkNode is AbstSdlComponent contentComponent)
                contentComponent.ComponentContext.SetParents(null);

            tabComponent.ComponentContext.SetParents(null);
        }


        private void EnsureResources(AbstSDLRenderContext ctx)
        {
            _font ??= ctx.SdlFontManager.GetTyped(this, Font, FontSize);
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;

            EnsureResources(context);
            int w = Math.Max(0, (int)Width);
            int h = Math.Max(0, (int)Height);

            bool needRender = _texture == nint.Zero || _texW != w || _texH != h;
            if (needRender)
            {
                if (_texture != nint.Zero)
                    SDL.SDL_DestroyTexture(_texture);
                _texture = SDL.SDL_CreateTexture(context.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
                SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            }

            var prevTarget = SDL.SDL_GetRenderTarget(context.Renderer);
            SDL.SDL_SetRenderTarget(context.Renderer, _texture);
            SDL.SDL_SetRenderDrawColor(context.Renderer, BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
            SDL.SDL_RenderClear(context.Renderer);

            int ascent = 0;
            int descent = 0;
            if (_children.Count > 0)
            {
                ascent = SDL_ttf.TTF_FontAscent(_font!.FontHandle);
                descent = SDL_ttf.TTF_FontDescent(_font.FontHandle);
            }
            int textBaselineHeight = ascent - descent + 4;
            _tabHeight = Math.Max(20, textBaselineHeight + 6);

            SDL.SDL_Rect contentRect = new SDL.SDL_Rect { x = 0, y = _tabHeight, w = w, h = Math.Max(0, h - _tabHeight) };
            SDL.SDL_SetRenderDrawColor(context.Renderer, BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
            SDL.SDL_RenderFillRect(context.Renderer, ref contentRect);
            if (BorderThickness > 0)
            {
                SDL.SDL_SetRenderDrawColor(context.Renderer, BorderColor.R, BorderColor.G, BorderColor.B, BorderColor.A);
                for (int t = 0; t < BorderThickness; t++)
                {
                    SDL.SDL_Rect br = new SDL.SDL_Rect
                    {
                        x = t,
                        y = _tabHeight + t,
                        w = Math.Max(0, w - 2 * t),
                        h = Math.Max(0, h - _tabHeight - 2 * t)
                    };
                    SDL.SDL_RenderDrawRect(context.Renderer, ref br);
                }
            }

            _tabRects.Clear();
            _tabOffsets.Clear();
            _tabWidths.Clear();

            var textWidths = new int[_children.Count];
            var textHeights = new int[_children.Count];
            _totalTabWidth = 0;

            for (int i = 0; i < _children.Count; i++)
            {
                var tab = _children[i];
                SDL_ttf.TTF_SizeUTF8(_font!.FontHandle, tab.Title, out int textW, out int textH);
                textWidths[i] = textW;
                textHeights[i] = textH;
                int tabW = Math.Max(textW + TabHorizontalPadding, MinTabWidth);
                _tabOffsets.Add(_totalTabWidth);
                _tabWidths.Add(tabW);
                _totalTabWidth += tabW;
                _tabRects.Add(new SDL.SDL_Rect { x = -1, y = -1, w = 0, h = 0 });
            }

            int arrowWidth = Math.Max(18, Math.Min(24, _tabHeight));
            bool requiresScrolling = _totalTabWidth > w;
            if (!requiresScrolling)
            {
                _scrollOffset = 0;
                _leftArrowVisible = false;
                _rightArrowVisible = false;
                _tabViewportWidth = w;
            }
            else
            {
                if (_scrollOffset < 0)
                    _scrollOffset = 0;

                while (true)
                {
                    bool leftVisible = _scrollOffset > 0;
                    int viewport = w - (leftVisible ? arrowWidth : 0);
                    if (viewport < 0)
                        viewport = 0;
                    bool rightVisible = (_scrollOffset + viewport) < _totalTabWidth;
                    if (rightVisible)
                    {
                        viewport -= arrowWidth;
                        if (viewport < 0)
                            viewport = 0;
                    }

                    int maxOffset = Math.Max(0, _totalTabWidth - viewport);
                    int desiredOffset = _scrollOffset;

                    if (_selectedIndex >= 0 && _selectedIndex < _tabOffsets.Count && viewport > 0)
                    {
                        int start = _tabOffsets[_selectedIndex];
                        int end = start + _tabWidths[_selectedIndex];
                        if (start < desiredOffset)
                            desiredOffset = start;
                        else if (end > desiredOffset + viewport)
                            desiredOffset = end - viewport;
                    }

                    if (desiredOffset > maxOffset)
                        desiredOffset = maxOffset;
                    if (desiredOffset < 0)
                        desiredOffset = 0;

                    if (desiredOffset == _scrollOffset)
                    {
                        _leftArrowVisible = leftVisible;
                        _rightArrowVisible = rightVisible;
                        _tabViewportWidth = viewport;
                        break;
                    }

                    _scrollOffset = desiredOffset;
                }
            }

            int tabAreaStart = _leftArrowVisible ? arrowWidth : 0;
            if (!_leftArrowVisible && !_rightArrowVisible)
                _tabViewportWidth = w;
            _tabViewportWidth = Math.Max(0, _tabViewportWidth);

            SDL.SDL_Rect clipRect = new SDL.SDL_Rect { x = tabAreaStart, y = 0, w = _tabViewportWidth, h = _tabHeight };
            bool useClip = (_leftArrowVisible || _rightArrowVisible) && clipRect.w > 0;
            SDL.SDL_Rect previousClip = default;
            SDL.SDL_bool clipWasEnabled = SDL.SDL_RenderIsClipEnabled(context.Renderer);
            if (useClip)
            {
                SDL.SDL_RenderGetClipRect(context.Renderer, out previousClip);
                SDL.SDL_RenderSetClipRect(context.Renderer, ref clipRect);
            }

            int baselineAscent = ascent;
            int textHeightMetric = textBaselineHeight;

            for (int i = 0; i < _children.Count; i++)
            {
                int tabWidth = _tabWidths[i];
                int tabOffset = i < _tabOffsets.Count ? _tabOffsets[i] : 0;
                int tabX = tabAreaStart + tabOffset - _scrollOffset;
                var fullRect = new SDL.SDL_Rect { x = tabX, y = 0, w = tabWidth, h = _tabHeight };
                SDL.SDL_Rect visibleRect = useClip ? fullRect.IntersectWith(clipRect) : fullRect;
                _tabRects[i] = visibleRect;

                if (useClip && visibleRect.w <= 0)
                    continue;

                var tab = _children[i];
                AColor bg = TabHeaderBGColor;
                AColor border = TabHeaderBorderColor;
                AColor txtCol = TextColor;
                if (i == _selectedIndex)
                {
                    bg = TabHeaderSelectedBGColor;
                    border = TabHeaderSelectedBorderColor;
                    txtCol = SelectedTextColor;
                }
                else if (i == _hoverIndex)
                {
                    bg = TabHeaderHoverBGColor;
                    border = TabHeaderHoverBorderColor;
                }

                SDL.SDL_SetRenderDrawColor(context.Renderer, bg.R, bg.G, bg.B, bg.A);
                SDL.SDL_RenderFillRect(context.Renderer, ref fullRect);
                SDL.SDL_SetRenderDrawColor(context.Renderer, border.R, border.G, border.B, border.A);
                SDL.SDL_RenderDrawRect(context.Renderer, ref fullRect);

                int textW = textWidths[i];
                int textH = textHeights[i];
                int baseline = fullRect.y + (_tabHeight - textHeightMetric) / 2 + baselineAscent;
                int tx = fullRect.x + (tabWidth - textW) / 2;
                int ty = baseline - baselineAscent;
                var sdlTxtCol = new SDL.SDL_Color { r = txtCol.R, g = txtCol.G, b = txtCol.B, a = txtCol.A };
                nint textSurf = SDL_ttf.TTF_RenderUTF8_Blended(_font!.FontHandle, tab.Title, sdlTxtCol);
                nint textTex = SDL.SDL_CreateTextureFromSurface(context.Renderer, textSurf);
                SDL.SDL_FreeSurface(textSurf);
                SDL.SDL_SetTextureBlendMode(textTex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                var dst = new SDL.SDL_Rect { x = tx, y = ty, w = textW, h = textH };
                SDL.SDL_RenderCopy(context.Renderer, textTex, IntPtr.Zero, ref dst);
                SDL.SDL_DestroyTexture(textTex);
            }

            if (useClip)
            {
                if (clipWasEnabled == SDL.SDL_bool.SDL_TRUE)
                    SDL.SDL_RenderSetClipRect(context.Renderer, ref previousClip);
                else
                    SDL.SDL_RenderSetClipRect(context.Renderer, IntPtr.Zero);
            }

            if (_leftArrowVisible)
            {
                _leftArrowRect = new SDL.SDL_Rect { x = 0, y = 0, w = arrowWidth, h = _tabHeight };
                DrawScrollArrow(context.Renderer, _leftArrowRect, true);
            }
            else
            {
                _leftArrowRect = new SDL.SDL_Rect { x = 0, y = 0, w = 0, h = 0 };
            }

            if (_rightArrowVisible)
            {
                _rightArrowRect = new SDL.SDL_Rect { x = Math.Max(0, w - arrowWidth), y = 0, w = arrowWidth, h = _tabHeight };
                DrawScrollArrow(context.Renderer, _rightArrowRect, false);
            }
            else
            {
                _rightArrowRect = new SDL.SDL_Rect { x = Math.Max(0, w - arrowWidth), y = 0, w = 0, h = 0 };
            }

            if (_selectedIndex >= 0 && _selectedIndex < _children.Count)
            {
                var tab = _children[_selectedIndex];
                if (tab.Content?.FrameworkObj.FrameworkNode is AbstSdlComponent comp)
                {
                    int targetWidth = Math.Max(0, w - BorderThickness * 2);
                    int targetHeight = Math.Max(0, h - _tabHeight - BorderThickness * 2);
                    var tabComponent = (AbstSdlTabItem)tab.FrameworkNode;

                    bool adjustWidth = comp.Width <= 0;
                    bool adjustHeight = comp.Height <= 0;
                    if (_tabContentSizes.TryGetValue(tabComponent, out var previousSize))
                    {
                        adjustWidth |= Math.Abs(comp.Width - previousSize.Width) < 0.1f;
                        adjustHeight |= Math.Abs(comp.Height - previousSize.Height) < 0.1f;
                    }
                    else
                    {
                        adjustWidth = true;
                        adjustHeight = true;
                    }

                    if (adjustWidth)
                        comp.Width = targetWidth;
                    if (adjustHeight)
                        comp.Height = targetHeight;

                    _tabContentSizes[tabComponent] = (comp.Width, comp.Height);

                    var ctx = comp.ComponentContext;
                    var oldOffX = ctx.OffsetX;
                    var oldOffY = ctx.OffsetY;
                    ctx.OffsetX += BorderThickness;
                    ctx.OffsetY += _tabHeight + BorderThickness;
                    ctx.RenderToTexture(context);
                    ctx.OffsetX = oldOffX;
                    ctx.OffsetY = oldOffY;
                }
            }

            if (_hoverIndex >= 0 && (_hoverIndex >= _tabRects.Count || _tabRects[_hoverIndex].w <= 0))
                _hoverIndex = -1;

            SDL.SDL_SetRenderTarget(context.Renderer, prevTarget);
            _texW = w;
            _texH = h;
            return _texture;
        }

        private void DrawScrollArrow(nint renderer, SDL.SDL_Rect rect, bool left)
        {
            if (rect.w <= 0 || rect.h <= 0)
                return;

            SDL.SDL_SetRenderDrawColor(renderer, TabHeaderBGColor.R, TabHeaderBGColor.G, TabHeaderBGColor.B, TabHeaderBGColor.A);
            SDL.SDL_RenderFillRect(renderer, ref rect);
            SDL.SDL_SetRenderDrawColor(renderer, TabHeaderBorderColor.R, TabHeaderBorderColor.G, TabHeaderBorderColor.B, TabHeaderBorderColor.A);
            SDL.SDL_RenderDrawRect(renderer, ref rect);

            int margin = Math.Max(3, Math.Min(rect.w, rect.h) / 4);
            int topY = rect.y + margin;
            int bottomY = rect.y + rect.h - margin;
            int leftX = rect.x + margin;
            int rightX = rect.x + rect.w - margin;
            if (bottomY <= topY)
                bottomY = rect.y + rect.h;
            if (rightX <= leftX)
                rightX = rect.x + rect.w;

            SDL.SDL_SetRenderDrawColor(renderer, TextColor.R, TextColor.G, TextColor.B, TextColor.A);
            int span = Math.Max(1, bottomY - topY);
            if (left)
            {
                for (int row = 0; row <= span; row++)
                {
                    int y = topY + row;
                    int start = rightX - row * (rightX - leftX) / span;
                    SDL.SDL_RenderDrawLine(renderer, start, y, rightX, y);
                }
            }
            else
            {
                for (int row = 0; row <= span; row++)
                {
                    int y = topY + row;
                    int end = leftX + row * (rightX - leftX) / span;
                    SDL.SDL_RenderDrawLine(renderer, leftX, y, end, y);
                }
            }
        }

        private void ScrollTabs(int direction)
        {
            if (direction == 0 || _tabViewportWidth <= 0 || _tabOffsets.Count == 0)
                return;

            int maxOffset = Math.Max(0, _totalTabWidth - _tabViewportWidth);
            if (direction < 0)
            {
                int target = 0;
                for (int i = _tabOffsets.Count - 1; i >= 0; i--)
                {
                    int start = _tabOffsets[i];
                    if (start < _scrollOffset - 1)
                    {
                        target = start;
                        break;
                    }
                }
                if (target != _scrollOffset)
                {
                    _scrollOffset = Math.Max(0, Math.Min(target, maxOffset));
                    _hoverIndex = -1;
                    _texture = nint.Zero;
                    ComponentContext.QueueRedraw(this);
                }
            }
            else
            {
                int viewportEnd = _scrollOffset + _tabViewportWidth;
                int target = _scrollOffset;
                int candidate = -1;
                for (int i = 0; i < _tabOffsets.Count; i++)
                {
                    int start = _tabOffsets[i];
                    int end = start + _tabWidths[i];
                    if (end > viewportEnd + 1)
                    {
                        candidate = start;
                        break;
                    }
                }
                if (candidate < 0)
                    candidate = maxOffset;
                target = Math.Max(0, Math.Min(candidate, maxOffset));
                if (target != _scrollOffset)
                {
                    _scrollOffset = target;
                    _hoverIndex = -1;
                    _texture = nint.Zero;
                    ComponentContext.QueueRedraw(this);
                }
            }
        }

        public virtual bool CanHandleEvent(AbstSDLEvent e)
        {
            return Enabled && (e.IsInside || !e.HasCoordinates);
        }
        public void HandleEvent(AbstSDLEvent e)
        {
            var ev = e.Event;
            float localX = e.ComponentLeft;
            float localY = e.ComponentTop;
            bool skipChildHandling = false;
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN when ev.button.button == SDL.SDL_BUTTON_LEFT:
                    bool inHeader = e.HasCoordinates && localY >= 0 && localY <= _tabHeight;
                    if (inHeader)
                    {
                        if (_leftArrowVisible && _leftArrowRect.ContainsPoint(localX, localY))
                        {
                            ScrollTabs(-1);
                            e.StopPropagation = true;
                            skipChildHandling = true;
                            break;
                        }
                        if (_rightArrowVisible && _rightArrowRect.ContainsPoint(localX, localY))
                        {
                            ScrollTabs(1);
                            e.StopPropagation = true;
                            skipChildHandling = true;
                            break;
                        }
                        for (int i = 0; i < _tabRects.Count; i++)
                        {
                            var r = _tabRects[i];
                            if (r.w <= 0 || r.h <= 0)
                                continue;
                            if (localX >= r.x && localX <= r.x + r.w &&
                                localY >= r.y && localY <= r.y + r.h)
                            {
                                if (_selectedIndex != i)
                                {
                                    _selectedIndex = i;
                                    _texture = nint.Zero;
                                    ComponentContext.QueueRedraw(this);
                                }
                                Factory.FocusManager.SetFocus(this);
                                e.StopPropagation = true;
                                skipChildHandling = true;
                                break;
                            }
                        }

                        if (!skipChildHandling)
                            skipChildHandling = true;
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    int hoverIndex = -1;
                    bool hoverInHeader = e.HasCoordinates && localY >= 0 && localY <= _tabHeight;
                    if (hoverInHeader &&
                        !((_leftArrowVisible && _leftArrowRect.ContainsPoint(localX, localY)) ||
                          (_rightArrowVisible && _rightArrowRect.ContainsPoint(localX, localY))))
                    {
                        for (int i = 0; i < _tabRects.Count; i++)
                        {
                            var r = _tabRects[i];
                            if (r.w <= 0 || r.h <= 0)
                                continue;
                            if (localX >= r.x && localX <= r.x + r.w &&
                                localY >= r.y && localY <= r.y + r.h)
                            {
                                hoverIndex = i;
                                break;
                            }
                        }
                    }

                    if (hoverIndex != _hoverIndex)
                    {
                        _hoverIndex = hoverIndex;
                        _texture = nint.Zero;
                        ComponentContext.QueueRedraw(this);
                    }

                    if (hoverInHeader)
                        skipChildHandling = true;
                    break;
                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    bool wheelInHeader = e.HasCoordinates && localY >= 0 && localY <= _tabHeight;
                    if (wheelInHeader)
                    {
                        if (_leftArrowVisible || _rightArrowVisible)
                        {
                            if (ev.wheel.y > 0 || ev.wheel.x < 0)
                                ScrollTabs(-1);
                            else if (ev.wheel.y < 0 || ev.wheel.x > 0)
                                ScrollTabs(1);
                            if (ev.wheel.y != 0 || ev.wheel.x != 0)
                                e.StopPropagation = true;
                        }

                        skipChildHandling = true;
                    }
                    break;
            }

            // Forward mouse events to children accounting for current scroll offset
            if (!skipChildHandling && _selectedIndex >= 0)
            {
                var tabItem = (AbstSdlTabItem)_children[_selectedIndex].FrameworkNode;
                if (!tabItem.Visibility) return;
                var oriX = e.OffsetX;
                var oriY = e.OffsetY;
                e.OffsetX += - BorderThickness;
                e.OffsetY += -(_tabHeight + BorderThickness);
                //if (e.Event.type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
                //{

                //}
                tabItem.HandleEvent(e);
                e.OffsetX = oriX;
                e.OffsetY = oriY;
            }
        }




        public override void Dispose()
        {
            ClearTabs();
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
            _font?.Release();
            base.Dispose();
        }
    }

    public class AbstSdlTabItem : AbstSdlComponent, IAbstFrameworkTabItem, IFrameworkFor<AbstTabItem>, IHandleSdlEvent
    {
        private IAbstNode? _content;

        public AbstSdlTabItem(AbstSdlComponentFactory factory, AbstTabItem tab) : base(factory)
        {
            tab.Init(this);
        }

        public string Title { get; set; } = string.Empty;
        public AMargin Margin { get; set; } = AMargin.Zero;
        public IAbstNode? Content
        {
            get => _content;
            set
            {
                if (ReferenceEquals(_content, value))
                    return;

                if (_content?.FrameworkObj.FrameworkNode is AbstSdlComponent oldComponent)
                    oldComponent.ComponentContext.SetParents(null);

                _content = value;

                if (_content?.FrameworkObj.FrameworkNode is AbstSdlComponent newComponent)
                    newComponent.ComponentContext.SetParents(ComponentContext);

                ComponentContext.QueueRedraw(this);
            }
        }
        public float TopHeight { get; set; }
        public object FrameworkNode => this;

        public override void Dispose()
        {
            Content = null;
            base.Dispose();
        }

        public void HandleEvent(AbstSDLEvent e)
        {
            if (Content?.FrameworkObj is IHandleSdlEvent handleEvent)
                handleEvent.HandleEvent(e);
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context) => default;
    }
}
