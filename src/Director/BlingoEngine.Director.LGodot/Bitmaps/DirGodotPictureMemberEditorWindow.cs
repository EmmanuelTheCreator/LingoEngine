using AbstEngine.Director.LGodot;
using AbstUI.Commands;
using AbstUI.FrameworkCommunication;
using AbstUI.Inputs;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Primitives;
using AbstUI.LGodot.Windowing;
using AbstUI.Primitives;
using Godot;
using BlingoEngine.Bitmaps;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Bitmaps;
using BlingoEngine.Director.Core.Bitmaps.Commands;
using BlingoEngine.Director.Core.Casts;
using BlingoEngine.Director.Core.Events;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.LGodot.Bitmaps;
using BlingoEngine.Events;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.LGodot.Bitmaps;
using BlingoEngine.Members;

namespace BlingoEngine.Director.LGodot.Pictures;

internal partial class DirGodotPictureMemberEditorWindow : BaseGodotWindow, IHasMemberSelectedEvent, IDirFrameworkBitmapEditWindow, IFrameworkFor<DirectorBitmapEditWindow>

{
    private const int NavigationBarHeight = 20;
    private const int IconBarHeight = 20;
    private const int BottomBarHeight = 20;
    private Vector2 _workAreaSize = new Vector2(2000, 2000); // fallback


    private readonly ScrollContainer _scrollContainer = new ScrollContainer();
    private readonly Control _centerContainer = new Control();
    private readonly ColorRect _background = new ColorRect();
    private readonly TextureRect _imageRect = new TextureRect();
    private readonly MemberNavigationBar<BlingoMemberBitmap> _navBar;
    private readonly HBoxContainer _iconBar = new HBoxContainer();
    private readonly HBoxContainer _bottomBar = new HBoxContainer();
    private readonly Button _flipHButton = new Button();
    private readonly Button _flipVButton = new Button();
    private readonly Button _toggleRegPointButton = new Button();
    private readonly HSlider _zoomSlider = new HSlider();
    private readonly OptionButton _scaleDropdown = new OptionButton();
    private readonly HSlider _brushSizeSlider = new HSlider();
    private readonly RegPointCanvas _regPointCanvas;
    private readonly SelectionCanvas _selectionCanvas;
    private readonly IDirectorEventMediator _mediator;
    private readonly IBlingoPlayer _player;
    private readonly IDirectorIconManager _iconManager;
    private readonly IAbstCommandManager _commandManager;
    private readonly IHistoryManager _historyManager;
    private BlingoMemberBitmap? _member;
    private bool _showRegPoint = true;
    private PicturePainter? _painter;
    private readonly PaintToolbar _paintToolbar;
    private int _brushSize = 1;
    private BlingoBitmapSelection _selection = new();
    private Vector2I _selectStart;
    private Vector2 _lastMousePos;


    private readonly float[] _zoomLevels = new float[]
        {
            0.25f, 0.33f, 0.5f, 0.66f, 0.75f, 1.0f, 1.25f, 1.5f, 2.0f, 2.5f, 3.0f, 4.0f
        };
    private float _scale = 1f;
    private bool _spaceHeld;
    private bool _panning;
    private bool _drawing;

    public DirGodotPictureMemberEditorWindow(IServiceProvider serviceProvider, IDirectorEventMediator mediator, IBlingoPlayer player, IAbstGodotWindowManager windowManager, DirectorBitmapEditWindow directorPictureEditWindow, IDirectorIconManager iconManager, IAbstCommandManager commandManager, IHistoryManager historyManager, IBlingoFrameworkFactory factory)
        : base("Picture Editor", serviceProvider)
    {
        _mediator = mediator;
        _player = player;
        _iconManager = iconManager;
        _commandManager = commandManager;
        _historyManager = historyManager;
        _mediator.Subscribe(this);
        Init(directorPictureEditWindow);


        _navBar = new MemberNavigationBar<BlingoMemberBitmap>(_mediator, _player, _iconManager, factory, NavigationBarHeight);
        AddChild(_navBar.Panel.Framework<AbstGodotWrapPanel>());
        _navBar.Panel.X = 0;
        _navBar.Panel.Y = TitleBarHeight;
        _navBar.Panel.Width = Size.X;
        _navBar.Panel.Height = NavigationBarHeight;

        // Icon bar below navigation
        AddChild(_iconBar);
        _iconBar.Position = new Vector2(150, TitleBarHeight + NavigationBarHeight + 5);
        _iconBar.CustomMinimumSize = new Vector2(Size.X, IconBarHeight);
        _paintToolbar = new PaintToolbar(_iconManager, _commandManager, factory);
        var toolbarPanel = _paintToolbar.Panel.Framework<AbstGodotPanel>();
        AddChild(toolbarPanel);
        toolbarPanel.Position = new Vector2(0, TitleBarHeight + NavigationBarHeight + 5 + IconBarHeight);

        StyleIconButton(_flipHButton, DirectorIcon.FlipHorizontal);
        _flipHButton.Pressed += OnFlipH;
        _iconBar.AddChild(_flipHButton);


        StyleIconButton(_flipVButton, DirectorIcon.FlipVertical);
        _flipVButton.Pressed += OnFlipV;
        _iconBar.AddChild(_flipVButton);


        StyleIconButton(_toggleRegPointButton, DirectorIcon.Crosshair);
        _toggleRegPointButton.ToggleMode = true;
        _toggleRegPointButton.ButtonPressed = true;
        _toggleRegPointButton.Toggled += pressed =>
        {
            _showRegPoint = pressed;
            _regPointCanvas.Visible = pressed;
            RedrawRegPointCanvas();
        };
        _iconBar.AddChild(_toggleRegPointButton);
        var applyButton = new Button { Text = "Apply", CustomMinimumSize = new Vector2(60, IconBarHeight) };
        applyButton.Pressed += ApplyPaintingToMember;
        _iconBar.AddChild(applyButton);

        _brushSizeSlider.MinValue = 1;
        _brushSizeSlider.MaxValue = 20;
        _brushSizeSlider.Step = 1;
        _brushSizeSlider.Value = _brushSize;
        _brushSizeSlider.CustomMinimumSize = new Vector2(100, IconBarHeight);
        _brushSizeSlider.Visible = false;
        _brushSizeSlider.ValueChanged += v => _brushSize = (int)v;
        _iconBar.AddChild(_brushSizeSlider);



        // Image display container with scrollbars
        AddChild(_scrollContainer);
        _scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _scrollContainer.VerticalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _scrollContainer.AnchorLeft = 0;
        _scrollContainer.AnchorTop = 0;
        _scrollContainer.AnchorRight = 1;
        _scrollContainer.AnchorBottom = 1;
        // Offset the scroll area by the toolbar width so the toolbar isn't overlapped.
        // Using `Size` here resulted in 0 during initial layout because the
        // control hasn't been measured yet, causing the scroll container to
        // cover the toolbar and consume its input. Rely on the toolbar's
        // configured minimum width instead.
        _scrollContainer.OffsetLeft = _paintToolbar.Panel.Width;
        _scrollContainer.OffsetTop = TitleBarHeight + NavigationBarHeight + IconBarHeight + 10;
        _scrollContainer.OffsetRight = 0;
        _scrollContainer.OffsetBottom = -BottomBarHeight;
        _scrollContainer.GuiInput += (InputEvent @event) =>
        {
            if (@event is InputEventMouseButton mb &&
                (mb.ButtonIndex == MouseButton.WheelUp || mb.ButtonIndex == MouseButton.WheelDown))
            {
                GetViewport().SetInputAsHandled();
            }
        };


        _scrollContainer.AddChild(_centerContainer);
        _centerContainer.CustomMinimumSize = Vector2.Zero;
        _centerContainer.AnchorLeft = 0.5f;
        _centerContainer.AnchorTop = 0.5f;
        _centerContainer.AnchorRight = 0.5f;
        _centerContainer.AnchorBottom = 0.5f;
        _centerContainer.OffsetLeft = 0;
        _centerContainer.OffsetTop = 0;
        _centerContainer.OffsetRight = 0;
        _centerContainer.OffsetBottom = 0;
        _centerContainer.PivotOffset = Vector2.Zero;



        _background.Color = Colors.White;
        _background.AnchorLeft = 0;
        _background.AnchorTop = 0;
        _background.AnchorRight = 1;
        _background.AnchorBottom = 1;
        _background.OffsetLeft = 0;
        _background.OffsetTop = 0;
        _background.OffsetRight = 0;
        _background.OffsetBottom = 0;
        _centerContainer.AddChild(_background);

        // Use Scale stretch mode so the image properly enlarges with the zoom
        // slider. Using "Keep" caused the texture to remain at its original
        // size when the parent container was scaled.
        _imageRect.StretchMode = TextureRect.StretchModeEnum.Scale;
        // Allow the texture to resize with its container. Godot 4+ renamed the
        // property from `Expand` to `ExpandMode`. Use reflection to set either
        // if available so the code works across engine versions.
        if (_imageRect.HasMethod("set_expand_mode"))
            _imageRect.Set("expand_mode", 1); // ExpandModeEnum.Expand
        else if (_imageRect.HasMethod("set_expand"))
            _imageRect.Set("expand", true);
        _imageRect.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _imageRect.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _imageRect.AnchorLeft = 0.5f;
        _imageRect.AnchorTop = 0.5f;
        _imageRect.AnchorRight = 0.5f;
        _imageRect.AnchorBottom = 0.5f;
        _centerContainer.AddChild(_imageRect);

        _regPointCanvas = new RegPointCanvas(factory);
        var regNode = _regPointCanvas.Canvas.Framework<AbstGodotGfxCanvas>();
        regNode.AnchorLeft = 0.5f;
        regNode.AnchorTop = 0.5f;
        regNode.AnchorRight = 0.5f;
        regNode.AnchorBottom = 0.5f;
        _regPointCanvas.Visible = true;
        _scrollContainer.AddChild(regNode);

        _selectionCanvas = new SelectionCanvas(factory);
        var selNode = _selectionCanvas.Canvas.Framework<AbstGodotGfxCanvas>();
        selNode.AnchorLeft = 0.5f;
        selNode.AnchorTop = 0.5f;
        selNode.AnchorRight = 0.5f;
        selNode.AnchorBottom = 0.5f;
        _selectionCanvas.Visible = true;
        _scrollContainer.AddChild(selNode);

        // Bottom zoom bar
        AddChild(_bottomBar);
        _bottomBar.Position = new Vector2(0, Size.Y - BottomBarHeight);
        _bottomBar.CustomMinimumSize = new Vector2(Size.X, BottomBarHeight);

        _zoomSlider.MinValue = 0;
        _zoomSlider.MaxValue = _zoomLevels.Length - 1;
        _zoomSlider.Step = 1;
        _zoomSlider.Value = Array.IndexOf(_zoomLevels, 1.0f); // 100%
        _zoomSlider.ValueChanged += index =>
        {
            var value = _zoomLevels[(int)index];
            OnZoomChanged(value);
        };

        _bottomBar.AddChild(_zoomSlider);

        _scaleDropdown.CustomMinimumSize = new Vector2(60, BottomBarHeight);
        for (int i = 0; i < _zoomLevels.Length; i++)
        {
            int percent = Mathf.RoundToInt(_zoomLevels[i] * 100);
            _scaleDropdown.AddItem($"{percent}%");
            if (_zoomLevels[i] == 1.0f)
                _scaleDropdown.Select(i);
        }


        _scaleDropdown.ItemSelected += id => OnScaleSelected(id);
        _bottomBar.AddChild(_scaleDropdown);

        MouseT.OnMouseDown(OnMouseDown);
        MouseT.OnMouseUp(OnMouseUp);
        MouseT.OnMouseMove(OnMouseMove);
        MouseT.OnMouseEvent(OnMouseEvent);
    }
    private void StyleIconButton(Button button, DirectorIcon icon)
    {
        button.Icon = ((AbstGodotTexture2D)_iconManager.Get(icon)).Texture;
        button.CustomMinimumSize = new Vector2(20, IconBarHeight);
        button.AddThemeStyleboxOverride("normal", new StyleBoxFlat
        {
            BgColor = Colors.Transparent,
            BorderWidthBottom = 0,
            BorderWidthTop = 0,
            BorderWidthLeft = 0,
            BorderWidthRight = 0
        });
    }
    private void ResetView()
    {
        _panning = false;
        _spaceHeld = false;
        _imageRect.FlipH = false;
        _imageRect.FlipV = false;
        _scrollContainer.ScrollHorizontal = 0;
        _scrollContainer.ScrollVertical = 0;
        _zoomSlider.Value = 1f;
        OnZoomChanged(1f);
    }

    public void SetPicture(BlingoMemberBitmap picture)
    {
        bool firstLoad = _member == null;
        var godotPicture = picture.Framework<BlingoGodotMemberBitmap>();
        godotPicture.Preload();
        var texture2D = godotPicture.TextureBlingo as AbstGodotTexture2D;


        if (texture2D != null && texture2D.Texture is ImageTexture tex)
        {
            _painter?.Dispose();

            var image = godotPicture.GetImageCopy();
            _painter = new PicturePainter(image);
            _imageRect.Texture = _painter.Texture;

            Vector2 imageSize = new(godotPicture.Width, godotPicture.Height);
            _imageRect.Texture = _painter.Texture;
            _imageRect.StretchMode = TextureRect.StretchModeEnum.Keep;
            _imageRect.CustomMinimumSize = imageSize;
            _imageRect.Size = imageSize;
            _imageRect.Position = -imageSize / 2f;
            _imageRect.PivotOffset = imageSize / 2f;
            _imageRect.OffsetLeft = -imageSize.X / 2f;
            _imageRect.OffsetTop = -imageSize.Y / 2f;
            _imageRect.OffsetRight = imageSize.X / 2f;
            _imageRect.OffsetBottom = imageSize.Y / 2f;

            PixelPerfectMaterial.ApplyTo(_imageRect);

            _workAreaSize = imageSize + new Vector2(2000, 2000);
            _centerContainer.CustomMinimumSize = _workAreaSize * _scale;
            _centerContainer.PivotOffset = _centerContainer.CustomMinimumSize / 2f;

            UpdateRegPointCanvasSize();

            if (firstLoad)
            {
                FitImageToView();
            }
            else
            {
                _zoomSlider.Value = _scale;
                OnZoomChanged(_scale);
            }

            CallDeferred(nameof(CenterImage));
        }

        _member = picture;
        _navBar.SetMember(picture);
        RedrawRegPointCanvas();
        RedrawSelectionCanvas();
    }

    private void FitImageToView()
    {
        var texture = _imageRect.Texture;
        if (texture == null) return;
        Vector2 areaSize = _scrollContainer.Size;
        if (areaSize == Vector2.Zero)
            areaSize = new Vector2(Size.X, Size.Y - (TitleBarHeight + IconBarHeight + BottomBarHeight));
        float factor = Math.Min(areaSize.X / texture.GetWidth(), areaSize.Y / texture.GetHeight());
        factor = Math.Min(1f, factor); // don't upscale on initial fit
        factor = (float)Mathf.Clamp(factor, _zoomSlider.MinValue, _zoomSlider.MaxValue);
        _zoomSlider.Value = factor;
        OnZoomChanged(factor);
        CenterImage();
    }

    private void CenterImage()
    {
        Vector2 view = _scrollContainer.Size;
        if (view == Vector2.Zero)
            view = new Vector2(Size.X, Size.Y - (TitleBarHeight + IconBarHeight + BottomBarHeight));

        if (_member == null)
            return;

        // Calculate the scaled work area size
        Vector2 canvasSize = _centerContainer.CustomMinimumSize * _scale;

        // Determine the position of the registration point within the canvas
        Vector2 canvasHalf = _centerContainer.CustomMinimumSize / 2f;
        Vector2 imageHalf = _imageRect.CustomMinimumSize / 2f;
        Vector2 regOffset = canvasHalf - imageHalf + new Vector2(_member.RegPoint.X, _member.RegPoint.Y);
        Vector2 regPos = regOffset * _scale;

        // Desired scroll positions so the reg point is centered in view
        float desiredH = regPos.X - view.X / 2f;
        float desiredV = regPos.Y - view.Y / 2f;

        int maxH = (int)Mathf.Max(0, canvasSize.X - view.X);
        int maxV = (int)Mathf.Max(0, canvasSize.Y - view.Y);

        _scrollContainer.ScrollHorizontal = (int)Mathf.Clamp(desiredH, 0, maxH);
        _scrollContainer.ScrollVertical = (int)Mathf.Clamp(desiredV, 0, maxV);
    }

    private void UpdateRegPointCanvasSize()
    {
        Vector2 view = _scrollContainer.Size;
        if (view == Vector2.Zero)
            view = new Vector2(Size.X, Size.Y - (TitleBarHeight + IconBarHeight + BottomBarHeight));
        Vector2 unscaled = view / _scale;
        float w = Mathf.Max(_centerContainer.CustomMinimumSize.X, unscaled.X);
        float h = Mathf.Max(_centerContainer.CustomMinimumSize.Y, unscaled.Y);
        _regPointCanvas.Canvas.Width = w;
        _regPointCanvas.Canvas.Height = h;
        var regNode = _regPointCanvas.Canvas.Framework<AbstGodotGfxCanvas>();
        regNode.OffsetLeft = -w / 2f;
        regNode.OffsetTop = -h / 2f;
        regNode.OffsetRight = w / 2f;
        regNode.OffsetBottom = h / 2f;
        _selectionCanvas.Canvas.Width = w;
        _selectionCanvas.Canvas.Height = h;
        var selNode = _selectionCanvas.Canvas.Framework<AbstGodotGfxCanvas>();
        selNode.OffsetLeft = -w / 2f;
        selNode.OffsetTop = -h / 2f;
        selNode.OffsetRight = w / 2f;
        selNode.OffsetBottom = h / 2f;
        _selection = new BlingoBitmapSelection();
    }

    private void RedrawRegPointCanvas()
    {
        if (_member != null)
            _regPointCanvas.Draw(_member, _scale);
        else
            _regPointCanvas.Canvas.Clear(AColors.Transparent);
    }

    private void RedrawSelectionCanvas()
    {
        if (_member != null)
        {
            _selectionCanvas.Draw(_member, _selection, _scale);
        }
        else
        {
            _selectionCanvas.Canvas.Clear(AColors.Transparent);
        }
    }

    protected override void OnResizing(bool firstResize, Vector2 size)
    {
        base.OnResizing(firstResize, size);
        _navBar.Panel.Width = size.X;
        _navBar.Panel.Height = NavigationBarHeight;
        _iconBar.Position = new Vector2(0, NavigationBarHeight + TitleBarHeight);
        _iconBar.CustomMinimumSize = new Vector2(size.X, IconBarHeight);
        _bottomBar.Position = new Vector2(0, size.Y - BottomBarHeight);
        _bottomBar.CustomMinimumSize = new Vector2(size.X, BottomBarHeight);

        _scrollContainer.OffsetTop = TitleBarHeight + NavigationBarHeight + IconBarHeight;
        _scrollContainer.OffsetBottom = -BottomBarHeight;
        // Keep the scroll container offset by the toolbar width when the window
        // is resized so it doesn't overlap the toolbar.
        _scrollContainer.OffsetLeft = _paintToolbar.Panel.Width;
        _scrollContainer.OffsetRight = 0;
        _centerContainer.PivotOffset = _centerContainer.CustomMinimumSize / 2f;
        UpdateRegPointCanvasSize();
        CenterImage();
        RedrawRegPointCanvas();
        RedrawSelectionCanvas();
    }
    private void OnZoomChanged(float value)
    {
        Vector2 viewSize = _scrollContainer.Size;
        if (viewSize == Vector2.Zero)
            viewSize = new Vector2(Size.X, Size.Y - (TitleBarHeight + IconBarHeight + BottomBarHeight));


        float oldScale = _scale;

        Vector2 globalMouse = GetGlobalMousePosition();
        Vector2 localMouse = _scrollContainer.GetGlobalTransform().AffineInverse() * globalMouse;
        Vector2 canvasMouse = localMouse + new Vector2(_scrollContainer.ScrollHorizontal, _scrollContainer.ScrollVertical);

        Vector2 canvasOrigin = _workAreaSize * oldScale / 2f;
        Vector2 logicalPoint = (canvasMouse - canvasOrigin) / oldScale;

        _scale = value;
        _imageRect.Scale = new Vector2(_scale, _scale);
        _imageRect.Size = _imageRect.CustomMinimumSize * _scale;
        _imageRect.OffsetTop = -_imageRect.CustomMinimumSize.Y / 2f;
        _imageRect.OffsetBottom = _imageRect.CustomMinimumSize.Y / 2f;

        _centerContainer.CustomMinimumSize = _workAreaSize * _scale;
        _centerContainer.PivotOffset = _centerContainer.CustomMinimumSize / 2f;

        UpdateRegPointCanvasSize();
        RedrawRegPointCanvas();
        RedrawSelectionCanvas();

        Vector2 newCanvasSize = _centerContainer.CustomMinimumSize;
        Vector2 newOrigin = newCanvasSize / 2f;
        Vector2 newScroll = logicalPoint * _scale + newOrigin - localMouse;

        int maxH = (int)Mathf.Max(0, newCanvasSize.X - viewSize.X);
        int maxV = (int)Mathf.Max(0, newCanvasSize.Y - viewSize.Y);

        _scrollContainer.ScrollHorizontal = (int)Mathf.Clamp(newScroll.X, 0, maxH);
        _scrollContainer.ScrollVertical = (int)Mathf.Clamp(newScroll.Y, 0, maxV);

        // Find the closest matching zoom level index
        int closestIndex = 0;
        float smallestDiff = float.MaxValue;
        for (int i = 0; i < _zoomLevels.Length; i++)
        {
            float diff = Mathf.Abs(_zoomLevels[i] - _scale);
            if (diff < smallestDiff)
            {
                smallestDiff = diff;
                closestIndex = i;
            }
        }
        _scaleDropdown.Select(closestIndex);
    }


    private void OnScaleSelected(long id)
    {
        var text = _scaleDropdown.GetItemText((int)id);
        if (text.EndsWith("%") && float.TryParse(text.TrimEnd('%'), out var percent))
        {
            var newScale = percent / 100f;
            _zoomSlider.Value = newScale;
            OnZoomChanged(newScale);
            // Clear focus from dropdown so Spacebar works for panning
            _scaleDropdown.ReleaseFocus();
        }
    }
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (base._dragging || !Visible)
            return;

        if (@event is InputEventKey keyEvent && keyEvent.Keycode == Key.Space)
        {
            SpaceBarPress(keyEvent);
        }
    }

    private void OnMouseDown(AbstMouseEvent e)
    {
        if (!IsEventInScrollArea() || !e.Mouse.LeftMouseDown)
            return;

        if (_spaceHeld)
        {
            _panning = true;
            _lastMousePos = new Vector2(e.Mouse.MouseH, e.Mouse.MouseV);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_paintToolbar.SelectedTool == PainterToolType.SelectRectangle)
            StartRectangleSelection();
        else if (_paintToolbar.SelectedTool == PainterToolType.SelectLasso)
            StartLassoSelection();
        else if (_painter != null && _imageRect.Texture != null)
        {
            _drawing = true;
            DrawingPixels();
        }

        GetViewport().SetInputAsHandled();
    }

    private void OnMouseUp(AbstMouseEvent e)
    {
        if (!IsEventInScrollArea())
            return;

        bool ctrl = Input.IsKeyPressed(Key.Ctrl);
        bool shift = Input.IsKeyPressed(Key.Shift);

        if (_selection.IsDragSelecting && _paintToolbar.SelectedTool == PainterToolType.SelectRectangle)
            FinishRectangleSelection(ctrl, shift);
        else if (_selection.IsLassoSelecting && _paintToolbar.SelectedTool == PainterToolType.SelectLasso)
            FinishLassoSelection(ctrl, shift);
        else
            _panning = false;

        _drawing = false;
        GetViewport().SetInputAsHandled();
    }

    private void OnMouseMove(AbstMouseEvent e)
    {
        if (!IsEventInScrollArea())
            return;

        var current = new Vector2(e.Mouse.MouseH, e.Mouse.MouseV);

        if (_selection.IsDragSelecting && _paintToolbar.SelectedTool == PainterToolType.SelectRectangle)
        {
            var local = _imageRect.GetLocalMousePosition();
            var end = new Vector2I((int)local.X, (int)local.Y);
            _selection.UpdateRectSelection(
                new APoint(_selectStart.X, _selectStart.Y),
                new APoint(end.X, end.Y));
            RedrawSelectionCanvas();
            GetViewport().SetInputAsHandled();
            return;
        }
        else if (_selection.IsLassoSelecting && _paintToolbar.SelectedTool == PainterToolType.SelectLasso)
        {
            var local = _imageRect.GetLocalMousePosition();
            var point = new Vector2I((int)local.X, (int)local.Y);
            _selection.AddLassoPoint(point.X, point.Y);
            RedrawSelectionCanvas();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_panning)
        {
            var rel = current - _lastMousePos;
            _scrollContainer.ScrollHorizontal -= (int)rel.X;
            _scrollContainer.ScrollVertical -= (int)rel.Y;
            _lastMousePos = current;
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_drawing)
        {
            DrawingPixels();
            GetViewport().SetInputAsHandled();
        }
    }

    private void OnMouseEvent(AbstMouseEvent e)
    {
        if (e.Type == AbstMouseEventType.MouseWheel && IsEventInScrollArea())
        {
            ZoomingWithMouseScroll(e.WheelDelta);
        }
    }

    private bool IsEventInScrollArea()
    {
        Rect2 bounds = new Rect2(_scrollContainer.GlobalPosition, _scrollContainer.Size);
        Vector2 mousePos = GetGlobalMousePosition();
        return bounds.HasPoint(mousePos);
    }

    private void ZoomingWithMouseScroll(float delta)
    {
        // Apply zoom factor per scroll step (e.g. 25% per notch)
        const float zoomStep = 1.25f;
        float factor = delta > 0 ? zoomStep : 1f / zoomStep;

        float rawScale = _scale * factor;

        // Clamp scale within limits
        float minScale = _zoomLevels.First();
        float maxScale = _zoomLevels.Last();
        float newScale = Mathf.Clamp(rawScale, minScale, maxScale);

        // Update controls and image
        _zoomSlider.Value = newScale;
        OnZoomChanged(newScale);
        GetViewport().SetInputAsHandled();
    }

    private void FinishRectangleSelection(bool ctrlPressed, bool shiftPressed)
    {
        var local = _imageRect.GetLocalMousePosition();
        var end = new Vector2I((int)local.X, (int)local.Y);
        var rect = APoint.RectFromPoints(
            new APoint(_selectStart.X, _selectStart.Y),
            new APoint(end.X, end.Y));
        _selection.ApplySelection(rect, ctrlPressed, shiftPressed);
        _selection.EndRectSelection();
        RedrawSelectionCanvas();
    }

    private void DrawingPixels()
    {
        var local = _imageRect.GetLocalMousePosition();
        var pixel = new Vector2I((int)local.X, (int)local.Y);
        _commandManager.Handle(new PainterDrawPixelCommand(pixel.X, pixel.Y));
    }

    private void StartRectangleSelection()
    {
        if (_painter != null)
        {
            var local = _imageRect.GetLocalMousePosition();
            _selectStart = new Vector2I((int)local.X, (int)local.Y);
            _selection.BeginRectSelection();
            RedrawSelectionCanvas();
        }
    }

    private void StartLassoSelection()
    {
        if (_painter != null)
        {
            var local = _imageRect.GetLocalMousePosition();
            _selection.BeginLassoSelection(new APoint((int)local.X, (int)local.Y));
            RedrawSelectionCanvas();
        }
    }
    private void FinishLassoSelection(bool ctrlPressed, bool shiftPressed)
    {
        var polygon = _selection.EndLassoSelection();
        if (polygon.Count > 2)
        {
            var pixels = APoint.PointsInsidePolygon(polygon);
            _selection.ApplySelection(pixels, ctrlPressed, shiftPressed);
        }
        RedrawSelectionCanvas();
    }


    private void SpaceBarPress(InputEventKey keyEvent)
    {
        _spaceHeld = keyEvent.Pressed;
        if (!keyEvent.Pressed)
            _panning = false;
    }

    public void MemberSelected(IBlingoMember member)
    {
        if (member is BlingoMemberBitmap pic)
            SetPicture(pic);
    }

    private void OnFlipH()
    {
        _imageRect.FlipH = !_imageRect.FlipH;
    }

    private void OnFlipV()
    {
        _imageRect.FlipV = !_imageRect.FlipV;
    }


    private void RefreshImageSize()
    {
        if (_painter == null)
            return;

        Vector2 imageSize = new(_painter.Size.X, _painter.Size.Y);
        _imageRect.CustomMinimumSize = imageSize;
        _imageRect.Size = imageSize * _scale;
        _imageRect.OffsetLeft = -imageSize.X / 2f;
        _imageRect.OffsetTop = -imageSize.Y / 2f;
        _imageRect.OffsetRight = imageSize.X / 2f;
        _imageRect.OffsetBottom = imageSize.Y / 2f;

        _workAreaSize = imageSize + new Vector2(2000, 2000);
        _centerContainer.CustomMinimumSize = _workAreaSize * _scale;
        _centerContainer.PivotOffset = _centerContainer.CustomMinimumSize / 2f;
        UpdateRegPointCanvasSize();
    }
    protected override void Dispose(bool disposing)
    {
        _painter?.Dispose();
        _painter = null;
        _mediator.Unsubscribe(this);
        base.Dispose(disposing);
    }

    private void ApplyPaintingToMember()
    {
        if (_painter == null || _member == null)
            return;

        _painter.Commit(); // Make sure texture is synced
        var image = _painter.GetImage(); // You’ll need to expose this

        var godotPicture = _member.Framework<BlingoGodotMemberBitmap>();
        godotPicture.ApplyImage(image);

        GD.Print("Changes applied to member.");
    }

    public bool SelectTheTool(PainterToolType tool)
    {
        _paintToolbar.SelectTool(tool);
        OnToolSelected(tool);
        return true;
    }
    private void OnToolSelected(PainterToolType tool)
    {
        _brushSizeSlider.Visible = tool == PainterToolType.PaintBrush;
    }

    public bool DrawThePixel(int x, int y)
    {
        if (_painter == null) return false;

        var beforeImage = _painter.GetImage();
        var beforeOffset = _painter.Offset;
        var oldReg = _member?.RegPoint ?? new APoint(0, 0);

        var pixel = new Vector2I(x, y);
        if (_paintToolbar.SelectedTool == PainterToolType.Eraser)
            _painter.ErasePixel(pixel);
        else if (_paintToolbar.SelectedTool == PainterToolType.PaintBrush)
            _painter.PaintBrush(pixel, _paintToolbar.SelectedColor.ToGodotColor(), _brushSize);
        else
            _painter.PaintPixel(pixel, _paintToolbar.SelectedColor.ToGodotColor());

        var delta = _painter.Offset - beforeOffset;
        if (delta != Vector2I.Zero && _member != null)
            _member.RegPoint = new APoint(_member.RegPoint.X + delta.X, _member.RegPoint.Y + delta.Y);

        _painter.Commit();
        var afterImage = _painter.GetImage();
        var afterOffset = _painter.Offset;
        var newReg = _member?.RegPoint ?? oldReg;
        _imageRect.Texture = _painter.Texture;
        if (delta != Vector2I.Zero)
        {
            RefreshImageSize();
            // Keep the registration point under the cursor when the bitmap
            // expands by adjusting the scroll offsets by the padded amount.
            _scrollContainer.ScrollHorizontal += (int)(delta.X * _scale);
            _scrollContainer.ScrollVertical += (int)(delta.Y * _scale);
        }
        RedrawRegPointCanvas();

        _historyManager.Push(() =>
        {
            if (_painter == null) return;
            _painter.SetState(beforeImage, beforeOffset);
            _imageRect.Texture = _painter.Texture;
            if (_member != null)
                _member.RegPoint = oldReg;
            RefreshImageSize();
            RedrawRegPointCanvas();
        },
        () =>
        {
            if (_painter == null) return;
            _painter.SetState(afterImage, afterOffset);
            _imageRect.Texture = _painter.Texture;
            if (_member != null)
                _member.RegPoint = newReg;
            RefreshImageSize();
            RedrawRegPointCanvas();
        });

        return true;
    }




}


