using System;
using AbstUI.Commands;
using AbstUI.Components.Containers;
using AbstUI.Components.Graphics;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Stages.Commands;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Stages;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Director.Core.Stages;

/// <summary>
/// Second generation Stage window implementation that relies exclusively on
/// AbstUI primitives so it can be re-used by every framework.
/// </summary>
public class DirectorStageWindowV2 : DirectorWindow<IDirFrameworkStageWindow>,
    IAbstCommandHandler<StageToolSelectCommand>,
    IAbstCommandHandler<MoveSpritesCommand>,
    IAbstCommandHandler<RotateSpritesCommand>
{
    private const float ScrollAreaWidth = 3000f;
    private const float ScrollAreaHeight = 2000f;
    private const float StagePadding = 0f;

    private readonly IBlingoPlayer _player;
    private readonly IHistoryManager _historyManager;
    private readonly IDirectorEventMediator _mediator;
    private readonly DirStageManager _stageManager;
    private readonly DirectorStageGuides _guides;
    private readonly StageBoundingBoxesOverlay _boundingBoxes;
    private readonly StageSpriteSummaryOverlay _spriteSummary;
    private readonly StageMotionPathOverlay _motionPath;

    private readonly AbstPanel _rootPanel;
    private readonly AbstScrollContainer _scrollContainer;
    private readonly AbstPanel _scrollContent;
    private readonly AbstZoomBox _zoomBox;
    private readonly AbstPanel _stageLayer;
    private readonly AbstPanel _stageBackgroundPanel;
    private readonly AbstPanel _stageHostPanel;
    private readonly AbstGfxCanvas _selectionCanvas;
    private readonly AbstGfxCanvas _boundingBoxesCanvas;
    private readonly AbstGfxCanvas _motionPathCanvas;
    private readonly AbstGfxCanvas _guidesCanvas;
    private readonly AbstGfxCanvas _spriteSummaryCanvas;

    private readonly IAbstMouseSubscription _mouseDownSub;
    private readonly IAbstMouseSubscription _mouseUpSub;
    private readonly IAbstMouseSubscription _mouseMoveSub;
    private readonly IAbstMouseSubscription _mouseWheelSub;

    private readonly IDirectorEventSubscription _stageChangedSubscription;

    private IBlingoMovie? _currentMovie;
    private bool _spaceHeld;
    private bool _ctrlHeld;
    private bool _panning;
    private float _scale = 1f;
    private APoint _lastMousePosition;

    protected IBlingoStage Stage { get; }

    public StageTool SelectedTool { get; private set; }
    public AbstPanel StageLayer => _stageLayer;
    public StageIconBar IconBar { get; }

    public DirectorStageWindowV2(
        IServiceProvider serviceProvider,
        IHistoryManager historyManager,
        IBlingoFrameworkFactory factory,
        IAbstCommandManager commandManager,
        IBlingoPlayer player,
        IDirectorEventMediator mediator,
        DirStageManager stageManager,
        DirectorStageGuides guides,
        IDirectorIconManager iconManager) : base(serviceProvider, DirectorMenuCodes.StageWindow)
    {
        _player = player;
        _historyManager = historyManager;
        _mediator = mediator;
        _stageManager = stageManager;
        _guides = guides;

        Stage = player.Stage;
        IconBar = new StageIconBar(factory, commandManager, player, mediator, stageManager);
        IconBar.ZoomChanged += SetScale;
        IconBar.ColorChanged += OnColorChanged;

        _boundingBoxes = new StageBoundingBoxesOverlay(factory, mediator);
        _spriteSummary = new StageSpriteSummaryOverlay(factory, mediator, iconManager);
        _motionPath = new StageMotionPathOverlay(factory);
        _guides.Draw();

        _rootPanel = factory.ComponentFactory.CreatePanel("StageWindowRoot");
        _scrollContainer = factory.ComponentFactory.CreateScrollContainer("StageScrollContainer");
        _scrollContainer.ScollbarModeH = AbstScrollbarMode.AlwaysVisible;
        _scrollContainer.ScollbarModeV = AbstScrollbarMode.AlwaysVisible;

        _scrollContent = factory.ComponentFactory.CreatePanel("StageScrollContent");
        _scrollContent.Width = ScrollAreaWidth;
        _scrollContent.Height = ScrollAreaHeight;

        _zoomBox = factory.ComponentFactory.CreateZoomBox("StageZoomBox");
        _zoomBox.Width = ScrollAreaWidth;
        _zoomBox.Height = ScrollAreaHeight;

        _stageLayer = factory.ComponentFactory.CreatePanel("StageLayer");
        _stageLayer.Width = Stage.Width + StagePadding * 2f;
        _stageLayer.Height = Stage.Height + StagePadding * 2f;

        _stageBackgroundPanel = factory.ComponentFactory.CreatePanel("StageBackgroundPanel");
        _stageBackgroundPanel.BackgroundColor = Stage.BackgroundColor;
        _stageBackgroundPanel.Width = Stage.Width;
        _stageBackgroundPanel.Height = Stage.Height;
       

        var stageContainer = serviceProvider.GetRequiredService<IBlingoFrameworkStageContainer>();

        Stage.X = StagePadding;
        Stage.Y = StagePadding;
        //_stageLayer.AddItem(Stage);

        _boundingBoxesCanvas = _boundingBoxes.Canvas;
        _boundingBoxesCanvas.X = StagePadding;
        _boundingBoxesCanvas.Y = StagePadding;
        
        _motionPathCanvas = _motionPath.Canvas;
        _motionPathCanvas.X = StagePadding;
        _motionPathCanvas.Y = StagePadding;

        _guidesCanvas = _guides.Canvas;
        _guidesCanvas.X = StagePadding;
        _guidesCanvas.Y = StagePadding;

        _selectionCanvas = factory.CreateGfxCanvas("StageSelectionCanvas", (int)Stage.Width, (int)Stage.Height);
        _selectionCanvas.X = StagePadding;
        _selectionCanvas.Y = StagePadding;
        _selectionCanvas.Visibility = false;

        _spriteSummaryCanvas = _spriteSummary.Canvas;
        _spriteSummaryCanvas.X = StagePadding;
        _spriteSummaryCanvas.Y = StagePadding;

        _stageHostPanel = factory.ComponentFactory.CreatePanel("StageHost");
        _stageHostPanel.Width = ScrollAreaWidth;
        _stageHostPanel.Height = ScrollAreaHeight;
        _stageHostPanel.AddItem(_stageLayer,
            (ScrollAreaWidth - _stageLayer.Width) / 2f,
            (ScrollAreaHeight - _stageLayer.Height) / 2f);

        _zoomBox.Content = _stageHostPanel;
        _scrollContent.AddItem(_zoomBox);
        _scrollContainer.AddItem(_scrollContent);

        _rootPanel.AddItem(_scrollContainer);
        _rootPanel.AddItem(IconBar.Panel);
       

        MinimumWidth = 200;
        MinimumHeight = 150;
        Width = 650;
        Height = 520;
        X = 70;
        Y = 22;

        _mouseDownSub = MouseT.OnMouseDown(OnMouseDown);
        _mouseUpSub = MouseT.OnMouseUp(OnMouseUp);
        _mouseMoveSub = MouseT.OnMouseMove(OnMouseMove);
        _mouseWheelSub = MouseT.OnMouseWheel(OnMouseWheel);

        _stageChangedSubscription = _mediator.Subscribe(DirectorEventType.StagePropertiesChanged, () =>
        {
            UpdateStageLayout();
            return true;
        });

        player.ActiveMovieChanged += Player_ActiveMovieChanged;
        _stageManager.SelectionChanged += OnStageSelectionChanged;
        _stageManager.SpritesTransformed += OnStageSpritesTransformed;

        UpdateStageLayout();
        UpdateBoundingBoxes();
        UpdateMotionPath();
    }

    protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
    {
        base.OnInit(frameworkWindow);
        Title = "Stage";
        Content = _rootPanel;
        _stageLayer.AddItem(_stageBackgroundPanel, StagePadding, StagePadding);
    }
    public void ComposeStageLayers()
    {
        
        _stageLayer.AddItem(_boundingBoxesCanvas);
        _stageLayer.AddItem(_motionPathCanvas);
        _stageLayer.AddItem(_guidesCanvas);
        _stageLayer.AddItem(_selectionCanvas);
        _stageLayer.AddItem(_spriteSummaryCanvas);
    }

    private void Player_ActiveMovieChanged(IBlingoMovie? movie)
    {
        if (_currentMovie is BlingoMovie previousMovie)
        {
            previousMovie.PlayStateChanged -= OnPlayStateChanged;
            previousMovie.Sprite2DListChanged -= SpriteListChanged;
        }

        _currentMovie = movie;
        if (_currentMovie is BlingoMovie blingoMovie)
        {
            blingoMovie.PlayStateChanged += OnPlayStateChanged;
            blingoMovie.Sprite2DListChanged += SpriteListChanged;
            var env = blingoMovie.GetEnvironment();
            _boundingBoxes.SetInput(env.Mouse, env.Key);
        }
        UpdateBoundingBoxes();
        UpdateMotionPath();
        UpdateSelectionBox();
    }

    protected override void OnDispose()
    {
        _mouseDownSub.Release();
        _mouseUpSub.Release();
        _mouseMoveSub.Release();
        _mouseWheelSub.Release();
        _stageChangedSubscription.Release();
        _player.ActiveMovieChanged -= Player_ActiveMovieChanged;
        _stageManager.SelectionChanged -= OnStageSelectionChanged;
        _stageManager.SpritesTransformed -= OnStageSpritesTransformed;
        if (_currentMovie is BlingoMovie movie)
        {
            movie.PlayStateChanged -= OnPlayStateChanged;
            movie.Sprite2DListChanged -= SpriteListChanged;
        }
        _boundingBoxes.Dispose();
        _spriteSummary.Dispose();
        _motionPath.Dispose();
        base.OnDispose();
    }

    public bool CanExecute(StageToolSelectCommand command) => true;

    public bool Handle(StageToolSelectCommand command)
    {
        SelectedTool = command.Tool;
        return true;
    }

    public bool CanExecute(MoveSpritesCommand command) => true;

    public bool Handle(MoveSpritesCommand command)
    {
        foreach (var kv in command.EndPositions)
        {
            kv.Key.LocH = kv.Value.X;
            kv.Key.LocV = kv.Value.Y;
        }

        _historyManager.Push(command.ToUndo(UpdateSelectionBox), command.ToRedo(UpdateSelectionBox));
        UpdateSelectionBox();
        UpdateBoundingBoxes();
        return true;
    }

    public bool CanExecute(RotateSpritesCommand command) => true;

    public bool Handle(RotateSpritesCommand command)
    {
        foreach (var kv in command.EndRotations)
        {
            kv.Key.Rotation = kv.Value;
        }

        _historyManager.Push(command.ToUndo(UpdateSelectionBox), command.ToRedo(UpdateSelectionBox));
        UpdateSelectionBox();
        UpdateBoundingBoxes();
        return true;
    }

    protected override void OnResizing(bool firstResize, int width, int height)
    {
        base.OnResizing(firstResize, width, height);

        var iconHeight = (int)IconBar.Panel.Height;
        _scrollContainer.Width = width;
        _scrollContainer.Height = Math.Max(0, height - iconHeight);
        _scrollContainer.X = 0;
        _scrollContainer.Y = 0;
        IconBar.Panel.Width = width;
        IconBar.Panel.Y = _scrollContainer.Height;
        IconBar.Panel.X = 0;
        CenterScrollToStage();
    }

    protected override void OnRaiseKeyDown(AbstKeyEvent blingoKey)
    {
        base.OnRaiseKeyDown(blingoKey);
        if (_currentMovie != null && _currentMovie.IsPlaying) return;
        if (blingoKey.KeyPressed(AbstUIKeyType.SPACE))
        {
            _spaceHeld = true;
        }

        if (blingoKey.ControlDown)
        {
            _ctrlHeld = true;
        }
    }

    protected override void OnRaiseKeyUp(AbstKeyEvent blingoKey)
    {
        base.OnRaiseKeyUp(blingoKey);
        if (_currentMovie != null && _currentMovie.IsPlaying) return;
        if (!blingoKey.KeyPressed(AbstUIKeyType.SPACE))
        {
            _spaceHeld = false;
            _panning = false;
        }

        _ctrlHeld = blingoKey.ControlDown;
    }

    private void OnMouseDown(AbstMouseEvent e)
    {
        if (_currentMovie != null && _currentMovie.IsPlaying) return;
        if (_spaceHeld)
        {
            _panning = true;
            _lastMousePosition = new APoint(e.MouseH, e.MouseV);
            return;
        }

        if (!IsInsideStage(e))
        {
            return;
        }

        switch (SelectedTool)
        {
            case StageTool.Pointer:
                HandlePointerMouseDown(e);
                break;
            case StageTool.Move:
                _stageManager.BeginMove(ToStagePoint(e));
                break;
            case StageTool.Rotate:
                _stageManager.BeginRotate(ToStagePoint(e));
                break;
        }
    }

    private void HandlePointerMouseDown(AbstMouseEvent e)
    {
        if (_currentMovie is not BlingoMovie movie)
        {
            return;
        }

        var point = ToStagePoint(e);
        var sprite = movie.GetSpriteAtPoint(point.X, point.Y, skipLockedSprites: true) as BlingoSprite2D;
        if (sprite != null)
        {
            _stageManager.HandlePointerClick(sprite, _ctrlHeld);
        }
        else if (!_ctrlHeld)
        {
            _stageManager.ClearSelection();
        }

        UpdateSelectionBox();
        UpdateBoundingBoxes();
        UpdateMotionPath();
    }

    private void OnMouseUp(AbstMouseEvent e)
    {
        if (_currentMovie != null && _currentMovie.IsPlaying) return;
        if (_panning)
        {
            _panning = false;
            return;
        }

        switch (SelectedTool)
        {
            case StageTool.Move:
                _stageManager.EndMove(ToStagePoint(e));
                break;
            case StageTool.Rotate:
                _stageManager.EndRotate(ToStagePoint(e));
                break;
        }
    }

    private void OnMouseMove(AbstMouseEvent e)
    {
        if (_currentMovie != null && _currentMovie.IsPlaying) return;
        if (_panning)
        {
            var current = new APoint(e.MouseH, e.MouseV);
            var delta = current - _lastMousePosition;
            _scrollContainer.ScrollHorizontal -= delta.X;
            _scrollContainer.ScrollVertical -= delta.Y;
            _lastMousePosition = current;
            return;
        }

        switch (SelectedTool)
        {
            case StageTool.Move:
                _stageManager.UpdateMove(ToStagePoint(e));
                break;
            case StageTool.Rotate:
                _stageManager.UpdateRotate(ToStagePoint(e));
                break;
        }
    }

    private void OnMouseWheel(AbstMouseEvent e)
    {
        if (_currentMovie != null && _currentMovie.IsPlaying) return;
        if (!IsInsideStage(e))
        {
            return;
        }

        float delta = e.WheelDelta > 0 ? 0.1f : -0.1f;
        SetScale(_scale + delta);
    }

    private bool IsInsideStage(AbstMouseEvent e)
    {
        var point = ToStagePoint(e);
        return point.X >= 0 && point.Y >= 0 && point.X <= Stage.Width && point.Y <= Stage.Height;
    }

    private APoint ToStagePoint(AbstMouseEvent e)
    {
        var localX = _scrollContainer.ScrollHorizontal + e.MouseH - _stageLayer.X - StagePadding;
        var localY = _scrollContainer.ScrollVertical + e.MouseV - _stageLayer.Y - StagePadding;
        if (_scale != 0f)
        {
            localX /= _scale;
            localY /= _scale;
        }
        return new APoint(localX, localY);
    }

    private void OnPlayStateChanged(bool playing)
    {
        if (playing)
        {
            _selectionCanvas.Visibility = false;
        }
        else if (_stageManager.SelectedSprites.Count > 0)
        {
            UpdateSelectionBox();
        }

        UpdateBoundingBoxes();
        UpdateMotionPath();
    }

    private void SpriteListChanged(int obj)
    {
        UpdateBoundingBoxes();
        UpdateMotionPath();
    }

    private void OnStageSelectionChanged()
    {
        if (_currentMovie != null && !_currentMovie.IsPlaying && _stageManager.SelectedSprites.Count > 0)
        {
            UpdateSelectionBox();
        }
        else
        {
            _selectionCanvas.Visibility = false;
        }

        UpdateBoundingBoxes();
        UpdateMotionPath();
    }

    private void OnStageSpritesTransformed()
    {
        UpdateSelectionBox();
        UpdateBoundingBoxes();
        UpdateMotionPath();
    }

    private void UpdateStageLayout()
    {
        _stageBackgroundPanel.Width = Stage.Width;
        _stageBackgroundPanel.Height = Stage.Height;
        _stageBackgroundPanel.BackgroundColor = Stage.BackgroundColor;
        _stageBackgroundPanel.X = StagePadding;
        _stageBackgroundPanel.Y = StagePadding;

        Stage.X = StagePadding;
        Stage.Y = StagePadding;

        //_boundingBoxesCanvas.Width = Stage.Width;
        //_boundingBoxesCanvas.Height = Stage.Height;
        //_boundingBoxesCanvas.X = StagePadding;
        //_boundingBoxesCanvas.Y = StagePadding;

        //_motionPathCanvas.Width = Stage.Width;
        //_motionPathCanvas.Height = Stage.Height;
        //_motionPathCanvas.X = StagePadding;
        //_motionPathCanvas.Y = StagePadding;

        _guidesCanvas.Width = Stage.Width;
        _guidesCanvas.Height = Stage.Height;
        _guidesCanvas.X = StagePadding;
        _guidesCanvas.Y = StagePadding;

        //_selectionCanvas.Width = Stage.Width;
        //_selectionCanvas.Height = Stage.Height;
        //_selectionCanvas.X = StagePadding;
        //_selectionCanvas.Y = StagePadding;

        //_spriteSummaryCanvas.Width = Stage.Width;
        //_spriteSummaryCanvas.Height = Stage.Height;
        //_spriteSummaryCanvas.X = StagePadding;
        //_spriteSummaryCanvas.Y = StagePadding;

        _stageLayer.Width = Stage.Width + StagePadding * 2f;
        _stageLayer.Height = Stage.Height + StagePadding * 2f;

        _stageLayer.X = (ScrollAreaWidth - _stageLayer.Width) / 2f;
        _stageLayer.Y = (ScrollAreaHeight - _stageLayer.Height) / 2f;
        _guides.Draw();
        CenterScrollToStage();
    }

    private void CenterScrollToStage()
    {
        _scrollContainer.ScrollHorizontal = Math.Max(0, (ScrollAreaWidth - _scrollContainer.Width) / 2f);
        _scrollContainer.ScrollVertical = Math.Max(0, (ScrollAreaHeight - _scrollContainer.Height) / 2f);
    }

    private void OnColorChanged(AColor color)
    {
        _stageManager.ChangeBackgroundColor(color);
    }

    private void SetScale(float scale)
    {
        _scale = Math.Clamp(scale, IconBar.MinZoom, IconBar.MaxZoom);
        _zoomBox.ScaleH = _scale;
        _zoomBox.ScaleV = _scale;
        IconBar.SetZoom(_scale);
    }

    private void UpdateSelectionBox()
    {
        if (_stageManager.SelectedSprites.Count == 0)
        {
            _selectionCanvas.Visibility = false;
            return;
        }

        var rect = _stageManager.ComputeSelectionRect();
        _selectionCanvas.Clear(AColors.Transparent);
        _selectionCanvas.DrawRect(rect, AColors.Yellow, false, 1);
        _selectionCanvas.Visibility = true;
    }

    private void UpdateBoundingBoxes()
    {
        if (_currentMovie == null || _currentMovie.IsPlaying)
        {
            _boundingBoxes.Visible = false;
            _spriteSummary.Visible = false;
            return;
        }

        if (_stageManager.SelectedSprites.Count > 0)
        {
            _boundingBoxes.SetSprites(_stageManager.SelectedSprites);
            _boundingBoxes.Visible = true;
            _spriteSummary.Visible = true;
        }
        else
        {
            _boundingBoxes.Visible = false;
            _spriteSummary.Visible = false;
        }
    }

    private void UpdateMotionPath()
    {
        if (_currentMovie == null || _currentMovie.IsPlaying)
        {
            _motionPath.Draw(null);
            return;
        }

        var sprite = _stageManager.PrimarySelectedSprite;
        var path = sprite != null ? _stageManager.GetMotionPath(sprite) : null;
        _motionPath.Draw(path);
    }
}

/// <summary>
/// Retains the historical name so existing registrations keep working while
/// new logic lives in <see cref="DirectorStageWindowV2"/>.
/// </summary>
public class DirectorStageWindow : DirectorStageWindowV2
{
    public DirectorStageWindow(
        IServiceProvider serviceProvider,
        IHistoryManager historyManager,
        IBlingoFrameworkFactory factory,
        IAbstCommandManager commandManager,
        IBlingoPlayer player,
        IDirectorEventMediator mediator,
        DirStageManager stageManager,
        DirectorStageGuides guides,
        IDirectorIconManager iconManager)
        : base(serviceProvider, historyManager, factory, commandManager, player, mediator, stageManager, guides, iconManager)
    {
    }
}
