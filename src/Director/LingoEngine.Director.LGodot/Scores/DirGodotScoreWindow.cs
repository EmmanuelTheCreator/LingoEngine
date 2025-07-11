using Godot;
using LingoEngine.Movies;
using LingoEngine.Core;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Director.Core.Stages.Commands;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.UI;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Simple timeline overlay showing the Score channels and frames.
/// Toggled with F2.
/// </summary>
public partial class DirGodotScoreWindow : BaseGodotWindow, IDirFrameworkScoreWindow,
    ICommandHandler<ChangeSpriteRangeCommand>,
    ICommandHandler<AddSpriteCommand>,
    ICommandHandler<RemoveSpriteCommand>
{
   
    
    private int _footerMargin= 10;

    private bool wasToggleKey;
    private LingoMovie? _movie;
    private readonly ScrollContainer _vClipper = new ScrollContainer();
    private readonly ScrollContainer _masterScroller = new ScrollContainer();
    private readonly Control _topStripContent = new Control();
    private readonly Control _scrollContent = new Control();
    private ColorRect _hClipper;
    private LingoPlayer _player;
    private readonly DirScoreGfxValues _gfxValues = new();
    private readonly DirGodotScoreGrid _grid;
    internal LingoSprite? SelectedSprite => _grid.SelectedSprite;
    private readonly DirGodotFrameHeader _header;
    private readonly DirGodotFrameScriptsBar _frameScripts;
    private readonly DirGodotSoundBar _soundBar;
    private readonly DirGodotScoreLabelsBar _labelBar;
    private readonly DirGodotScoreChannelBar _channelBar;
    private readonly CollapseButton _collapseButton;
    private readonly DirGodotCastLeftLabel _leftLabelHeader;
    private readonly DirGodotCastLeftTopLabels _leftTopLabels;

    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoCommandManager _commandManager;
    private readonly IHistoryManager _historyManager;


    public DirGodotScoreWindow(IDirectorEventMediator directorMediator, ILingoCommandManager commandManager, IHistoryManager historyManager, DirectorScoreWindow directorScoreWindow, ILingoPlayer player, IDirGodotWindowManager windowManager)
        : base(DirectorMenuCodes.ScoreWindow, "Score", windowManager)
    {
        _mediator = directorMediator;
        _commandManager = commandManager;
        _historyManager = historyManager;
        directorScoreWindow.Init(this);
        _player = (LingoPlayer) player;
        _player.ActiveMovieChanged += OnActiveMovieChanged;
        

        var height = 400;
        var width = 800;

        _leftLabelHeader = new DirGodotCastLeftLabel(_gfxValues);
        _leftTopLabels = new DirGodotCastLeftTopLabels(_gfxValues);
        AddChild(_leftLabelHeader);
        AddChild(_leftTopLabels);


        Size = new Vector2(width, height);
        CustomMinimumSize = Size;
        _soundBar = new DirGodotSoundBar(_gfxValues, _player.Factory);
        _soundBar.Collapsed = true;
        _channelBar = new DirGodotScoreChannelBar(_gfxValues, _soundBar);
        _grid = new DirGodotScoreGrid(directorMediator, _gfxValues, commandManager, historyManager, _player.Factory);
        _mediator.Subscribe(_grid);
        _header = new DirGodotFrameHeader(_gfxValues);
        _frameScripts = new DirGodotFrameScriptsBar(_gfxValues, _player.Factory);
        _labelBar = new DirGodotScoreLabelsBar(_gfxValues, commandManager);
        _labelBar.HeaderCollapseChanged += OnHeaderCollapseChanged;
        _labelBar.HeaderCollapsed = _soundBar.Collapsed;
        _collapseButton = new CollapseButton(_labelBar) { ZIndex = 1 };
        

        // The grid inside master scoller
        _masterScroller.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _masterScroller.VerticalScrollMode= ScrollContainer.ScrollMode.ShowAlways;
        _masterScroller.Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, Size.Y - _gfxValues.TopStripHeight- _footerMargin);
        _masterScroller.Position = new Vector2(_gfxValues.ChannelInfoWidth, _gfxValues.TopStripHeight);
        
        _masterScroller.AddChild(_scrollContent);
        AddChild(_masterScroller);

        _scrollContent.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _scrollContent.SizeFlagsVertical = SizeFlags.ExpandFill;
        _scrollContent.MouseFilter = MouseFilterEnum.Ignore;
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
        _hClipper.AddChild(_collapseButton);
        AddChild(_hClipper);
        AddChild(_soundBar);
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
        _soundBar.Position = new Vector2(0, 40);
        RepositionBars();
        


        UpdateScrollSize();
    }

    private void OnHeaderCollapseChanged(bool collapsed)
    {
        _soundBar.Collapsed = collapsed;
        RepositionBars();
    }

    private void RepositionBars()
    {
        float soundHeight = (_soundBar.Collapsed ? 0 : _gfxValues.ChannelHeight * 4);

        _frameScripts.Position = new Vector2(0, 20 + soundHeight);
        _header.Position = new Vector2(0, _frameScripts.Position.Y + 20);

        float topHeight = _header.Position.Y + 20;
        _masterScroller.Position = new Vector2(_gfxValues.ChannelInfoWidth, topHeight + 20);
        _vClipper.Position = new Vector2(0, topHeight + 20);
        _collapseButton.Position = new Vector2(_hClipper.Size.X - 16, 4);
        _leftTopLabels.Position = new Vector2(0, _frameScripts.Position.Y+20);
        UpdateScrollSize();
    }
    public override void _Process(double delta)
    {
        if (!Visible) return;
        _channelBar.Position = new Vector2(0, -_masterScroller.ScrollVertical);
        _topStripContent.Position = new Vector2(-_masterScroller.ScrollHorizontal, _topStripContent.Position.Y);
        _soundBar.ScrollX = _masterScroller.ScrollHorizontal;
    }

    private void RefreshGrid()
    {
        _grid.MarkSpriteDirty();
    }

    private void UpdateScrollSize()
    {
        if (_movie == null) return;

        float gridWidth = _gfxValues.ChannelInfoWidth + _movie.FrameCount * _gfxValues.FrameWidth + _gfxValues.ExtraMargin;
        float gridHeight = _movie.MaxSpriteChannelCount * _gfxValues.ChannelHeight + _gfxValues.ExtraMargin;

        float topHeight = _header.Position.Y + 20;

        _channelBar.CustomMinimumSize = new Vector2(_gfxValues.ChannelInfoWidth, gridHeight - _footerMargin);
        _scrollContent.CustomMinimumSize = new Vector2(gridWidth, gridHeight - _footerMargin);
        _topStripContent.CustomMinimumSize = new Vector2(gridWidth, topHeight + 20);

        _vClipper.Size = new Vector2(_gfxValues.ChannelInfoWidth, Size.Y - topHeight - 20 - _footerMargin);
        _hClipper.Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, topHeight + 20);
        _masterScroller.Size = new Vector2(Size.X - _gfxValues.ChannelInfoWidth, Size.Y - topHeight - 20 - _footerMargin);
    }


    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        RepositionBars();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
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
        _soundBar.SetMovie(movie);
        _channelBar.SetMovie(movie);
        _labelBar.SetMovie(movie);
        _labelBar.HeaderCollapsed = _soundBar.Collapsed;
        RepositionBars();
    }

    

    protected override void Dispose(bool disposing)
    {
        _player.ActiveMovieChanged -= OnActiveMovieChanged;
        _grid.Dispose();
        _labelBar.Dispose();
        _frameScripts.Dispose();
        _soundBar.Dispose();
        _masterScroller.Dispose();
        //_hScroller.Dispose();
        _channelBar.Dispose();
        _mediator.Unsubscribe(_grid);
        base.Dispose(disposing);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsActiveWindow) return;
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
                    GetViewport().SetInputAsHandled();
                }
            }
        }
    }

    public bool CanExecute(ChangeSpriteRangeCommand command) => true;
    public bool Handle(ChangeSpriteRangeCommand command)
    {
        if (command.EndChannel != command.Sprite.SpriteNum - 1)
            command.Movie.ChangeSpriteChannel(command.Sprite, command.EndChannel);
        command.Sprite.BeginFrame = command.EndBegin;
        command.Sprite.EndFrame = command.EndEnd;
        _historyManager.Push(command.ToUndo(RefreshGrid), command.ToRedo(RefreshGrid));
        RefreshGrid();
        return true;
    }

    public bool CanExecute(AddSpriteCommand command) => true;
    public bool Handle(AddSpriteCommand command)
    {
        var sprite = command.Movie.AddSprite(command.Channel, command.BeginFrame, command.EndFrame, 0, 0,
            s => s.SetMember(command.Member));
        _historyManager.Push(command.ToUndo(sprite, RefreshGrid), command.ToRedo(RefreshGrid));
        RefreshGrid();
        return true;
    }

    public bool CanExecute(RemoveSpriteCommand command) => true;
    public bool Handle(RemoveSpriteCommand command)
    {
        var movie = command.Movie;
        var sprite = command.Sprite;

        int channel = sprite.SpriteNum;
        int begin = sprite.BeginFrame;
        int end = sprite.EndFrame;
        var member = sprite.Member;
        string name = sprite.Name;
        float x = sprite.LocH;
        float y = sprite.LocV;

        sprite.RemoveMe();

        LingoSprite current = sprite;
        void refresh() => RefreshGrid();

        Action undo = () =>
        {
            current = movie.AddSprite(channel, begin, end, x, y, s =>
            {
                s.Name = name;
                if (member != null)
                    s.SetMember(member);
            });
            refresh();
        };

        Action redo = () =>
        {
            current.RemoveMe();
            refresh();
        };

        _historyManager.Push(undo, redo);
        RefreshGrid();
        return true;
    }

 

    internal partial class DirGodotCastLeftTopLabels : Control
    {
        private readonly DirScoreGfxValues _gfxValues;

        public DirGodotCastLeftTopLabels(DirScoreGfxValues gfxValues)
        {
            _gfxValues = gfxValues;
            Size = new Vector2(gfxValues.ChannelLabelWidth + gfxValues.ChannelHeight, 40);
        }
        public override void _Draw()
        {
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), new Color("#f0f0f0"));
            int labelHeight = 20;
            DrawTextWithLine(0, labelHeight, "Scripts");
            DrawTextWithLine(labelHeight, labelHeight, "Member", false);
        }
        private void DrawTextWithLine(int top, int height, string text, bool withTopLines = false)
        {
            var font = ThemeDB.FallbackFont;
            
            DrawString(font, new Vector2(5, top+ font.GetAscent()-3), text, HorizontalAlignment.Left, -1, 10, new Color("#666666"));
            if (withTopLines)
            {
                DrawLines(top);
               
            }
            DrawLine(new Vector2(0, top + height), new Vector2(Size.X, top + height), _gfxValues.ColLineDark.ToGodotColor());
            DrawLine(new Vector2(0, top + height + 1), new Vector2(Size.X, top + height + 1), _gfxValues.ColLineLight.ToGodotColor());
        }
        private void DrawLines(int top)
        {
            DrawLine(new Vector2(0, top), new Vector2(Size.X, top), _gfxValues.ColLineDark.ToGodotColor());
            DrawLine(new Vector2(0, top + 1), new Vector2(Size.X, top + 1), _gfxValues.ColLineLight.ToGodotColor());
        }
    }

    internal partial class DirGodotCastLeftLabel : Control
    {
        private readonly DirScoreGfxValues _gfxValues;

        public DirGodotCastLeftLabel(DirScoreGfxValues gfxValues)
        {
            _gfxValues = gfxValues;
            Size = new Vector2(gfxValues.ChannelLabelWidth + gfxValues.ChannelHeight, 20);
            Position = new Vector2(0, 20);
        }

        public override void _Draw()
        {
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), new Color("#f0f0f0"));
            DrawTextWithLine(0, 20, "Labels", false);
        }

        private void DrawTextWithLine(int top, int height, string text, bool withTopLines = true)
        {
            var font = ThemeDB.FallbackFont;
            if (withTopLines)
                DrawLines(top);
            DrawString(font, new Vector2(5, top + font.GetAscent() - 3), text, HorizontalAlignment.Left, -1, 10, new Color("#666666"));
            DrawLine(new Vector2(0, top + height), new Vector2(Size.X, top + height), _gfxValues.ColLineDark.ToGodotColor());
            DrawLine(new Vector2(0, top + height + 1), new Vector2(Size.X, top + height + 1), _gfxValues.ColLineLight.ToGodotColor());
        }

        private void DrawLines(int top)
        {
            DrawLine(new Vector2(0, top), new Vector2(Size.X, top), _gfxValues.ColLineDark.ToGodotColor());
            DrawLine(new Vector2(0, top + 1), new Vector2(Size.X, top + 1), _gfxValues.ColLineLight.ToGodotColor());
        }
    }

    private partial class CollapseButton : Control
    {
        private readonly DirGodotScoreLabelsBar _labels;
        public CollapseButton(DirGodotScoreLabelsBar labels)
        {
            _labels = labels;
            Size = new Vector2(12, 12);
            MouseFilter = MouseFilterEnum.Stop;
            _labels.HeaderCollapseChanged += _ => QueueRedraw();
        }
        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                _labels.ToggleCollapsed();
            }
        }
        public override void _Draw()
        {
            var font = ThemeDB.FallbackFont;
            DrawRect(new Rect2(0, 0, 12, 12), Colors.Black, false, 1);
            DrawString(font, new Vector2(2, font.GetAscent() - 5), (_labels.HeaderCollapsed ? "▶" : "▼"));
        }
    }
}
