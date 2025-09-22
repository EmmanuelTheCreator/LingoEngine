using System;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using AbstUI.LUnity.Primitives;
using AbstUI.Primitives;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Inputs;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkColorPicker"/>.
/// </summary>
internal class AbstUnityColorPicker : AbstUnityComponent, IAbstFrameworkColorPicker, IFrameworkFor<AbstColorPicker>
{
    private readonly Image _image;
    private AColor _color = AColor.FromRGB(0, 0, 0);

    public AbstUnityColorPicker() : base(CreateGameObject(out var image))
    {
        _image = image;
    }

    private static GameObject CreateGameObject(out Image image)
    {
        var go = new GameObject("ColorPicker");
        image = go.AddComponent<Image>();
        return go;
    }

    public bool Enabled
    {
        get => _image.raycastTarget;
        set => _image.raycastTarget = value;
    }

    public AColor Color
    {
        get => _color;
        set
        {
            if (_color.Equals(value))
                return;
            _color = value;
            _image.color = value.ToUnityColor();
            ValueChanged?.Invoke();
            OnCommit?.Invoke();
        }
    }

    public event Action? ValueChanged;
    public event Action? OnCommit;
}
