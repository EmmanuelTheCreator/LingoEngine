using Godot;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Events;
using LingoEngine.Core;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.LGodot;
using LingoEngine.Director.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Simple timeline overlay showing the Score channels and frames.
/// Toggled with F2.
/// </summary>
public partial class DirGodotScoreWindow : BaseGodotWindow, IDirFrameworkScoreWindow
{
   
    
    private int _footerMargin= 10;

    private bool wasToggleKey;
    private LingoMovie? _movie;
    private readonly ScrollContainer _vClipper = new ScrollContainer();
    private readonly ScrollContainer _masterScroller = new ScrollContainer();
    private readonly Control _topStripContent = new Control();
    private readonly Control _scrollContent = new Control();
    private ColorRect _hClipper;
    private ILingoPlayer _player;
    private readonly DirGodotScoreGfxValues _gfxValues = new();
    private readonly DirGodotScoreGrid _grid;
    private readonly DirGodotFrameHeader _header;
    private readonly DirGodotFrameScriptsBar _frameScripts;
    private readonly DirGodotScoreLabelsBar _labelBar;
    private readonly DirGodotScoreChannelBar _channelBar;

    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoCommandManager _commandManager;


    public DirGodotScoreWindow(IDirectorEventMediator directorMediator, ILingoCommandManager commandManager, DirectorScoreWindow directorScoreWindow, ILingoPlayer player, IDirGodotWindowManager windowManager)
        : base(DirectorMenuCodes.ScoreWindow, "Score", windowManager)
    {
        _mediator = directorMediator;
        _commandManager = commandManager;
        directorScoreWindow.Init(this);
        _player = player;
        _player.ActiveMovieChanged += OnActiveMovieChanged;


        var height = 400;
        var width = 800;

        AddChild(new DirGodotCastLeftTopLabels(_gfxValues));


        Size = new Vector2(width, height);
        CustomMinimumSize = Size;
        _channelBar = new DirGodotScoreChannelBar(_gfxValues);
        _grid = new DirGodotScoreGrid(directorMediator, _gfxValues);
        _mediator.Subscribe(_grid);
        _header = new DirGodotFrameHeader(_gfxValues);
        _frameScripts = new DirGodotFrameScriptsBar(_gfxValues);
        _labelBar = new DirGodotScoreLabelsBar(_gfxValues, commandManager);
        

        // The grid inside master scoller
        _masterScroller.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _masterScroller.VerticalScrollMode= ScrollContainer.ScrollMode.ShowAlways;
        _masterScroller.Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, Size.Y - _gfxValues.TopStripHeight- _footerMargin);
        _masterScroller.Position = new Vector2(_gfxValues.ChannelInfoWidth, _gfxValues.TopStripHeight);
        
        _masterScroller.AddChild(_scrollContent);
        AddChild(_masterScroller);

        _scrollContent.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _scrollContent.SizeFlagsVertical = SizeFlags.ExpandFill;
        _scrollContent.AddChild(_grid);

        _grid.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _grid.SizeFlagsVertical = SizeFlags.ExpandFill;
        _grid.Resized += UpdateScrollSize;

        // The top strip with clipper
        _hClipper = new ColorRect
        {
            Color = new Color(0, 0, 0, 0),
            Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, _gfxValues.TopStripHeight),
            Position = new Vector2(_gfxValues.ChannelInfoWidth, TitleBarHeight),
            ClipContents = true
        };
        _topStripContent.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        _topStripContent.SizeFlagsVertical = Control.SizeFlags.Fill;
        _hClipper.AddChild(_topStripContent);
        AddChild(_hClipper);
        _topStripContent.AddChild(_labelBar);
        _topStripContent.AddChild(_frameScripts);
        _topStripContent.AddChild(_header);




        // the vertical channel sprite numbers with visibility
        _channelBar.Position = new Vector2(0, _gfxValues.TopStripHeight - _footerMargin);
        _channelBar.Size = new Vector2(_gfxValues.ChannelInfoWidth, Size.Y - _gfxValues.TopStripHeight - _footerMargin);

        _vClipper.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        _vClipper.VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever;
        _vClipper.Position = new Vector2(0, _gfxValues.TopStripHeight);
        _vClipper.Size = new Vector2(_gfxValues.ChannelInfoWidth, Size.Y - _gfxValues.TopStripHeight - _footerMargin);
        _vClipper.ClipContents = true;
        _vClipper.AddChild(_channelBar);
        AddChild(_vClipper); 

       
        _labelBar.Position = new Vector2(0, 0);
        _frameScripts.Position = new Vector2(0, 20);
        _header.Position = new Vector2(0, 40);
        


        UpdateScrollSize();
    }
    public override void _Process(double delta)
    {
        if (!Visible) return;
        _channelBar.Position = new Vector2(0, -_masterScroller.ScrollVertical);
        _topStripContent.Position = new Vector2(-_masterScroller.ScrollHorizontal, _topStripContent.Position.Y);
    }

    private void UpdateScrollSize()
    {
        if (_movie == null) return;

        float gridWidth = _gfxValues.ChannelInfoWidth + _movie.FrameCount * _gfxValues.FrameWidth + _gfxValues.ExtraMargin;
        float gridHeight = _movie.MaxSpriteChannelCount * _gfxValues.ChannelHeight + _gfxValues.ExtraMargin;

        _channelBar.CustomMinimumSize = new Vector2(_gfxValues.ChannelInfoWidth, gridHeight - _footerMargin);
        _scrollContent.CustomMinimumSize = new Vector2(gridWidth, gridHeight - _footerMargin);
        _topStripContent.CustomMinimumSize = new Vector2(gridWidth, _gfxValues.TopStripHeight);

        _vClipper.Size = new Vector2(_gfxValues.ChannelInfoWidth, Size.Y - _gfxValues.TopStripHeight - _footerMargin);
        _hClipper.Size = new Vector2(Size.X- _gfxValues.ChannelInfoWidth, _gfxValues.TopStripHeight);
        _masterScroller.Size = new Vector2(Size.X- _gfxValues.ChannelInfoWidth, Size.Y - _gfxValues.TopStripHeight - _footerMargin);
    }


    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        UpdateScrollSize();
    }

    private void OnActiveMovieChanged(ILingoMovie? movie)
    {
        SetActiveMovie(movie as LingoMovie);
    }
    public void SetActiveMovie(LingoMovie? movie)
    {
        _movie = movie;
        _grid.SetMovie(movie);
        _header.SetMovie(movie);
        _frameScripts.SetMovie(movie);
        _channelBar.SetMovie(movie);
        _labelBar.SetMovie(movie);

        UpdateScrollSize();
    }

    

    protected override void Dispose(bool disposing)
    {
        _player.ActiveMovieChanged -= OnActiveMovieChanged;
        _grid.Dispose();
        _labelBar.Dispose();
        _frameScripts.Dispose();
        _masterScroller.Dispose();
        //_hScroller.Dispose();
        _channelBar.Dispose();
        _mediator.Unsubscribe(_grid);
        base.Dispose(disposing);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && !mb.IsPressed())
        {
            if (mb.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
            {
                var mousePos = GetGlobalMousePosition();
                Rect2 bounds = new Rect2(_masterScroller.GlobalPosition, _masterScroller.Size);
                if (bounds.HasPoint(mousePos))
                {
                    if (mb.ButtonIndex == MouseButton.WheelUp)
                        _masterScroller.ScrollVertical -= 20;
                    else
                        _masterScroller.ScrollVertical += 20;
                }
            }
        }
    }

 

    internal partial class DirGodotCastLeftTopLabels : Control
    {
        private DirGodotScoreGfxValues _gfxValues;

        public DirGodotCastLeftTopLabels(DirGodotScoreGfxValues gfxValues)
        {
            _gfxValues = gfxValues;
            Size = new Vector2(gfxValues.ChannelLabelWidth + gfxValues.ChannelHeight, gfxValues.TopStripHeight - 20);
            Position = new Vector2(0, 20);
        }
        public override void _Draw()
        {
            
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), new Color("#f0f0f0"));
            DrawTextWithLine(0,20, "Labels");
            DrawTextWithLine(20,20, "Scripts");
            DrawTextWithLine(37,23, "Member", false);
        }
        private void DrawTextWithLine(int top, int height, string text, bool withTopLines = false)
        {
            var font = ThemeDB.FallbackFont;
            
            DrawString(font, new Vector2(5, top+ font.GetAscent()-3), text, HorizontalAlignment.Left, -1, 10, new Color("#666666"));
            if (withTopLines)
            {
                DrawLines(top);
               
            }
            DrawLine(new Vector2(0, top + height), new Vector2(Size.X, top + height), _gfxValues.ColLineDark);
            DrawLine(new Vector2(0, top + height + 1), new Vector2(Size.X, top + height + 1), _gfxValues.ColLineLight);
        }
        private void DrawLines(int top)
        {
            DrawLine(new Vector2(0, top), new Vector2(Size.X, top), _gfxValues.ColLineDark);
            DrawLine(new Vector2(0, top + 1), new Vector2(Size.X, top + 1), _gfxValues.ColLineLight);
        }

      
    }
}
