using Godot;
using LingoEngine.Pictures;
using LingoEngine.LGodot.Pictures;
using LingoEngine.Director.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.Pictures;

internal partial class DirGodotPictureMemberEditorWindow : BaseGodotWindow
{
    private const int IconBarHeight = 20;
    private const int BottomBarHeight = 20;

    private readonly TextureRect _imageRect = new TextureRect();
    private readonly HBoxContainer _iconBar = new HBoxContainer();
    private readonly HBoxContainer _bottomBar = new HBoxContainer();
    private readonly Button _flipHButton = new Button();
    private readonly Button _flipVButton = new Button();
    private readonly HSlider _zoomSlider = new HSlider();
    private readonly OptionButton _scaleDropdown = new OptionButton();

    private float _scale = 1f;

    public DirGodotPictureMemberEditorWindow() : base("Picture Editor")
    {
        Size = new Vector2(400, 300);
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

        // Image display
        _imageRect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
        _imageRect.AnchorLeft = 0;
        _imageRect.AnchorTop = 0;
        _imageRect.AnchorRight = 1;
        _imageRect.AnchorBottom = 1;
        _imageRect.OffsetLeft = 0;
        _imageRect.OffsetTop = TitleBarHeight + IconBarHeight;
        _imageRect.OffsetRight = 0;
        _imageRect.OffsetBottom = -BottomBarHeight;
        AddChild(_imageRect);

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
            _imageRect.Texture = godotPicture.Texture;
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
        _imageRect.Scale = new Vector2(_scale, _scale);

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

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _iconBar.CustomMinimumSize = new Vector2(size.X, IconBarHeight);
        _bottomBar.Position = new Vector2(0, size.Y - BottomBarHeight);
        _bottomBar.CustomMinimumSize = new Vector2(size.X, BottomBarHeight);
        _imageRect.OffsetTop = TitleBarHeight + IconBarHeight;
        _imageRect.OffsetBottom = -BottomBarHeight;
    }
}
