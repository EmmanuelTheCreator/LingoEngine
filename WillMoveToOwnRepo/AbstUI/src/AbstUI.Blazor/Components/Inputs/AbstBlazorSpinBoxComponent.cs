using System;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Inputs;

public class AbstBlazorSpinBoxComponent : AbstBlazorComponentModelBase, IAbstFrameworkSpinBox, IFrameworkFor<AbstInputSpinBox>, IHasTextBackgroundBorderColor
{
    private float _value;
    public float Value
    {
        get => _value;
        set { if (Math.Abs(_value - value) > float.Epsilon) { _value = value; RaiseChanged(); } }
    }

    private float _min;
    public float Min
    {
        get => _min;
        set { if (Math.Abs(_min - value) > float.Epsilon) { _min = value; RaiseChanged(); } }
    }

    private float _max;
    public float Max
    {
        get => _max;
        set { if (Math.Abs(_max - value) > float.Epsilon) { _max = value; RaiseChanged(); } }
    }

    private AColor _textColor = AbstDefaultColors.InputTextColor;
    public AColor TextColor
    {
        get => _textColor;
        set { if (!_textColor.Equals(value)) { _textColor = value; RaiseChanged(); } }
    }

    private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
    public AColor BackgroundColor
    {
        get => _backgroundColor;
        set { if (!_backgroundColor.Equals(value)) { _backgroundColor = value; RaiseChanged(); } }
    }

    private AColor _borderColor = AbstDefaultColors.InputBorderColor;
    public AColor BorderColor
    {
        get => _borderColor;
        set { if (!_borderColor.Equals(value)) { _borderColor = value; RaiseChanged(); } }
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
