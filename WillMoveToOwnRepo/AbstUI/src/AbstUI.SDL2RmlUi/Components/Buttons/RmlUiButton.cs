using AbstUI.Components;
using AbstUI.Components.Buttons;
using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using RmlUiNet;

namespace AbstUI.SDL2RmlUi.Components;

/// <summary>
/// Button implementation backed by a RmlUi element.
/// </summary>
public class RmlUiButton : IAbstFrameworkButton, IDisposable
{
    private readonly Element _element;
    private readonly nint _renderer;
    private AMargin _margin;
    private string _name = string.Empty;
    private bool _visibility = true;
    private float _width;
    private float _height;
    private event Action? _pressed;
    private Element? _iconElement;
    private IAbstTexture2D? _iconTexture;
    private string _text = string.Empty;
    private string? _iconDataUrl;
    private int _iconWidth;
    private int _iconHeight;

    public RmlUiButton(ElementDocument document, nint renderer)
    {
        // Create a <button> element and hook up the click event
        _element = document.AppendChildTag("button");
        _element.AddEventListener("click", _ => _pressed?.Invoke());
        _renderer = renderer;
    }

    public object FrameworkNode => _element;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            _element.SetAttribute("id", value);
        }
    }

    public bool Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            _element.SetProperty("display", value ? "block" : "none");
        }
    }

    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            _element.SetProperty("width", $"{value}px");
        }
    }

    public float Height
    {
        get => _height;
        set
        {
            _height = value;
            _element.SetProperty("height", $"{value}px");
        }
    }

    public AMargin Margin
    {
        get => _margin;
        set
        {
            _margin = value;
            _element.SetProperty("margin", $"{value.Top}px {value.Right}px {value.Bottom}px {value.Left}px");
        }
    }

    public string Text
    {
        get
        {
            _text = _element.GetInnerRml();
            return _text;
        }
        set
        {
            _text = value ?? string.Empty;
            _element.SetInnerRml(_text);
            UpdateIconElement();
            UpdateContentLayout();
        }
    }

    public bool Enabled
    {
        get => !_element.HasAttribute("disabled");
        set
        {
            if (value) _element.RemoveAttribute("disabled");
            else _element.SetAttribute("disabled", "disabled");
        }
    }

    public IAbstTexture2D? IconTexture
    {
        get => _iconTexture;
        set
        {
            _iconTexture = value;
            UpdateIconSource(value);
            UpdateIconElement();
            UpdateContentLayout();
        }
    }

    public event Action? Pressed
    {
        add => _pressed += value;
        remove => _pressed -= value;
    }

    public void Dispose() { }

    private void UpdateIconSource(IAbstTexture2D? texture)
    {
        if (texture is SdlTexture2D tex)
        {
            _iconDataUrl = $"data:image/png;base64,{tex.ToPngBase64(_renderer)}";
            _iconWidth = tex.Width;
            _iconHeight = tex.Height;
        }
        else
        {
            _iconDataUrl = null;
            _iconWidth = 0;
            _iconHeight = 0;
        }
    }

    private void UpdateIconElement()
    {
        if (_iconElement != null)
        {
            if (_iconElement.GetParentNode() == _element)
                _element.RemoveChild(_iconElement);
            _iconElement = null;
        }

        if (_iconDataUrl != null)
        {
            _iconElement = _element.AppendChildTag("img");
            _iconElement.SetAttribute("src", _iconDataUrl);
            if (_iconWidth > 0)
                _iconElement.SetAttribute("width", _iconWidth.ToString());
            if (_iconHeight > 0)
                _iconElement.SetAttribute("height", _iconHeight.ToString());
        }
    }

    private void UpdateContentLayout()
    {
        bool hasIcon = _iconElement != null && _iconElement.GetParentNode() == _element;
        bool hasText = !string.IsNullOrWhiteSpace(_text);
        _element.SetProperty("text-align", hasIcon && !hasText ? "center" : "left");
    }
}
