using Godot;
using LingoEngine.Movies;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Stages;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Core;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.LGodot;
using System.Linq;
using System.Collections.Generic;
using LingoEngine.LGodot.Primitives;


namespace LingoEngine.Director.LGodot.Movies;

internal partial class DirGodotStageWindow : BaseGodotWindow, IHasSpriteSelectedEvent, IDirFrameworkStageWindow,
    ICommandHandler<MoveSpritesCommand>, ICommandHandler<RotateSpritesCommand>
{
    private const int IconBarHeight = 12;
    private readonly LingoGodotStageContainer _stageContainer;
    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoPlayer _player;
    private readonly ILingoCommandManager _commandManager;
    private readonly IStageToolManager _toolManager;
    private readonly IHistoryManager _historyManager;
    private readonly HBoxContainer _iconBar = new HBoxContainer();
    private readonly HSlider _zoomSlider = new HSlider();
    private readonly OptionButton _zoomDropdown = new OptionButton();
    private readonly Button _rewindButton = new Button();
    private readonly Button _playButton = new Button();
    private readonly Button _prevFrameButton = new Button();
    private readonly Button _nextFrameButton = new Button();
    private readonly Button _recordButton = new Button();
    private readonly ColorRect _stageBgRect = new ColorRect();
    private readonly ColorRect _colorDisplay = new ColorRect();
    private readonly ColorPickerButton _colorPicker = new ColorPickerButton();
    private readonly ScrollContainer _scrollContainer = new ScrollContainer();
    private readonly SelectionBox _selectionBox = new SelectionBox();

    private LingoMovie? _movie;
    private ILingoFrameworkStage? _stage;
    private readonly List<LingoSprite> _selectedSprites = new();
    private LingoSprite? _primarySelectedSprite;
    private Vector2? _dragStart;
    private Dictionary<LingoSprite, Primitives.LingoPoint>? _initialPositions;
    private Dictionary<LingoSprite, float>? _initialRotations;
    private bool _rotating;
    private bool _spaceHeld;
    private bool _panning;
    private float _scale = 1f;

    public DirGodotStageWindow(ILingoFrameworkStageContainer stageContainer, IDirectorEventMediator directorEventMediator, ILingoCommandManager commandManager, IStageToolManager toolManager, IHistoryManager historyManager, ILingoPlayer player, DirectorStageWindow directorStageWindow, IDirGodotWindowManager windowManager)
        : base(DirectorMenuCodes.StageWindow, "Stage", windowManager)
    {
        _stageContainer = (LingoGodotStageContainer)stageContainer;
        _mediator = directorEventMediator;
        _player = player;
        _player.ActiveMovieChanged += OnActiveMovieChanged;
        _commandManager = commandManager;
        _toolManager = toolManager;
        _historyManager = historyManager;
        directorStageWindow.Init(this);

        _mediator.Subscribe(this);

        _toolManager.ToolChanged += OnToolChanged;
        
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
        

        _scrollContainer.Position = new Vector2(0, 20);
        _stageBgRect.Color = Colors.Black;
        _stageBgRect.CustomMinimumSize = new Vector2(640, 480);
        _stageBgRect.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _stageBgRect.SizeFlagsVertical = SizeFlags.ExpandFill;
        _scrollContainer.AddChild(_stageBgRect);
        _scrollContainer.AddChild(_stageContainer.Container);
        _stageContainer.Container.AddChild(_selectionBox);
        _selectionBox.Visible = false;
        _selectionBox.ZIndex = 1000;
        AddChild(_scrollContainer);

        // bottom icon bar
        AddChild(_iconBar);
        _iconBar.AnchorLeft = 0;
        _iconBar.AnchorRight = 1;
        _iconBar.AnchorTop = 1;
        _iconBar.AnchorBottom = 1;
        _iconBar.OffsetLeft = 0;
        _iconBar.OffsetRight = 0;
        _iconBar.OffsetTop = -IconBarHeight;
        _iconBar.OffsetBottom = 0;

        _rewindButton.Text = "|<";
        _rewindButton.CustomMinimumSize = new Vector2(20, IconBarHeight);
        _rewindButton.Pressed += () => _commandManager.Handle(new RewindMovieCommand());
        _iconBar.AddChild(_rewindButton);

        _playButton.CustomMinimumSize = new Vector2(60, IconBarHeight);
        _playButton.AddThemeFontSizeOverride("font_size", 8);
        _playButton.Pressed += () => _commandManager.Handle(new PlayMovieCommand());
        _iconBar.AddChild(_playButton);

        _prevFrameButton.Text = "<";
        _prevFrameButton.CustomMinimumSize = new Vector2(20, IconBarHeight);
        _prevFrameButton.Pressed += () => _commandManager.Handle(new StepFrameCommand(-1));
        _iconBar.AddChild(_prevFrameButton);

        _nextFrameButton.Text = ">";
        _nextFrameButton.CustomMinimumSize = new Vector2(20, IconBarHeight);
        _nextFrameButton.Pressed += () => _commandManager.Handle(new StepFrameCommand(1));
        _iconBar.AddChild(_nextFrameButton);

        _zoomSlider.MinValue = 0.5f;
        _zoomSlider.MaxValue = 1.5f;
        _zoomSlider.Step = 0.1f;
        _zoomSlider.Value = 1f;
        _zoomSlider.CustomMinimumSize = new Vector2(150, IconBarHeight);
        _zoomSlider.ValueChanged += value =>
        {
            float scale = (float)value;
            _scale = scale;
            UpdateScaleDropdown(scale);
            _stageContainer.SetScale(scale);
        };
        _iconBar.AddChild(_zoomSlider);

        for (int i = 50; i <= 150; i += 10)
            _zoomDropdown.AddItem($"{i}%");
        _zoomDropdown.Select(5); // 100%
        _zoomDropdown.CustomMinimumSize = new Vector2(60, IconBarHeight);
        _zoomDropdown.ItemSelected += id =>
        {
            float scale = (50 + id * 10) / 100f;
            _zoomSlider.Value = scale;
            _scale = scale;
            _stageContainer.SetScale(scale);
        };
        _iconBar.AddChild(_zoomDropdown);

        _colorDisplay.Color = Colors.Black;
        _colorDisplay.CustomMinimumSize = new Vector2(IconBarHeight, IconBarHeight);
        _iconBar.AddChild(_colorDisplay);

        _colorPicker.CustomMinimumSize = new Vector2(IconBarHeight, IconBarHeight);
        _colorPicker.Color = Colors.Black;
        _colorPicker.ColorChanged += c => OnColorChanged(c);
        _iconBar.AddChild(_colorPicker);

        _recordButton.Text = "●";
        _recordButton.ToggleMode = true;
        _recordButton.CustomMinimumSize = new Vector2(IconBarHeight, IconBarHeight);
        _recordButton.AddThemeColorOverride("font_color", Colors.Red);
        _recordButton.Toggled += pressed =>
        {
            if (_player is LingoPlayer lp)
                lp.Stage.RecordKeyframes = pressed;
        };
        _iconBar.AddChild(_recordButton);

        UpdatePlayButton();
    }
    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
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
        _selectedSprites.Clear();
        _primarySelectedSprite = null;
        _selectionBox.Visible = false;

        if (_movie != null)
            _movie.PlayStateChanged += OnPlayStateChanged;

        UpdatePlayButton();
    }

    private void OnActiveMovieChanged(ILingoMovie? movie)
    {
        SetActiveMovie(movie as LingoMovie);
    }

    private void OnPlayStateChanged(bool isPlaying)
    {
        UpdatePlayButton();
        if (isPlaying)
            _selectionBox.Visible = false;
        else if (_selectedSprites.Count > 0)
            UpdateSelectionBox();
    }

    private void UpdatePlayButton()
    {
        _playButton.Text = _movie != null && _movie.IsPlaying ? "stop !" : "Play >";
    }


    private void OnColorChanged(Color color)
    {
        _stageBgRect.Color = color;
        _colorDisplay.Color = color;
    }

    private void UpdateScaleDropdown(float value)
    {
        int percent = (int)Mathf.Round(value * 100);
        int index = (percent - 50) / 10;
        if (index >= 0 && index < _zoomDropdown.ItemCount)
            _zoomDropdown.Select(index);
    }

    private void OnToolChanged(StageTool tool)
    {
        switch (tool)
        {
            case StageTool.Pointer:
                Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
                break;
            case StageTool.Move:
                Input.SetDefaultCursorShape(Input.CursorShape.Move);
                break;
            case StageTool.Rotate:
                Input.SetDefaultCursorShape(Input.CursorShape.Cross);
                break;
        }
    }

    public void SpriteSelected(ILingoSprite sprite)
    {
        _selectedSprites.Clear();
        if (!(sprite is LingoSprite ls))
            return;
        _selectedSprites.Add(ls);
        _primarySelectedSprite = ls;
        if (_movie != null && !_movie.IsPlaying && ls != null)
            UpdateSelectionBox();
    }

    private void UpdateSelectionBox()
    {
        if (_selectedSprites.Count == 0)
        {
            _selectionBox.Visible = false;
            return;
        }
        float left = _selectedSprites.Min(s => s.LocH);
        float top = _selectedSprites.Min(s => s.LocV);
        float right = _selectedSprites.Max(s => s.LocH + s.Width);
        float bottom = _selectedSprites.Max(s => s.LocV + s.Height);
        var rect = new Rect2(left, top, right - left, bottom - top);
        _selectionBox.UpdateRect(rect);
        _selectionBox.Visible = true;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (!Visible || _movie == null || _movie.IsPlaying) return;

        if (@event is InputEventKey spaceKey && spaceKey.Keycode == Key.Space)
        {
            _spaceHeld = spaceKey.Pressed;
            if (!spaceKey.Pressed)
                _panning = false;
            return;
        }
        else if (@event is InputEventMouseButton mb)
        {
            Vector2 mousePos = GetGlobalMousePosition();
            Rect2 bounds = new Rect2(_scrollContainer.GlobalPosition, _scrollContainer.Size);
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed && _spaceHeld && bounds.HasPoint(mousePos))
                {
                    _panning = true;
                    return;
                }
                else if (!mb.Pressed && _panning)
                {
                    _panning = false;
                    return;
                }
            }
            else if (!mb.Pressed && (mb.ButtonIndex == MouseButton.WheelUp || mb.ButtonIndex == MouseButton.WheelDown) && bounds.HasPoint(mousePos))
            {
                float delta = mb.ButtonIndex == MouseButton.WheelUp ? 0.1f : -0.1f;
                float newScale = Mathf.Clamp(_scale + delta, (float)_zoomSlider.MinValue, (float)_zoomSlider.MaxValue);
                _zoomSlider.Value = newScale;
                _scale = newScale;
                UpdateScaleDropdown(newScale);
                _stageContainer.SetScale(newScale);
                return;
            }
        }
        else if (@event is InputEventMouseMotion motion && _panning)
        {
            _scrollContainer.ScrollHorizontal -= (int)motion.Relative.X;
            _scrollContainer.ScrollVertical -= (int)motion.Relative.Y;
            return;
        }

        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Z && key.CtrlPressed)
        {
            _historyManager.Undo();
            return;
        }

        switch (_toolManager.CurrentTool)
        {
            case StageTool.Pointer:
                HandlePointerInput(@event);
                break;
            case StageTool.Move:
                HandleMoveInput(@event);
                break;
            case StageTool.Rotate:
                HandleRotateInput(@event);
                break;
        }
    }

    private void HandlePointerInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            Vector2 localPos = _stageContainer.Container.ToLocal(mb.Position);

            var sprite = _movie.GetSpriteAtPoint(localPos.X, localPos.Y) as LingoSprite;
            if (mb.Pressed)
            {
                if (sprite != null)
                {
                    if (Input.IsKeyPressed(Key.Ctrl))
                    {
                        if (_selectedSprites.Contains(sprite))
                            _selectedSprites.Remove(sprite);
                        else
                            _selectedSprites.Add(sprite);
                        UpdateSelectionBox();
                    }
                    else
                    {
                        _selectedSprites.Clear();
                        _selectedSprites.Add(sprite);
                        _mediator.RaiseSpriteSelected(sprite);
                        UpdateSelectionBox();
                    }
                }
            }
        }
    }

    private void HandleMoveInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                if (_selectedSprites.Count > 0)
                {
                    _dragStart = mb.Position;
                    _initialPositions = _selectedSprites.ToDictionary(s => s, s => new Primitives.LingoPoint(s.LocH, s.LocV));
                    if (_player is LingoPlayer lp && lp.Stage.RecordKeyframes)
                        foreach (var s in _selectedSprites)
                            lp.Stage.AddKeyFrame(s);
                }
            }
            else if (_dragStart.HasValue && _initialPositions != null)
            {
                var end = _selectedSprites.ToDictionary(s => s, s => new Primitives.LingoPoint(s.LocH, s.LocV));
                _commandManager.Handle(new MoveSpritesCommand(_initialPositions, end));
                _dragStart = null;
                _initialPositions = null;
                if (_player is LingoPlayer lp && lp.Stage.RecordKeyframes)
                    foreach (var s in _selectedSprites)
                        lp.Stage.UpdateKeyFrame(s);
            }
        }
        else if (@event is InputEventMouseMotion motion && _dragStart.HasValue && _initialPositions != null)
        {
            Vector2 delta = motion.Position - _dragStart.Value;
            foreach (var s in _selectedSprites)
            {
                var start = _initialPositions[s];
                s.LocH = start.X + delta.X;
                s.LocV = start.Y + delta.Y;
                if (_player is LingoPlayer lp && lp.Stage.RecordKeyframes)
                    lp.Stage.UpdateKeyFrame(s);
            }
            UpdateSelectionBox();
        }
    }

    private void HandleRotateInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                if (_selectedSprites.Count > 0)
                {
                    _dragStart = mb.Position;
                    _initialRotations = _selectedSprites.ToDictionary(s => s, s => s.Rotation);
                    _rotating = true;
                    if (_player is LingoPlayer lp && lp.Stage.RecordKeyframes)
                        foreach (var s in _selectedSprites)
                            lp.Stage.AddKeyFrame(s);
                }
            }
            else if (_rotating && _initialRotations != null)
            {
                var end = _selectedSprites.ToDictionary(s => s, s => s.Rotation);
                _commandManager.Handle(new RotateSpritesCommand(_initialRotations, end));
                _rotating = false;
                _dragStart = null;
                _initialRotations = null;
                if (_player is LingoPlayer lp && lp.Stage.RecordKeyframes)
                    foreach (var s in _selectedSprites)
                        lp.Stage.UpdateKeyFrame(s);
            }
        }
        else if (@event is InputEventMouseMotion motion && _rotating && _initialRotations != null && _dragStart.HasValue)
        {
            Vector2 center = ComputeSelectionCenter();
            float startAngle = (_dragStart.Value - center).Angle();
            float currentAngle = (motion.Position - center).Angle();
            float delta = Mathf.RadToDeg(currentAngle - startAngle);
            foreach (var s in _selectedSprites)
            {
                s.Rotation = _initialRotations[s] + delta;
                if (_player is LingoPlayer lp && lp.Stage.RecordKeyframes)
                    lp.Stage.UpdateKeyFrame(s);
            }
            UpdateSelectionBox();
        }
    }

    private Vector2 ComputeSelectionCenter()
    {
        if (_selectedSprites.Count == 0) return Vector2.Zero;
        float left = _selectedSprites.Min(s => s.LocH);
        float top = _selectedSprites.Min(s => s.LocV);
        float right = _selectedSprites.Max(s => s.LocH + s.Width);
        float bottom = _selectedSprites.Max(s => s.LocV + s.Height);
        return new Vector2((left + right) / 2f, (top + bottom) / 2f);
    }

    public bool CanExecute(MoveSpritesCommand command) => true;
    public bool Handle(MoveSpritesCommand command)
    {
        foreach (var kv in command.EndPositions)
        {
            kv.Key.LocH = kv.Value.X;
            kv.Key.LocV = kv.Value.Y;
        }
        _historyManager.Push(command.ToUndo(UpdateSelectionBox));
        UpdateSelectionBox();
        return true;
    }

    public bool CanExecute(RotateSpritesCommand command) => true;
    public bool Handle(RotateSpritesCommand command)
    {
        foreach (var kv in command.EndRotations)
            kv.Key.Rotation = kv.Value;
        _historyManager.Push(command.ToUndo(UpdateSelectionBox));
        UpdateSelectionBox();
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (_movie != null)
            _movie.PlayStateChanged -= OnPlayStateChanged;
        _player.ActiveMovieChanged -= OnActiveMovieChanged;
        _toolManager.ToolChanged -= OnToolChanged;
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
