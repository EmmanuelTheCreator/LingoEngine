using Godot;
using LingoEngine.Pictures;
using LingoEngine.LGodot.Pictures;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.LGodot;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Director.Core.Events;
using LingoEngine.Members;
using LingoEngine.Core;
using LingoEngine.Movies;
using System.Linq;

namespace LingoEngine.Director.LGodot.Pictures;

internal partial class DirGodotPictureMemberEditorWindow : BaseGodotWindow, IHasMemberSelectedEvent, IDirFrameworkPictureEditWindow
{
    private const int NavigationBarHeight = 20;
    private const int IconBarHeight = 20;
    private const int BottomBarHeight = 20;
    private static readonly Vector2 WorkAreaSize = new Vector2(2000, 2000);

    private readonly ScrollContainer _scrollContainer = new ScrollContainer();
    private readonly Control _centerContainer = new Control();
    private readonly ColorRect _background = new ColorRect();
    private readonly TextureRect _imageRect = new TextureRect();
    private readonly MemberNavigationBar<LingoMemberPicture> _navBar;
    private readonly HBoxContainer _iconBar = new HBoxContainer();
    private readonly HBoxContainer _bottomBar = new HBoxContainer();
    private readonly Button _flipHButton = new Button();
    private readonly Button _flipVButton = new Button();
    private readonly Button _toggleRegPointButton = new Button();
    private readonly HSlider _zoomSlider = new HSlider();
    private readonly OptionButton _scaleDropdown = new OptionButton();
    private readonly RegPointCanvas _regPointCanvas;
    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoPlayer _player;
    private LingoMemberPicture? _member;
    private bool _showRegPoint = true;

    private float _scale = 1f;
    private bool _spaceHeld;
    private bool _panning;

    public DirGodotPictureMemberEditorWindow(IDirectorEventMediator mediator, ILingoPlayer player, IDirGodotWindowManager windowManager, DirectorPictureEditWindow directorPictureEditWindow) : base(DirectorMenuCodes.PictureEditWindow, "Picture Editor", windowManager)
    {
        _mediator = mediator;
        _player = player;
        _mediator.Subscribe(this);
        Size = new Vector2(400, 300);
        directorPictureEditWindow.Init(this);
        CustomMinimumSize = Size;

        _navBar = new MemberNavigationBar<LingoMemberPicture>(_mediator, _player, NavigationBarHeight);
        AddChild(_navBar);
        _navBar.Position = new Vector2(0, TitleBarHeight);
        _navBar.CustomMinimumSize = new Vector2(Size.X, NavigationBarHeight);

        // Icon bar below navigation
        AddChild(_iconBar);
        _iconBar.Position = new Vector2(0, TitleBarHeight + NavigationBarHeight);
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
        _scrollContainer.OffsetTop = TitleBarHeight + NavigationBarHeight + IconBarHeight;
        _scrollContainer.OffsetRight = 0;
        _scrollContainer.OffsetBottom = -BottomBarHeight;

        _scrollContainer.AddChild(_centerContainer);
        _centerContainer.CustomMinimumSize = WorkAreaSize;
        _centerContainer.AnchorLeft = 0.5f;
        _centerContainer.AnchorTop = 0.5f;
        _centerContainer.AnchorRight = 0.5f;
        _centerContainer.AnchorBottom = 0.5f;
        _centerContainer.OffsetLeft = -WorkAreaSize.X / 2f;
        _centerContainer.OffsetTop = -WorkAreaSize.Y / 2f;
        _centerContainer.OffsetRight = WorkAreaSize.X / 2f;
        _centerContainer.OffsetBottom = WorkAreaSize.Y / 2f;
        _centerContainer.PivotOffset = WorkAreaSize / 2f;

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

        _imageRect.StretchMode = TextureRect.StretchModeEnum.Keep;
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

    public void SetPicture(LingoMemberPicture picture)
    {
        bool firstLoad = _member == null;
        if (firstLoad)
            ResetView();
        var godotPicture = picture.Framework<LingoGodotMemberPicture>();
        godotPicture.Preload();
        if (godotPicture.Texture != null)
        {
            _imageRect.Texture = godotPicture.Texture;
            Vector2 size = new(godotPicture.Width, godotPicture.Height);
            _imageRect.CustomMinimumSize = size;
            _imageRect.OffsetLeft = -size.X / 2f;
            _imageRect.OffsetTop = -size.Y / 2f;
            _imageRect.OffsetRight = size.X / 2f;
            _imageRect.OffsetBottom = size.Y / 2f;
            if (firstLoad)
            {
                FitImageToView();
            }
            else
            {
                _zoomSlider.Value = _scale;
                OnZoomChanged(_scale);
            }
            UpdateRegPointCanvasSize();
            CallDeferred(nameof(CenterImage));
        }
        _member = picture;
        _navBar.SetMember(picture);
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
        Vector2 view = _scrollContainer.Size;
        if (view == Vector2.Zero)
            view = new Vector2(Size.X, Size.Y - (TitleBarHeight + IconBarHeight + BottomBarHeight));

        // keep the same canvas point at the center of the view after scaling
        float oldScale = _scale;
        float centerX = (_scrollContainer.ScrollHorizontal + view.X / 2f) / oldScale;
        float centerY = (_scrollContainer.ScrollVertical + view.Y / 2f) / oldScale;

        _scale = value;
        _centerContainer.Scale = new Vector2(_scale, _scale);
        UpdateRegPointCanvasSize();
        _regPointCanvas.QueueRedraw();

        Vector2 canvasSize = _centerContainer.CustomMinimumSize * _scale;
        float newH = centerX * _scale - view.X / 2f;
        float newV = centerY * _scale - view.Y / 2f;

        int maxH = (int)Mathf.Max(0, canvasSize.X - view.X);
        int maxV = (int)Mathf.Max(0, canvasSize.Y - view.Y);

        _scrollContainer.ScrollHorizontal = (int)Mathf.Clamp(newH, 0, maxH);
        _scrollContainer.ScrollVertical = (int)Mathf.Clamp(newV, 0, maxV);

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
        _scrollContainer.OffsetLeft = 0;
        _scrollContainer.OffsetRight = 0;
        _centerContainer.PivotOffset = _centerContainer.CustomMinimumSize / 2f;
        UpdateRegPointCanvasSize();
        CenterImage();
        _regPointCanvas.QueueRedraw();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
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
                    return;
                }
                else if (!mb.Pressed)
                {
                    _panning = false;
                    return;
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
                    return;
                }
            }
        }
        else if (@event is InputEventMouseMotion motion)
        {
            if (_panning)
            {
                _scrollContainer.ScrollHorizontal -= (int)motion.Relative.X;
                _scrollContainer.ScrollVertical -= (int)motion.Relative.Y;
                return;
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
            float factor = _owner._scale;
            Vector2 canvasHalf = _owner._centerContainer.CustomMinimumSize / 2f;
            Vector2 imageHalf = _owner._imageRect.CustomMinimumSize / 2f;
            Vector2 offset = canvasHalf - imageHalf;
            // RegPoint origin is the texture's top-left corner
            Vector2 pos = (offset + new Vector2(member.RegPoint.X, member.RegPoint.Y)) * factor + canvasHalf * (1 - factor);

            DrawLine(new Vector2(pos.X, 0), new Vector2(pos.X, areaSize.Y), Colors.Red);
            DrawLine(new Vector2(0, pos.Y), new Vector2(areaSize.X, pos.Y), Colors.Red);
        }
    }
}
