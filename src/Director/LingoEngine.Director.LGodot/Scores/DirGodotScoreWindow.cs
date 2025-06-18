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
    const int ChannelHeight = 16;
    const int FrameWidth = 9;
    const int ChannelLabelWidth = 54;
    const int ChannelInfoWidth = ChannelHeight + ChannelLabelWidth;
    const int ExtraMargin = 20;

    private bool wasToggleKey;
    private LingoMovie? _movie;
    private readonly ScrollContainer _hScroller = new ScrollContainer();
    private readonly ScrollContainer _vScroller = new ScrollContainer();
    private readonly ScrollContainer _vClipper = new ScrollContainer();
    private readonly Control _scrollContent = new Control();
    private readonly DirGodotScoreGrid _grid;
    private readonly DirGodotFrameHeader _header;
    private readonly DirGodotFrameScriptsBar _frameScripts;
    private readonly DirGodotScoreLabelsBar _labelBar;
    private readonly DirGodotScoreChannelBar _channelBar = new DirGodotScoreChannelBar();


    private readonly Button _playButton = new Button();
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
        _grid = new DirGodotScoreGrid(directorMediator);
        _mediator.Subscribe(_grid);
        _header = new DirGodotFrameHeader();
        _frameScripts = new DirGodotFrameScriptsBar();
        _labelBar = new DirGodotScoreLabelsBar();
        _channelBar.Position = new Vector2(0, 60);
        


        AddChild(_hScroller);
        _hScroller.VerticalScrollMode = ScrollContainer.ScrollMode.Disabled;
        _hScroller.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _hScroller.ZIndex = 100;
        _hScroller.Size = new Vector2(Size.X- 40, Size.Y-40);
        _hScroller.Position = new Vector2(0, 20);
        _hScroller.AddChild(_scrollContent);

        _scrollContent.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _scrollContent.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _scrollContent.AddChild(_labelBar);
        _scrollContent.AddChild(_frameScripts);
        _scrollContent.AddChild(_header);
        _scrollContent.AddChild(_vScroller);
        _scrollContent.AddChild(_vClipper);
        _vClipper.AddChild(_channelBar);


        _vClipper.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        _vClipper.VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever;
        _vClipper.Size = new Vector2(ChannelInfoWidth, Size.Y - 90);
        _vClipper.Position = new Vector2(0, 60);
        _vClipper.ClipContents = true;



        _vScroller.VerticalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _vScroller.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        _vScroller.ZIndex = 100;


        _vScroller.AddChild(_grid);
        _grid.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _grid.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _grid.Resized += UpdateScrollSize;
        _vScroller.Size = new Vector2(Size.X - 10, Size.Y- 90);
        _vScroller.Position = new Vector2(0, 60);

        _labelBar.Position = new Vector2(0, 0);
        _frameScripts.Position = new Vector2(0, 20);
        _header.Position = new Vector2(0, 40);
        
        UpdateScrollSize();
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Visible)
        {
            _channelBar.Position = new Vector2(0, -_vScroller.ScrollVertical);
        }
    }



    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _hScroller.Size = new Vector2(size.X - 40, size.Y - 40);
        _vScroller.Size = new Vector2(size.X - 40, size.Y - 90);
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

    private void UpdateScrollSize()
    {
        if (_movie == null) return;

      

        float gridWidth = ChannelInfoWidth + _movie.FrameCount * FrameWidth + ExtraMargin;
        float gridHeight = _movie.MaxSpriteChannelCount * ChannelHeight + ExtraMargin;
        _vScroller.Position = new Vector2(ChannelInfoWidth, 60);
        //_vScroller.Size = new Vector2(Size.X - ChannelInfoWidth - 10, Size.Y - 90);
        _vClipper.Size = new Vector2(ChannelInfoWidth, Size.Y - 90);
        _channelBar.CustomMinimumSize = new Vector2(ChannelInfoWidth, gridHeight);
        _scrollContent.CustomMinimumSize = new Vector2(gridWidth+20, 60 + gridHeight+60);
    }


    protected override void Dispose(bool disposing)
    {
        _grid.Dispose();
        _labelBar.Dispose();
        _frameScripts.Dispose();
        _vScroller.Dispose();
        _hScroller.Dispose();
        _channelBar.Dispose();
        _mediator.Unsubscribe(_grid);
        base.Dispose(disposing);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && !mb.IsPressed())
        {
            if (mb.ButtonIndex == MouseButton.WheelUp)
                _vScroller.ScrollVertical -= 20;
            else if (mb.ButtonIndex == MouseButton.WheelDown)
                _vScroller.ScrollVertical += 20;
        }
    }

}
