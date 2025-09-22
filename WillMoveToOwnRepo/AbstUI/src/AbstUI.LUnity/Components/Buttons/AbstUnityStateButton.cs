using System;
using AbstUI.Components.Buttons;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using AbstUI.Primitives;
using AbstUI.LUnity.Bitmaps;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Buttons;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkStateButton"/>.
/// </summary>
internal class AbstUnityStateButton : AbstUnityComponent, IAbstFrameworkStateButton, IFrameworkFor<AbstStateButton>
{
    private readonly Toggle _toggle;
    private readonly Image _image;
    private readonly Text _textComponent;
    private IAbstTexture2D? _textureOn;
    private IAbstTexture2D? _textureOff;
    private event Action? _valueChanged;
    private event Action? _onCommit;

    public AbstUnityStateButton() : base(CreateGameObject(out var toggle, out var image, out var text))
    {
        _toggle = toggle;
        _image = image;
        _textComponent = text;
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private static GameObject CreateGameObject(out Toggle toggle, out Image image, out Text text)
    {
        var go = new GameObject("StateButton");
        image = go.AddComponent<Image>();
        toggle = go.AddComponent<Toggle>();
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        text = textGo.AddComponent<Text>();
        return go;
    }

    public string Text
    {
        get => _textComponent.text;
        set => _textComponent.text = value;
    }

    public bool Enabled
    {
        get => _toggle.interactable;
        set => _toggle.interactable = value;
    }

    public AColor BorderColor { get; set; }
    public AColor BorderHoverColor { get; set; }
    public AColor BorderPressedColor { get; set; }
    public AColor BackgroundColor { get; set; }
    public AColor BackgroundHoverColor { get; set; }
    public AColor BackgroundPressedColor { get; set; }
    public AColor TextColor { get; set; }

    public IAbstTexture2D? TextureOn
    {
        get => _textureOn;
        set { _textureOn = value; UpdateImage(); }
    }

    public IAbstTexture2D? TextureOff
    {
        get => _textureOff;
        set { _textureOff = value; UpdateImage(); }
    }

    public bool IsOn
    {
        get => _toggle.isOn;
        set
        {
            if (_toggle.isOn != value)
            {
                _toggle.isOn = value;
                _valueChanged?.Invoke();
                UpdateImage();
            }
        }
    }

    public event Action? ValueChanged
    {
        add => _valueChanged += value;
        remove => _valueChanged -= value;
    }

    public event Action? OnCommit
    {
        add => _onCommit += value;
        remove => _onCommit -= value;
    }

    private void UpdateImage()
    {
        var tex = IsOn ? _textureOn : _textureOff;
        _image.sprite = tex is UnityTexture2D ut ? ut.ToSprite() : null;
    }

    private void OnToggleValueChanged(bool _)
    {
        _valueChanged?.Invoke();
        _onCommit?.Invoke();
        UpdateImage();
    }
}
