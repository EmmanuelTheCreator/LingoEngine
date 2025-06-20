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

    private readonly CenterContainer _centerContainer = new CenterContainer();
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

        // Image display container
        AddChild(_centerContainer);
        _centerContainer.AnchorLeft = 0;
        _centerContainer.AnchorTop = 0;
        _centerContainer.AnchorRight = 1;
        _centerContainer.AnchorBottom = 1;
        _centerContainer.OffsetLeft = 0;
        _centerContainer.OffsetTop = TitleBarHeight + IconBarHeight;
        _centerContainer.OffsetRight = 0;
        _centerContainer.OffsetBottom = -BottomBarHeight;
        _centerContainer.PivotOffset = new Vector2(Size.X / 2f,
            (Size.Y - (TitleBarHeight + IconBarHeight + BottomBarHeight)) / 2f);

        _imageRect.StretchMode = TextureRect.StretchModeEnum.Keep;
        _centerContainer.AddChild(_imageRect);

        _regPointCanvas = new RegPointCanvas(this);
        _regPointCanvas.AnchorLeft = 0;
        _regPointCanvas.AnchorTop = 0;
        _regPointCanvas.AnchorRight = 1;
        _regPointCanvas.AnchorBottom = 1;
        _regPointCanvas.OffsetLeft = 0;
        _regPointCanvas.OffsetTop = 0;
        _regPointCanvas.OffsetRight = 0;
        _regPointCanvas.OffsetBottom = 0;
        _regPointCanvas.Visible = true;
        _centerContainer.AddChild(_regPointCanvas);

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
            _imageRect.CustomMinimumSize = new Vector2(godotPicture.Width, godotPicture.Height);
            FitImageToView();
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
        Vector2 areaSize = _centerContainer.Size;
        if (areaSize == Vector2.Zero)
            areaSize = new Vector2(Size.X, Size.Y - (TitleBarHeight + IconBarHeight + BottomBarHeight));
        float factor = Math.Min(areaSize.X / texture.GetWidth(), areaSize.Y / texture.GetHeight());
        factor = Mathf.Clamp(factor, _zoomSlider.MinValue, _zoomSlider.MaxValue);
        _zoomSlider.Value = factor;
        OnZoomChanged(factor);
    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _iconBar.CustomMinimumSize = new Vector2(size.X, IconBarHeight);
        _bottomBar.Position = new Vector2(0, size.Y - BottomBarHeight);
        _bottomBar.CustomMinimumSize = new Vector2(size.X, BottomBarHeight);

        _centerContainer.OffsetTop = TitleBarHeight + IconBarHeight;
        _centerContainer.OffsetBottom = -BottomBarHeight;
        _centerContainer.OffsetLeft = 0;
        _centerContainer.OffsetRight = 0;
        _centerContainer.PivotOffset = new Vector2(size.X / 2f,
            (size.Y - (TitleBarHeight + IconBarHeight + BottomBarHeight)) / 2f);
        _regPointCanvas.QueueRedraw();
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
            var texture = _owner._imageRect.Texture;
            if (member == null || texture == null) return;

            Vector2 texSize = new(texture.GetWidth(), texture.GetHeight());
            Vector2 areaSize = _owner._imageRect.Size;

            float factor = Math.Min(areaSize.X / texSize.X, areaSize.Y / texSize.Y);
            Vector2 offset = new((areaSize.X - texSize.X * factor) / 2f,
                                 (areaSize.Y - texSize.Y * factor) / 2f);

            Vector2 pos = new Vector2(member.RegPoint.X, member.RegPoint.Y) * factor + offset;

            DrawLine(new Vector2(pos.X, 0), new Vector2(pos.X, areaSize.Y), Colors.Red);
            DrawLine(new Vector2(0, pos.Y), new Vector2(areaSize.X, pos.Y), Colors.Red);
        }
    }
}
