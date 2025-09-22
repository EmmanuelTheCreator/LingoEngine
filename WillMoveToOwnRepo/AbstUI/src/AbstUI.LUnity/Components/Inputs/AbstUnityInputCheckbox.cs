using System;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Inputs;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkInputCheckbox"/>.
/// </summary>
internal class AbstUnityInputCheckbox : AbstUnityComponent, IAbstFrameworkInputCheckbox, IFrameworkFor<AbstInputCheckbox>
{
    private readonly Toggle _toggle;
    private bool _checked;

    public AbstUnityInputCheckbox() : base(CreateGameObject(out var toggle))
    {
        _toggle = toggle;
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private static GameObject CreateGameObject(out Toggle toggle)
    {
        var go = new GameObject("Toggle");
        go.AddComponent<Image>();
        toggle = go.AddComponent<Toggle>();
        return go;
    }

    private void OnToggleChanged(bool value)
    {
        if (_checked == value) return;
        _checked = value;
        ValueChanged?.Invoke();
    }

    public bool Enabled
    {
        get => _toggle.interactable;
        set => _toggle.interactable = value;
    }

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value;
            _toggle.isOn = value;
            ValueChanged?.Invoke();
            OnCommit?.Invoke();
        }
    }

    public event Action? ValueChanged;
    public event Action? OnCommit;
}
