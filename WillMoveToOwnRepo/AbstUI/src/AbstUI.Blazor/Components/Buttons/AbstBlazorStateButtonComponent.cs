using System;
using AbstUI.Components.Buttons;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Buttons;

public class AbstBlazorStateButtonComponent : AbstBlazorComponentModelBase, IAbstFrameworkStateButton, IFrameworkFor<AbstStateButton>
{
    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set { if (_text != value) { _text = value; RaiseChanged(); } }
    }

    private AColor _borderColor = AbstDefaultColors.Button_Border_Normal;
    public AColor BorderColor
    {
        get => _borderColor;
        set { if (!_borderColor.Equals(value)) { _borderColor = value; RaiseChanged(); } }
    }

    private AColor _borderHoverColor = AbstDefaultColors.Button_Border_Hover;
    public AColor BorderHoverColor
    {
        get => _borderHoverColor;
        set { if (!_borderHoverColor.Equals(value)) { _borderHoverColor = value; RaiseChanged(); } }
    }

    private AColor _borderPressedColor = AbstDefaultColors.Button_Border_Pressed;
    public AColor BorderPressedColor
    {
        get => _borderPressedColor;
        set { if (!_borderPressedColor.Equals(value)) { _borderPressedColor = value; RaiseChanged(); } }
    }

    private AColor _backgroundColor = AbstDefaultColors.Button_Bg_Normal;
    public AColor BackgroundColor
    {
        get => _backgroundColor;
        set { if (!_backgroundColor.Equals(value)) { _backgroundColor = value; RaiseChanged(); } }
    }

    private AColor _backgroundHoverColor = AbstDefaultColors.Button_Bg_Hover;
    public AColor BackgroundHoverColor
    {
        get => _backgroundHoverColor;
        set { if (!_backgroundHoverColor.Equals(value)) { _backgroundHoverColor = value; RaiseChanged(); } }
    }

    private AColor _backgroundPressedColor = AbstDefaultColors.Button_Bg_Pressed;
    public AColor BackgroundPressedColor
    {
        get => _backgroundPressedColor;
        set { if (!_backgroundPressedColor.Equals(value)) { _backgroundPressedColor = value; RaiseChanged(); } }
    }

    private AColor _textColor = AColor.FromRGB(0, 0, 0);
    public AColor TextColor
    {
        get => _textColor;
        set { if (!_textColor.Equals(value)) { _textColor = value; RaiseChanged(); } }
    }

    private IAbstTexture2D? _textureOn;
    public IAbstTexture2D? TextureOn
    {
        get => _textureOn;
        set { if (_textureOn != value) { _textureOn = value; RaiseChanged(); } }
    }

    private IAbstTexture2D? _textureOff;
    public IAbstTexture2D? TextureOff
    {
        get => _textureOff;
        set { if (_textureOff != value) { _textureOff = value; RaiseChanged(); } }
    }

    private bool _isOn;
    public bool IsOn
    {
        get => _isOn;
        set { if (_isOn != value) { _isOn = value; RaiseChanged(); } }
    }

    private bool _enabled = true;
    public bool Enabled
    {
        get => _enabled;
        set { if (_enabled != value) { _enabled = value; RaiseChanged(); } }
    }

    public event Action? ValueChanged;
    public event Action? OnCommit;
    public void RaiseValueChanged()
    {
        ValueChanged?.Invoke();
        OnCommit?.Invoke();
    }
}
