using Godot;
using LingoEngine.Movies;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Movies;
using LingoEngine.LGodot.Stages;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Core;
using System.Linq;

namespace LingoEngine.Director.LGodot.Movies;

internal partial class DirGodotStageWindow : BaseGodotWindow, IHasSpriteSelectedEvent
{
    private const int IconBarHeight = 12;
    private readonly LingoGodotStageContainer _stageContainer;
    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoPlayer _player;
    private readonly HBoxContainer _iconBar = new HBoxContainer();
    private readonly HSlider _zoomSlider = new HSlider();
    private readonly Button _rewindButton = new Button();
    private readonly Button _playButton = new Button();
    private readonly Button _prevFrameButton = new Button();
    private readonly Button _nextFrameButton = new Button();
    private readonly ColorRect _stageBgRect = new ColorRect();
    private readonly ColorRect _colorDisplay = new ColorRect();
    private readonly ColorPickerButton _colorPicker = new ColorPickerButton();
    private readonly ScrollContainer _scrollContainer = new ScrollContainer();
    private readonly SelectionBox _selectionBox = new SelectionBox();

    private LingoMovie? _movie;
    private ILingoFrameworkStage? _stage;
    private LingoSprite? _selectedSprite;

    public DirGodotStageWindow(Node root, LingoGodotStageContainer stageContainer, IDirectorEventMediator directorEventMediator, ILingoPlayer player)
        : base("Stage")
    {
        _stageContainer = stageContainer;
        _mediator = directorEventMediator;
        _player = player;
        _player.ActiveMovieChanged += OnActiveMovieChanged;
        _mediator.Subscribe(this);
        
        Size = new Vector2(640 +10, 480+ TitleBarHeight);
        CustomMinimumSize = Size;
        // Set anchors to stretch fully
        _scrollContainer.AnchorLeft = 0;
        _scrollContainer.AnchorTop = 0;
        _scrollContainer.AnchorRight = 1;
        _scrollContainer.AnchorBottom = 1;

        // Set offsets to 0
        _scrollContainer.OffsetLeft = 0;
        _scrollContainer.OffsetTop = 0;
        _scrollContainer.OffsetRight = -10;
        _scrollContainer.OffsetBottom = -IconBarHeight - 5;
        root.AddChild(this);

        _scrollContainer.Position = new Vector2(0, 20);
        _stageBgRect.Color = Colors.Black;
        _stageBgRect.CustomMinimumSize = new Vector2(640, 480);
        _stageBgRect.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _stageBgRect.SizeFlagsVertical = SizeFlags.ExpandFill;
        _scrollContainer.AddChild(_stageBgRect);
        _scrollContainer.AddChild(stageContainer.Container);
        stageContainer.Container.AddChild(_selectionBox);
        _selectionBox.Visible = false;
        _selectionBox.ZIndex = 1000;
        AddChild(_scrollContainer);

        // bottom icon bar
        AddChild(_iconBar);
        _iconBar.Position = new Vector2(0, Size.Y - IconBarHeight);
        _iconBar.CustomMinimumSize = new Vector2(Size.X, IconBarHeight);
        _iconBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _rewindButton.Text = "|<";
        _rewindButton.CustomMinimumSize = new Vector2(20, IconBarHeight);
        _rewindButton.Pressed += () => _movie?.GoTo(1);
        _iconBar.AddChild(_rewindButton);

        _playButton.CustomMinimumSize = new Vector2(60, IconBarHeight);
        _playButton.AddThemeFontSizeOverride("font_size", 8);
        _playButton.Pressed += OnPlayPressed;
        _iconBar.AddChild(_playButton);

        _prevFrameButton.Text = "<";
        _prevFrameButton.CustomMinimumSize = new Vector2(20, IconBarHeight);
        _prevFrameButton.Pressed += () => _movie?.PrevFrame();
        _iconBar.AddChild(_prevFrameButton);

        _nextFrameButton.Text = ">";
        _nextFrameButton.CustomMinimumSize = new Vector2(20, IconBarHeight);
        _nextFrameButton.Pressed += () => _movie?.NextFrame();
        _iconBar.AddChild(_nextFrameButton);

        _zoomSlider.MinValue = 0.1f;
        _zoomSlider.MaxValue = 3f;
        _zoomSlider.Step = 0.1f;
        _zoomSlider.Value = 1f;
        _zoomSlider.CustomMinimumSize = new Vector2(100, IconBarHeight);
        _zoomSlider.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _zoomSlider.ValueChanged += value => UpdateZoom((float)value);
        _iconBar.AddChild(_zoomSlider);

        _colorDisplay.Color = Colors.Black;
        _colorDisplay.CustomMinimumSize = new Vector2(IconBarHeight, IconBarHeight);
        _iconBar.AddChild(_colorDisplay);

        _colorPicker.CustomMinimumSize = new Vector2(IconBarHeight, IconBarHeight);
        _colorPicker.Color = Colors.Black;
        _colorPicker.ColorChanged += c => OnColorChanged(c);
        _iconBar.AddChild(_colorPicker);

        UpdatePlayButton();

        directorEventMediator.SubscribeToMenu(DirectorMenuCodes.StageWindow, () => Visible = !Visible);
    }
    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _iconBar.Position = new Vector2(0, size.Y - IconBarHeight);
        _iconBar.CustomMinimumSize = new Vector2(size.X, IconBarHeight);
    }

    public void SetStage(ILingoFrameworkStage stage)
    {
        _stage = stage;
        if (stage is Node node)
        {
            if (node.GetParent() != this)
            {
                node.GetParent()?.RemoveChild(node);
                AddChild(node);
            }
            if (node is Node2D node2D)
                node2D.Position = new Vector2(0, TitleBarHeight);
        }
    }

    public void SetActiveMovie(LingoMovie? movie)
    {
        if (_movie != null)
            _movie.PlayStateChanged -= OnPlayStateChanged;

        _stage?.SetActiveMovie(movie);
        _movie = movie;
        _selectedSprite = null;
        _selectionBox.Visible = false;

        if (_movie != null)
            _movie.PlayStateChanged += OnPlayStateChanged;

        UpdatePlayButton();
    }

    private void OnActiveMovieChanged(ILingoMovie? movie)
    {
        SetActiveMovie(movie as LingoMovie);
    }

    private void OnPlayPressed()
    {
        if (_movie == null) return;
        if (_movie.IsPlaying)
            _movie.Halt();
        else
            _movie.Play();
    }

    private void OnPlayStateChanged(bool isPlaying)
    {
        UpdatePlayButton();
        if (isPlaying)
            _selectionBox.Visible = false;
        else if (_selectedSprite != null)
            UpdateSelectionBox(_selectedSprite);
    }

    private void UpdatePlayButton()
    {
        _playButton.Text = _movie != null && _movie.IsPlaying ? "stop !" : "Play >";
    }

    private void UpdateZoom(float value)
    {
        if (_stage is Node2D node2D)
            node2D.Scale = new Vector2(value, value);
    }

    private void OnColorChanged(Color color)
    {
        _stageBgRect.Color = color;
        _colorDisplay.Color = color;
    }

    public void SpriteSelected(ILingoSprite sprite)
    {
        _selectedSprite = sprite as LingoSprite;
        if (_movie != null && !_movie.IsPlaying)
            UpdateSelectionBox(_selectedSprite);
    }

    private void UpdateSelectionBox(LingoSprite sprite)
    {
        var rect = new Rect2(sprite.LocH, sprite.LocV, sprite.Width, sprite.Height);
        _selectionBox.UpdateRect(rect);
        _selectionBox.Visible = true;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (!Visible || _movie == null || _movie.IsPlaying) return;
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            var sprite = _movie.GetSpriteUnderMouse();
            if (sprite != null)
                _mediator.RaiseSpriteSelected(sprite);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_movie != null)
            _movie.PlayStateChanged -= OnPlayStateChanged;
        _player.ActiveMovieChanged -= OnActiveMovieChanged;
        _mediator.Unsubscribe(this);
        base.Dispose(disposing);
    }

    private partial class SelectionBox : Node2D
    {
        private Rect2 _rect;
        public void UpdateRect(Rect2 rect)
        {
            _rect = rect;
            QueueRedraw();
        }

        public override void _Draw()
        {
            DrawRect(_rect, Colors.Yellow, false, 1);
        }
    }
}
