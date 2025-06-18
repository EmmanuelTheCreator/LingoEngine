using Godot;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Simple timeline overlay showing the Score channels and frames.
/// Toggled with F2.
/// </summary>
public partial class DirGodotScoreWindow : BaseGodotWindow
{
   
    private int _topStripHeight = 80;
    private int _footerMargin= 10;

    private bool wasToggleKey;
    private LingoMovie? _movie;
    private readonly ScrollContainer _hClipper = new ScrollContainer();
    private readonly ScrollContainer _vClipper = new ScrollContainer();
    private readonly ScrollContainer _masterScroller = new ScrollContainer();
    private readonly Control _topStripContent = new Control();
    private readonly Control _scrollContent = new Control();
    private ColorRect _hClipper2;

    private readonly DirGodotScoreGfxValues _gfxValues = new();
    private readonly DirGodotScoreGrid _grid;
    private readonly DirGodotFrameHeader _header;
    private readonly DirGodotFrameScriptsBar _frameScripts;
    private readonly DirGodotScoreLabelsBar _labelBar;
    private readonly DirGodotScoreChannelBar _channelBar;
    
    private readonly IDirectorEventMediator _mediator;


    public DirGodotScoreWindow(IDirectorEventMediator directorMediator)
        : base("Score")
    {
        _mediator = directorMediator;
        _mediator.SubscribeToMenu(DirectorMenuCodes.ScoreWindow, () => Visible = !Visible);
        var height = 400;
        var width = 800;


       

        Size = new Vector2(width, height);
        CustomMinimumSize = Size;
        _channelBar = new DirGodotScoreChannelBar(_gfxValues);
        _grid = new DirGodotScoreGrid(directorMediator, _gfxValues);
        _mediator.Subscribe(_grid);
        _header = new DirGodotFrameHeader(_gfxValues);
        _frameScripts = new DirGodotFrameScriptsBar(_gfxValues);
        _labelBar = new DirGodotScoreLabelsBar(_gfxValues);
        

        // The grid inside master scoller
        _masterScroller.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _masterScroller.VerticalScrollMode= ScrollContainer.ScrollMode.ShowAlways;
        _masterScroller.Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, Size.Y - _topStripHeight- _footerMargin);
        _masterScroller.Position = new Vector2(_gfxValues.ChannelInfoWidth, _topStripHeight);
        _masterScroller.AddChild(_scrollContent);
        AddChild(_masterScroller);

        _scrollContent.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _scrollContent.SizeFlagsVertical = SizeFlags.ExpandFill;
        _scrollContent.AddChild(_grid);

        _grid.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _grid.SizeFlagsVertical = SizeFlags.ExpandFill;
        _grid.Resized += UpdateScrollSize;

        // The top strip with clipper
        _hClipper2 = new ColorRect
        {
            Color = new Color(0, 0, 0, 0),
            Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, _topStripHeight),
            Position = new Vector2(_gfxValues.ChannelInfoWidth, TitleBarHeight),
            ClipContents = true
        };
        _topStripContent.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        _topStripContent.SizeFlagsVertical = Control.SizeFlags.Fill;
        _hClipper2.AddChild(_topStripContent);
        AddChild(_hClipper2);
        //_topStripContent.AddChild(_labelBar);
        //_topStripContent.AddChild(_frameScripts);
        _topStripContent.AddChild(_header);

        //_hClipper.AddChild(_topStripContent);
        //_hClipper.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowNever;
        //_hClipper.VerticalScrollMode = ScrollContainer.ScrollMode.Disabled;
        //_hClipper.ClipContents = true;
        //_hClipper.Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, _topStripHeight - TitleBarHeight);
        //_hClipper.Position = new Vector2(_gfxValues.ChannelInfoWidth, TitleBarHeight);
        //AddChild(_hClipper);


        // the vertical channel sprite numbers with visibility
        _channelBar.Position = new Vector2(0, _topStripHeight - _footerMargin);
        _channelBar.Size = new Vector2(_gfxValues.ChannelInfoWidth, Size.Y - _topStripHeight - _footerMargin);

        _vClipper.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        _vClipper.VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever;
        _vClipper.Position = new Vector2(0, _topStripHeight);
        _vClipper.Size = new Vector2(_gfxValues.ChannelInfoWidth, Size.Y - _topStripHeight - _footerMargin);
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
        _hClipper.ScrollHorizontal = _masterScroller.ScrollHorizontal;
        _topStripContent.Position = new Vector2(-_masterScroller.ScrollHorizontal, _topStripContent.Position.Y);
    }

    private void UpdateScrollSize()
    {
        if (_movie == null) return;

        float gridWidth = _gfxValues.ChannelInfoWidth + _movie.FrameCount * _gfxValues.FrameWidth + _gfxValues.ExtraMargin;
        float gridHeight = _movie.MaxSpriteChannelCount * _gfxValues.ChannelHeight + _gfxValues.ExtraMargin;

        _channelBar.CustomMinimumSize = new Vector2(_gfxValues.ChannelInfoWidth, gridHeight - _footerMargin);
        _scrollContent.CustomMinimumSize = new Vector2(gridWidth, gridHeight - _footerMargin);
        _topStripContent.CustomMinimumSize = new Vector2(gridWidth, _topStripHeight);

        _vClipper.Size = new Vector2(_gfxValues.ChannelInfoWidth, Size.Y - _topStripHeight - _footerMargin);
        _hClipper.Size = new Vector2(Size.X- _gfxValues.ChannelInfoWidth, _topStripHeight);
    }


    //public override void _Process(double delta)
    //{
    //    base._Process(delta);
    //    if (Visible)
    //    {
    //        _channelBar.Position = new Vector2(0, -_masterScroller.ScrollVertical);
    //        //_topStripContent.Position = new Vector2(-_masterScroller.ScrollHorizontal, 0);
    //        _topStripWrapper.Position = new Vector2(-_masterScroller.ScrollHorizontal, 0);

    //    }
    //}
    //private void UpdateScrollSize()
    //{
    //    if (_movie == null) return;

    //    float gridWidth = _gfxValues.ChannelInfoWidth + _movie.FrameCount * _gfxValues.FrameWidth + _gfxValues.ExtraMargin;
    //    float gridHeight = _movie.MaxSpriteChannelCount * _gfxValues.ChannelHeight + _gfxValues.ExtraMargin;

    //    _channelBar.CustomMinimumSize = new Vector2(_gfxValues.ChannelInfoWidth, gridHeight - _footerMargin);
    //    _scrollContent.CustomMinimumSize = new Vector2(gridWidth, gridHeight - _footerMargin);
    //    _topStripContent.CustomMinimumSize = new Vector2(gridWidth, _topStripHeight);
    //    _topStripWrapper.CustomMinimumSize = new Vector2(gridWidth, _topStripHeight);

    //    _vClipper.Size = new Vector2(_gfxValues.ChannelInfoWidth, Size.Y - _topStripHeight - _footerMargin);
    //}





    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        UpdateScrollSize();
    }

    public void SetMovie(LingoMovie? movie)
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
            if (mb.ButtonIndex == MouseButton.WheelUp)
                _masterScroller.ScrollVertical -= 20;
            else if (mb.ButtonIndex == MouseButton.WheelDown)
                _masterScroller.ScrollVertical += 20;
        }
    }

}
