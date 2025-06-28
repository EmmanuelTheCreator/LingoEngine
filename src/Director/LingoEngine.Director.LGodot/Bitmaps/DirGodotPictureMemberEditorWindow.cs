using Godot;
using LingoEngine.Pictures;
using LingoEngine.LGodot.Pictures;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.Core.Events;
using LingoEngine.Members;
using LingoEngine.Core;
using LingoEngine.Primitives;
using LingoEngine.Movies;
using System.Linq;
using System;
using System.Collections.Generic;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.LGodot.Icons;
using LingoEngine.Director.Core.Pictures;
using LingoEngine.Director.Core.Gfx;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Pictures.Commands;
using LingoEngine.Director.Core.Bitmaps;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Icons;

namespace LingoEngine.Director.LGodot.Pictures;

internal partial class DirGodotPictureMemberEditorWindow : BaseGodotWindow, IHasMemberSelectedEvent, IDirFrameworkBitmapEditWindow
  
{
    private const int NavigationBarHeight = 20;
    private const int IconBarHeight = 20;
    private const int BottomBarHeight = 20;
    private Vector2 _workAreaSize = new Vector2(2000, 2000); // fallback


    private readonly ScrollContainer _scrollContainer = new ScrollContainer();
    private readonly Control _centerContainer = new Control();
    private readonly ColorRect _background = new ColorRect();
    private readonly TextureRect _imageRect = new TextureRect();
    private readonly MemberNavigationBar<LingoMemberBitmap> _navBar;
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
    private readonly ILingoPlayer _player;
    private readonly IDirectorIconManager _iconManager;
    private readonly ILingoCommandManager _commandManager;
    private readonly IHistoryManager _historyManager;
    private LingoMemberBitmap? _member;
    private bool _showRegPoint = true;
    private PicturePainter? _painter;
    private readonly PaintToolbar _paintToolbar;
    private int _brushSize = 1;
    private readonly HashSet<Vector2I> _selectedPixels = new();
    private bool _dragSelecting;
    private Vector2I _selectStart;
    private Rect2I? _dragRect;


    private readonly float[] _zoomLevels = new float[]
        {
            0.25f, 0.33f, 0.5f, 0.66f, 0.75f, 1.0f, 1.25f, 1.5f, 2.0f, 2.5f, 3.0f, 4.0f
        };
    private float _scale = 1f;
    private bool _spaceHeld;
    private bool _panning;

    public DirGodotPictureMemberEditorWindow(IDirectorEventMediator mediator, ILingoPlayer player, IDirGodotWindowManager windowManager, DirectorBitmapEditWindow directorPictureEditWindow, IDirectorIconManager iconManager, ILingoCommandManager commandManager, IHistoryManager historyManager)
        : base(DirectorMenuCodes.PictureEditWindow, "Picture Editor", windowManager)
    {
        _mediator = mediator;
        _player = player;
        _iconManager = iconManager;
        _commandManager = commandManager;
        _historyManager = historyManager;
        _mediator.Subscribe(this);
        Size = new Vector2(800, 500);
        directorPictureEditWindow.Init(this);
        CustomMinimumSize = Size;

        _navBar = new MemberNavigationBar<LingoMemberBitmap>(_mediator, _player, _iconManager, NavigationBarHeight);
        AddChild(_navBar);
        _navBar.Position = new Vector2(0, TitleBarHeight);
        _navBar.CustomMinimumSize = new Vector2(Size.X, NavigationBarHeight);

        // Icon bar below navigation
        AddChild(_iconBar);
        _iconBar.Position = new Vector2(0, TitleBarHeight + NavigationBarHeight);
        _iconBar.CustomMinimumSize = new Vector2(Size.X, IconBarHeight);
        _paintToolbar = new PaintToolbar(_iconManager, _commandManager);
        AddChild(_paintToolbar);
        _paintToolbar.Position = new Vector2(0, TitleBarHeight + NavigationBarHeight + IconBarHeight);
        _paintToolbar.ToolSelected += OnToolSelected;


        _flipHButton.Text = "Flip H";
        _flipHButton.CustomMinimumSize = new Vector2(60, IconBarHeight);
        _flipHButton.Pressed += OnFlipH;
        _iconBar.AddChild(_flipHButton);

        _flipVButton.Text = "Flip V";
        _flipVButton.CustomMinimumSize = new Vector2(60, IconBarHeight);
        _flipVButton.Pressed += OnFlipV;
        _iconBar.AddChild(_flipVButton);

        _toggleRegPointButton.Text = "Reg";
        _toggleRegPointButton.ToggleMode = true;
        _toggleRegPointButton.ButtonPressed = true;
        _toggleRegPointButton.CustomMinimumSize = new Vector2(40, IconBarHeight);
        _toggleRegPointButton.Toggled += pressed =>
        {
            _showRegPoint = pressed;
            _regPointCanvas.Visible = pressed;
            _regPointCanvas.QueueRedraw();
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
        _scrollContainer.OffsetLeft = _paintToolbar.CustomMinimumSize.X;
        _scrollContainer.OffsetTop = TitleBarHeight + NavigationBarHeight + IconBarHeight;
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

        _regPointCanvas = new RegPointCanvas(this);
        _regPointCanvas.AnchorLeft = 0.5f;
        _regPointCanvas.AnchorTop = 0.5f;
        _regPointCanvas.AnchorRight = 0.5f;
        _regPointCanvas.AnchorBottom = 0.5f;
        _regPointCanvas.Visible = true;
        _scrollContainer.AddChild(_regPointCanvas);

        _selectionCanvas = new SelectionCanvas(this);
        _selectionCanvas.AnchorLeft = 0.5f;
        _selectionCanvas.AnchorTop = 0.5f;
        _selectionCanvas.AnchorRight = 0.5f;
        _selectionCanvas.AnchorBottom = 0.5f;
        _selectionCanvas.Visible = true;
        _scrollContainer.AddChild(_selectionCanvas);

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

    public void SetPicture(LingoMemberBitmap picture)
    {
        bool firstLoad = _member == null;
        var godotPicture = picture.Framework<LingoGodotMemberBitmap>();
        godotPicture.Preload();

        if (godotPicture.TextureGodot is ImageTexture tex)
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
        _regPointCanvas.QueueRedraw();
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
        _regPointCanvas.CustomMinimumSize = new Vector2(w, h);
        _regPointCanvas.OffsetLeft = -w / 2f;
        _regPointCanvas.OffsetTop = -h / 2f;
        _regPointCanvas.OffsetRight = w / 2f;
        _regPointCanvas.OffsetBottom = h / 2f;
        _selectionCanvas.CustomMinimumSize = new Vector2(w, h);
        _selectionCanvas.OffsetLeft = -w / 2f;
        _selectionCanvas.OffsetTop = -h / 2f;
        _selectionCanvas.OffsetRight = w / 2f;
        _selectionCanvas.OffsetBottom = h / 2f;
    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _navBar.CustomMinimumSize = new Vector2(size.X, NavigationBarHeight);
        _iconBar.Position = new Vector2(0, NavigationBarHeight + TitleBarHeight);
        _iconBar.CustomMinimumSize = new Vector2(size.X, IconBarHeight);
        _bottomBar.Position = new Vector2(0, size.Y - BottomBarHeight);
        _bottomBar.CustomMinimumSize = new Vector2(size.X, BottomBarHeight);

        _scrollContainer.OffsetTop = TitleBarHeight + NavigationBarHeight + IconBarHeight;
        _scrollContainer.OffsetBottom = -BottomBarHeight;
        // Keep the scroll container offset by the toolbar width when the window
        // is resized so it doesn't overlap the toolbar.
        _scrollContainer.OffsetLeft = _paintToolbar.CustomMinimumSize.X;
        _scrollContainer.OffsetRight = 0;
        _centerContainer.PivotOffset = _centerContainer.CustomMinimumSize / 2f;
        UpdateRegPointCanvasSize();
        CenterImage();
        _regPointCanvas.QueueRedraw();
        _selectionCanvas.QueueRedraw();
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
        _regPointCanvas.QueueRedraw();
        _selectionCanvas.QueueRedraw();

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

        if (base._dragging) return;
        if (!Visible) return;
        Rect2 bounds = new Rect2(_scrollContainer.GlobalPosition, _scrollContainer.Size);
        Vector2 mousePos = GetGlobalMousePosition();
        if (!bounds.HasPoint(mousePos)) return;
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.Space)
                SpaceBarPress(keyEvent);
            return;
        }

        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed && _spaceHeld)
                {
                    _panning = true;
                    GetViewport().SetInputAsHandled();
                    return;
                }

                if (mb.Pressed && !@_spaceHeld)
                {
                    if (_paintToolbar.SelectedTool == PainterToolType.SelectRectangle)
                        SelectingPixels();
                    else if (_painter != null && _imageRect.Texture != null)
                        DrawingPixels();
                    return;
                }

                if (!mb.Pressed)
                {
                    if (_dragSelecting && _paintToolbar.SelectedTool == PainterToolType.SelectRectangle)
                        SelectingPixelsFinished(mb);
                    else
                        _panning = false;
                    GetViewport().SetInputAsHandled();
                    return;
                }
            }
            else if (!mb.Pressed && (mb.ButtonIndex == MouseButton.WheelUp || mb.ButtonIndex == MouseButton.WheelDown))
            {
                ZoomingWithMouseScroll(mb);
                return;
            }

        }
        else if (@event is InputEventMouseMotion motion)
        {
            if (_dragSelecting && _paintToolbar.SelectedTool == PainterToolType.SelectRectangle)
            {
                var local = _imageRect.GetLocalMousePosition() / _scale;
                var end = new Vector2I((int)local.X, (int)local.Y);
                _dragRect = RectFromPoints(_selectStart, end);
                _selectionCanvas.QueueRedraw();
                GetViewport().SetInputAsHandled();
                return;
            }
            if (_panning)
            {
                _scrollContainer.ScrollHorizontal -= (int)motion.Relative.X;
                _scrollContainer.ScrollVertical -= (int)motion.Relative.Y;

                GetViewport().SetInputAsHandled();
                return;
            }
        }
    }

    private void ZoomingWithMouseScroll(InputEventMouseButton mb)
    {
        // Apply zoom factor per scroll step (e.g. 25% per notch)
        const float zoomStep = 1.25f;
        float factor = mb.ButtonIndex == MouseButton.WheelUp ? zoomStep : 1f / zoomStep;

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

    private void SelectingPixelsFinished(InputEventMouseButton mb)
    {
        var local = _imageRect.GetLocalMousePosition() / _scale;
        var end = new Vector2I((int)local.X, (int)local.Y);
        Rect2I rect = RectFromPoints(_selectStart, end);
        ApplySelection(rect, mb.CtrlPressed, mb.ShiftPressed);
        _dragSelecting = false;
        _dragRect = null;
        _selectionCanvas.QueueRedraw();
    }

    private void DrawingPixels()
    {
        var local = _imageRect.GetLocalMousePosition() / _scale;
        var pixel = new Vector2I((int)local.X, (int)local.Y);
        _commandManager.Handle(new PainterDrawPixelCommand(pixel.X, pixel.Y));
    }

    private void SelectingPixels()
    {
        if (_painter != null)
        {
            var local = _imageRect.GetLocalMousePosition() / _scale;
            _selectStart = new Vector2I((int)local.X, (int)local.Y);
            _dragRect = null;
            _dragSelecting = true;
            _selectionCanvas.QueueRedraw();
        }
    }

    private void SpaceBarPress(InputEventKey keyEvent)
    {
        _spaceHeld = keyEvent.Pressed;
        if (!keyEvent.Pressed)
            _panning = false;
    }

    public void MemberSelected(ILingoMember member)
    {
        if (member is LingoMemberBitmap pic)
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

   
    private static Rect2I RectFromPoints(Vector2I a, Vector2I b)
    {
        int minX = Math.Min(a.X, b.X);
        int minY = Math.Min(a.Y, b.Y);
        int maxX = Math.Max(a.X, b.X);
        int maxY = Math.Max(a.Y, b.Y);
        return new Rect2I(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private void ApplySelection(Rect2I rect, bool ctrl, bool shift)
    {
        var pixels = new List<Vector2I>();
        for (int y = 0; y < rect.Size.Y; y++)
            for (int x = 0; x < rect.Size.X; x++)
                pixels.Add(rect.Position + new Vector2I(x, y));

        if (shift)
        {
            foreach (var p in pixels)
                _selectedPixels.Remove(p);
        }
        else if (ctrl)
        {
            foreach (var p in pixels)
                _selectedPixels.Add(p);
        }
        else
        {
            _selectedPixels.Clear();
            foreach (var p in pixels)
                _selectedPixels.Add(p);
        }
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

        var godotPicture = _member.Framework<LingoGodotMemberBitmap>();
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
        var oldReg = _member?.RegPoint ?? new LingoPoint(0, 0);

        var pixel = new Vector2I(x, y);
        if (_paintToolbar.SelectedTool == PainterToolType.Eraser)
            _painter.ErasePixel(pixel);
        else if (_paintToolbar.SelectedTool == PainterToolType.PaintBrush)
            _painter.PaintBrush(pixel, _paintToolbar.SelectedColor, _brushSize);
        else
            _painter.PaintPixel(pixel, _paintToolbar.SelectedColor);

        var delta = _painter.Offset - beforeOffset;
        if (delta != Vector2I.Zero && _member != null)
            _member.RegPoint = new LingoPoint(_member.RegPoint.X + delta.X, _member.RegPoint.Y + delta.Y);

        _painter.Commit();
        var afterImage = _painter.GetImage();
        var afterOffset = _painter.Offset;
        var newReg = _member?.RegPoint ?? oldReg;
        _imageRect.Texture = _painter.Texture;
        if (delta != Vector2I.Zero)
            RefreshImageSize();
        _regPointCanvas.QueueRedraw();

        _historyManager.Push(() =>
        {
            if (_painter == null) return;
            _painter.SetState(beforeImage, beforeOffset);
            _imageRect.Texture = _painter.Texture;
            if (_member != null)
                _member.RegPoint = oldReg;
            RefreshImageSize();
            _regPointCanvas.QueueRedraw();
        },
        () =>
        {
            if (_painter == null) return;
            _painter.SetState(afterImage, afterOffset);
            _imageRect.Texture = _painter.Texture;
            if (_member != null)
                _member.RegPoint = newReg;
            RefreshImageSize();
            _regPointCanvas.QueueRedraw();
        });

        return true;
    }


    
    
}

