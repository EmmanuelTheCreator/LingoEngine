using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Components.Graphics;
using AbstUI.FrameworkCommunication;
using AbstUI.Primitives;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Containers;
using AbstUI.SDL2.Components.Graphics;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Windowing;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Scores;
using BlingoEngine.Director.Core.Sprites;
using BlingoEngine.Director.Core.Stages;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Movies;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace BlingoEngine.Director.SDL2.Scores;

/// <summary>
/// SDL2 implementation of the Director Score window.
/// </summary>
internal class DirSdlScoreWindow : AbstSdlWindow, IDirFrameworkScoreWindow, IDisposable, IFrameworkFor<DirectorScoreWindow>
{
    private readonly DirectorScoreWindow _directorScoreWindow;
    private readonly BlingoPlayer _player;
    private readonly DirScoreGfxValues _gfxValues;
    private readonly AbstPanel _marginPanel;
    private readonly AbstScrollContainer _masterScroller;
    private readonly AbstPanel _scrollContent;
    private readonly AbstScrollContainer _leftChannelsScroller;
    private readonly AbstPanel _topStripContent;
    private readonly AbstPanel _topStrip;
    private readonly TopChannelsContainer _topContainer;
    private readonly Sprites2DChannelsContainer _sprites2DContainer;
    private readonly AbstGfxCanvas _frameHeaderCanvas;
    private readonly AbstPanel _labelsPanel;
    private readonly AbstPanel _leftScrollPanel;
    private readonly AbstPanel _leftFixPanel;
    private float _lastScrollH;
    private float _lastScrollV = -1f;
    private bool _topCollapsed = true;
    private BlingoMovie? _movie;
    private const int TitleBarHeight = 24;
    private const int FooterMargin = 10;

    public DirSdlScoreWindow(DirectorScoreWindow directorScoreWindow, IBlingoPlayer player, IServiceProvider services)
        : base((AbstSdlComponentFactory)services.GetRequiredService<IAbstComponentFactory>())
    {
        _directorScoreWindow = directorScoreWindow;
        _player = (BlingoPlayer)player;
        _player.ActiveMovieChanged += OnActiveMovieChanged;
        _gfxValues = directorScoreWindow.GfxValues;
        Init(directorScoreWindow);

        var factory = (AbstSdlComponentFactory)services.GetRequiredService<IAbstComponentFactory>();

        // Margin panel to offset below title bar
        _marginPanel = factory.CreatePanel("ScoreMarginPanel");
        _marginPanel.X = 0;
        _marginPanel.Y = TitleBarHeight;
        AddItem(_marginPanel.Framework<AbstSdlPanel>());

        // Master scroll container for main grid
        _masterScroller = factory.CreateScrollContainer("ScoreMasterScroller");
        _masterScroller.ScollbarModeH = AbstScrollbarMode.Auto;
        _masterScroller.ScollbarModeV = AbstScrollbarMode.Auto;
        _scrollContent = factory.CreatePanel("ScoreScrollContent");
        _masterScroller.AddItem(_scrollContent);
        _marginPanel.AddItem(_masterScroller);

        // Left channel numbers scroller
        _leftChannelsScroller = factory.CreateScrollContainer("ScoreLeftChannelsScroller");
        _leftChannelsScroller.ScollbarModeH = AbstScrollbarMode.Hidden;
        _marginPanel.AddItem(_leftChannelsScroller);

        // Top strip containing labels and frame header
        _topStrip = factory.CreatePanel("ScoreTopStrip");
        _topStrip.Y = 0;
        _marginPanel.AddItem(_topStrip);

        _topStripContent = factory.CreatePanel("ScoreTopStripContent");
        _topStrip.AddItem(_topStripContent);

        // Director core panels/canvases
        _frameHeaderCanvas = directorScoreWindow.FrameHeader.Canvas;
        _labelsPanel = directorScoreWindow.LabelsBar.ScollingPanel;
        _topStripContent.AddItem(_frameHeaderCanvas);
        _topStripContent.AddItem(_labelsPanel);

        _topContainer = new TopChannelsContainer(directorScoreWindow.TopContainer, factory);
        _topContainer.Y = _gfxValues.LabelsBarHeight + 1;
        _topStrip.Framework<AbstSdlPanel>().AddItem(_topContainer);

        _sprites2DContainer = new Sprites2DChannelsContainer(directorScoreWindow.Sprites2DContainer, factory);
        _scrollContent.Framework<AbstSdlPanel>().AddItem(_sprites2DContainer);

        _leftScrollPanel = directorScoreWindow.ScorePanelScroll;
        _leftChannelsScroller.AddItem(_leftScrollPanel);
        _leftFixPanel = directorScoreWindow.ScorePanelFix;
        _marginPanel.AddItem(_leftFixPanel);

        _directorScoreWindow.HeaderCollapseChanged += OnHeaderCollapseChanged;

        Width = directorScoreWindow.Width;
        Height = directorScoreWindow.Height;
        _directorScoreWindow.ResizingContentFromFW(true, (int)Width, (int)Height - TitleBarHeight);
        RepositionBars(true);
    }

    private void OnActiveMovieChanged(IBlingoMovie? movie)
    {
        SetActiveMovie(movie as BlingoMovie);
    }

    private void SetActiveMovie(BlingoMovie? movie)
    {
        _movie = movie;
        RepositionBars();
    }

    private void OnHeaderCollapseChanged(bool collapsed)
    {
        _topCollapsed = collapsed;
        RepositionBars();
    }

    private void RepositionBars(bool firstLoad = false)
    {
        float barsHeight = _topCollapsed ? _gfxValues.ChannelHeight : _gfxValues.ChannelHeight * 6;
        _frameHeaderCanvas.X = 0;
        _frameHeaderCanvas.Y = barsHeight + _gfxValues.LabelsBarHeight + 2;
        float topHeight = barsHeight + _gfxValues.ChannelFramesHeight + 22;

        _topStrip.X = _gfxValues.ChannelInfoWidth;
        _topStrip.Width = Width - _gfxValues.ChannelInfoWidth;
        _topStrip.Height = topHeight + 20;

        _masterScroller.X = _gfxValues.ChannelInfoWidth;
        _masterScroller.Y = topHeight;
        _masterScroller.Width = Width - _gfxValues.ChannelInfoWidth;
        _masterScroller.Height = Height - topHeight - FooterMargin;
        _directorScoreWindow.UpdateViewportSize(_masterScroller.Width, _masterScroller.Height);

        _leftChannelsScroller.X = 0;
        _leftChannelsScroller.Y = topHeight;
        _leftChannelsScroller.Width = _gfxValues.ChannelInfoWidth;
        _leftChannelsScroller.Height = _masterScroller.Height;

        UpdateScrollSize();
        _directorScoreWindow.ResizingContentFromFW(firstLoad, (int)Width, (int)Height - TitleBarHeight);
    }

    private void UpdateScrollSize()
    {
        if (_movie == null) return;
        float gridWidth = _gfxValues.ChannelInfoWidth + _movie.FrameCount * _gfxValues.FrameWidth + _gfxValues.ExtraMargin;
        float gridHeight = _movie.MaxSpriteChannelCount * _gfxValues.ChannelHeight + _gfxValues.ExtraMargin;
        float topHeight = _frameHeaderCanvas.Y + 20;

        _scrollContent.Width = gridWidth;
        _scrollContent.Height = gridHeight - FooterMargin;
        _topStripContent.Width = gridWidth;
        _topStripContent.Height = topHeight + 20;
    }

    public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        var result = base.Render(context);
        UpdateScroll();
        return result;
    }

    private void UpdateScroll()
    {
        if (_lastScrollH != _masterScroller.ScrollHorizontal || _lastScrollV != _masterScroller.ScrollVertical)
        {
            _lastScrollH = _masterScroller.ScrollHorizontal;
            _lastScrollV = _masterScroller.ScrollVertical;
            _directorScoreWindow.ScollX = _lastScrollH;
            _directorScoreWindow.ScollY = _lastScrollV;
            _directorScoreWindow.ScollVPositionChanged();
            _topStripContent.X = -_lastScrollH;
            _topContainer.ScrollX = _lastScrollH;
            _leftChannelsScroller.ScrollVertical = _lastScrollV;
        }
    }

    public void ScrollTo(float horizontal, float vertical)
    {
        _masterScroller.ScrollHorizontal = horizontal;
        _masterScroller.ScrollVertical = vertical;
        UpdateScroll();
    }

    public new void Dispose()
    {
        _player.ActiveMovieChanged -= OnActiveMovieChanged;
        _directorScoreWindow.HeaderCollapseChanged -= OnHeaderCollapseChanged;
        _topContainer.Dispose();
        _sprites2DContainer.Dispose();
        base.Dispose();
    }

    #region Container helpers

    private partial class TopChannelsContainer : ChannelsContainer<DirScoreGridTopContainer>
    {
        public TopChannelsContainer(DirScoreGridTopContainer top, AbstSdlComponentFactory factory) : base(top, factory) { }
    }
    private partial class Sprites2DChannelsContainer : ChannelsContainer<DirScoreGridSprites2DContainer>
    {
        public Sprites2DChannelsContainer(DirScoreGridSprites2DContainer cont, AbstSdlComponentFactory factory) : base(cont, factory) { }
    }

    private abstract partial class ChannelsContainer<TDirScoreGridContainer> : AbstSdlPanel, IDirScoreFrameworkGridContainer
        where TDirScoreGridContainer : DirScoreGridContainer
    {
        protected readonly TDirScoreGridContainer _DirScoreContainer;
        private readonly AbstSdlGfxCanvas _gridLines;
        private readonly AbstSdlGfxCanvas _currentFrame;
        private readonly List<ContainerChannel> _channels = new();
        private float _scrollX;

        public float CurrentFrameX { get => _currentFrame.X; set => _currentFrame.X = value; }

        protected ChannelsContainer(TDirScoreGridContainer container, AbstSdlComponentFactory factory) : base(factory)
        {
            _DirScoreContainer = container;
            _DirScoreContainer.Init(this);
            _gridLines = _DirScoreContainer.CanvasGridLines.Framework<AbstSdlGfxCanvas>();
            _currentFrame = _DirScoreContainer.CanvasCurrentFrame.Framework<AbstSdlGfxCanvas>();
            AddItem(_gridLines);
            CreateChannels();
        }

        public void RequireRecreateChannels() => CreateChannels();

        private void CreateChannels()
        {
            foreach (var ch in _channels)
                ch.Dispose();
            _channels.Clear();
            foreach (var ch in _DirScoreContainer.Channels)
            {
                var channel = new ContainerChannel(ch, Factory);
                _channels.Add(channel);
                AddItem(channel);
            }
            AddItem(_currentFrame);
        }

        public void RequireRedrawChannels()
        {
            foreach (var ch in _channels)
                ch.RequireSetPosAndSize();
            CurrentFrameX = _DirScoreContainer.CurrentFrameX;
        }

        public void UpdateSize()
        {
            if (_DirScoreContainer.Size.X == 0) return;
            Width = _DirScoreContainer.Size.X;
            Height = _DirScoreContainer.Size.Y;
            CurrentFrameX = _DirScoreContainer.CurrentFrameX;
        }

        public float ScrollX
        {
            get => _scrollX;
            set { _scrollX = value; X = -value; }
        }
    }

    private partial class ContainerChannel : AbstSdlPanel, IDirScoreChannelFramework
    {
        private readonly DirScoreChannel _directorChannel;
        private readonly AbstSdlGfxCanvas _canvas;
        private bool _isRequiringRedraw;

        public ContainerChannel(DirScoreChannel ch, AbstSdlComponentFactory factory) : base(factory)
        {
            _directorChannel = ch;
            _directorChannel.Init(this);
            _canvas = ch.Framework<AbstSdlGfxCanvas>();
            AddItem(_canvas);
            RequireSetPosAndSize();
        }

        public void RequireSetPosAndSize()
        {
            Width = _directorChannel.Size.X;
            Height = _directorChannel.Size.Y;
            X = _directorChannel.Position.X;
            Y = _directorChannel.Position.Y;
            Visibility = _directorChannel.Visible;
        }
        
        public void RequireRedraw()
        {
            if (_isRequiringRedraw) return;
            _isRequiringRedraw = true;
            _directorChannel.Draw();
            _isRequiringRedraw = false;
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            _directorChannel.Draw();
            return base.Render(context);
        }
    }
    #endregion
}

