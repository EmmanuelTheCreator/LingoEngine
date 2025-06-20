using Godot;
using LingoEngine.Pictures;
using LingoEngine.LGodot.Pictures;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.LGodot;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Director.Core.Events;
using LingoEngine.Members;

namespace LingoEngine.Director.LGodot.Pictures;

internal partial class DirGodotPictureMemberEditorWindow : BaseGodotWindow, IHasMemberSelectedEvent, IDirFrameworkPictureEditWindow
{
    private const int IconBarHeight = 20;
    private const int BottomBarHeight = 20;

    private readonly ScrollContainer _scrollContainer = new ScrollContainer();
    private readonly Control _centerContainer = new Control();
    private readonly ColorRect _background = new ColorRect();
    private readonly TextureRect _imageRect = new TextureRect();
    private readonly HBoxContainer _iconBar = new HBoxContainer();
    private readonly HBoxContainer _bottomBar = new HBoxContainer();
    private readonly Button _flipHButton = new Button();
    private readonly Button _flipVButton = new Button();
    private readonly Button _toggleRegPointButton = new Button();
    private readonly HSlider _zoomSlider = new HSlider();
    private readonly OptionButton _scaleDropdown = new OptionButton();
    private readonly RegPointCanvas _regPointCanvas;
    private readonly IDirectorEventMediator _mediator;
    private LingoMemberPicture? _member;
    private bool _showRegPoint = true;

    private float _scale = 1f;
    private bool _spaceHeld;
    private bool _panning;

    public DirGodotPictureMemberEditorWindow(IDirectorEventMediator mediator, IDirGodotWindowManager windowManager, DirectorPictureEditWindow directorPictureEditWindow) : base(DirectorMenuCodes.PictureEditWindow, "Picture Editor", windowManager)
    {
        _mediator = mediator;
        _mediator.Subscribe(this);
        Size = new Vector2(400, 300);
        directorPictureEditWindow.Init(this);
        CustomMinimumSize = Size;

        // Icon bar at the top
        AddChild(_iconBar);
        _iconBar.Position = new Vector2(0, TitleBarHeight);
        _iconBar.CustomMinimumSize = new Vector2(Size.X, IconBarHeight);

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

        // Image display container with scrollbars
        AddChild(_scrollContainer);
        _scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _scrollContainer.VerticalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
        _scrollContainer.AnchorLeft = 0;
        _scrollContainer.AnchorTop = 0;
        _scrollContainer.AnchorRight = 1;
        _scrollContainer.AnchorBottom = 1;
        _scrollContainer.OffsetLeft = 0;
        _scrollContainer.OffsetTop = TitleBarHeight + IconBarHeight;
        _scrollContainer.OffsetRight = 0;
        _scrollContainer.OffsetBottom = -BottomBarHeight;

        _background.Color = Colors.White;
        _background.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _background.SizeFlagsVertical = SizeFlags.ExpandFill;
        _scrollContainer.AddChild(_background);

        _scrollContainer.AddChild(_centerContainer);
        _centerContainer.AnchorLeft = 0;
        _centerContainer.AnchorTop = 0;
        _centerContainer.AnchorRight = 0;
        _centerContainer.AnchorBottom = 0;
        _centerContainer.OffsetLeft = 0;
        _centerContainer.OffsetTop = 0;
        _centerContainer.PivotOffset = _centerContainer.CustomMinimumSize / 2f;

        _imageRect.StretchMode = TextureRect.StretchModeEnum.Keep;
        _imageRect.AnchorLeft = 0;
        _imageRect.AnchorTop = 0;
        _imageRect.AnchorRight = 0;
        _imageRect.AnchorBottom = 0;
        _imageRect.OffsetLeft = 0;
        _imageRect.OffsetTop = 0;
        _centerContainer.AddChild(_imageRect);

        _regPointCanvas = new RegPointCanvas(this);
        _regPointCanvas.AnchorLeft = 0;
        _regPointCanvas.AnchorTop = 0;
        _regPointCanvas.AnchorRight = 0;
        _regPointCanvas.AnchorBottom = 0;
        _regPointCanvas.OffsetLeft = 0;
        _regPointCanvas.OffsetTop = 0;
        _regPointCanvas.Visible = true;
        _scrollContainer.AddChild(_regPointCanvas);

        // Bottom zoom bar
        AddChild(_bottomBar);
        _bottomBar.Position = new Vector2(0, Size.Y - BottomBarHeight);
        _bottomBar.CustomMinimumSize = new Vector2(Size.X, BottomBarHeight);

        _zoomSlider.MinValue = 0.2f;
        _zoomSlider.MaxValue = 2f;
        _zoomSlider.Step = 0.1f;
        _zoomSlider.Value = 1f;
        _zoomSlider.CustomMinimumSize = new Vector2(150, BottomBarHeight);
        _zoomSlider.ValueChanged += value => OnZoomChanged((float)value);
        _bottomBar.AddChild(_zoomSlider);

        _scaleDropdown.CustomMinimumSize = new Vector2(60, BottomBarHeight);
        for (int percent = 20; percent <= 200; percent += 10)
        {
            _scaleDropdown.AddItem($"{percent}%");
            if (percent == 100)
                _scaleDropdown.Select(_scaleDropdown.ItemCount - 1);
        }
        _scaleDropdown.ItemSelected += id => OnScaleSelected(id);
        _bottomBar.AddChild(_scaleDropdown);
    }

    public void SetPicture(LingoMemberPicture picture)
    {
        var godotPicture = picture.Framework<LingoGodotMemberPicture>();
        godotPicture.Preload();
        if (godotPicture.Texture != null)
        {
            _imageRect.Texture = godotPicture.Texture;
            Vector2 size = new(godotPicture.Width, godotPicture.Height);
            _imageRect.CustomMinimumSize = size;
        _centerContainer.CustomMinimumSize = size;
        _centerContainer.PivotOffset = size / 2f;
        FitImageToView();
        CenterImage();
        UpdateRegPointCanvasSize();
        }
        _member = picture;
        _regPointCanvas.QueueRedraw();
    }

    public void MemberSelected(ILingoMember member)
    {
        if (member is LingoMemberPicture pic)
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

    private void OnZoomChanged(float value)
    {
        _scale = value;
        _centerContainer.Scale = new Vector2(_scale, _scale);
        UpdateRegPointCanvasSize();
        _regPointCanvas.QueueRedraw();
        
        int percent = Mathf.RoundToInt(_scale * 100);
        for (int i = 0; i < _scaleDropdown.ItemCount; i++)
        {
            if (_scaleDropdown.GetItemText(i).TrimEnd('%') == percent.ToString())
            {
                _scaleDropdown.Select(i);
                break;
            }
        }
    }

    private void OnScaleSelected(long id)
    {
        var text = _scaleDropdown.GetItemText((int)id);
        if (text.EndsWith("%") && float.TryParse(text.TrimEnd('%'), out var percent))
        {
            var newScale = percent / 100f;
            _zoomSlider.Value = newScale;
            OnZoomChanged(newScale);
        }
    }

    private void FitImageToView()
    {
        var texture = _imageRect.Texture;
        if (texture == null) return;
        Vector2 areaSize = _scrollContainer.Size;
        if (areaSize == Vector2.Zero)
            areaSize = new Vector2(Size.X, Size.Y - (TitleBarHeight + IconBarHeight + BottomBarHeight));
        float factor = Math.Min(areaSize.X / texture.GetWidth(), areaSize.Y / texture.GetHeight());
        factor = (float)Mathf.Clamp(factor, _zoomSlider.MinValue, _zoomSlider.MaxValue);
        _zoomSlider.Value = factor;
        OnZoomChanged(factor);
    }

    private void CenterImage()
    {
        Vector2 view = _scrollContainer.Size;
        if (view == Vector2.Zero)
            view = new Vector2(Size.X, Size.Y - (TitleBarHeight + IconBarHeight + BottomBarHeight));
        Vector2 img = _centerContainer.CustomMinimumSize * _scale;
        int h = (int)Mathf.Max(0, (img.X - view.X) / 2f);
        int v = (int)Mathf.Max(0, (img.Y - view.Y) / 2f);
        _scrollContainer.ScrollHorizontal = h;
        _scrollContainer.ScrollVertical = v;
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
    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _iconBar.CustomMinimumSize = new Vector2(size.X, IconBarHeight);
        _bottomBar.Position = new Vector2(0, size.Y - BottomBarHeight);
        _bottomBar.CustomMinimumSize = new Vector2(size.X, BottomBarHeight);

        _scrollContainer.OffsetTop = TitleBarHeight + IconBarHeight;
        _scrollContainer.OffsetBottom = -BottomBarHeight;
        _scrollContainer.OffsetLeft = 0;
        _scrollContainer.OffsetRight = 0;
        _centerContainer.PivotOffset = _centerContainer.CustomMinimumSize / 2f;
        UpdateRegPointCanvasSize();
        CenterImage();
        _regPointCanvas.QueueRedraw();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (!Visible) return;

        if (@event is InputEventKey key && key.Keycode == Key.Space)
        {
            _spaceHeld = key.Pressed;
            if (!key.Pressed)
                _panning = false;
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
                }
                else if (!mb.Pressed)
                {
                    _panning = false;
                }
            }
            else if (!mb.Pressed && (mb.ButtonIndex == MouseButton.WheelUp || mb.ButtonIndex == MouseButton.WheelDown))
            {
                if (bounds.HasPoint(mousePos))
                {
                    float delta = mb.ButtonIndex == MouseButton.WheelUp ? 0.1f : -0.1f;
                    float newScale = (float)Mathf.Clamp(_scale + delta, _zoomSlider.MinValue, _zoomSlider.MaxValue);
                    _zoomSlider.Value = newScale;
                    OnZoomChanged(newScale);
                }
            }
        }
        else if (@event is InputEventMouseMotion motion)
        {
            if (_panning)
            {
                _scrollContainer.ScrollHorizontal -= (int)motion.Relative.X;
                _scrollContainer.ScrollVertical -= (int)motion.Relative.Y;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        _mediator.Unsubscribe(this);
        base.Dispose(disposing);
    }
}

internal partial class DirGodotPictureMemberEditorWindow
{
    private partial class RegPointCanvas : Control
    {
        private readonly DirGodotPictureMemberEditorWindow _owner;
        public RegPointCanvas(DirGodotPictureMemberEditorWindow owner)
        {
            _owner = owner;
            MouseFilter = MouseFilterEnum.Ignore;
        }

        public override void _Draw()
        {
            if (!_owner._showRegPoint) return;
            var member = _owner._member;
            if (member == null || _owner._imageRect.Texture == null) return;

            Vector2 areaSize = Size;

            // RegPoint origin is the texture's top-left corner
            Vector2 pos = new Vector2(member.RegPoint.X, member.RegPoint.Y);

            DrawLine(new Vector2(pos.X, 0), new Vector2(pos.X, areaSize.Y), Colors.Red);
            DrawLine(new Vector2(0, pos.Y), new Vector2(areaSize.X, pos.Y), Colors.Red);
        }
    }
}
