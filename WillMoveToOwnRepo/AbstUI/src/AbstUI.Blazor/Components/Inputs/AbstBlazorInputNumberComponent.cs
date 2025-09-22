using System;
using System.Numerics;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Inputs;

public class AbstBlazorInputNumberComponent<TValue> : AbstBlazorComponentModelBase, IAbstFrameworkInputNumber<TValue>, IFrameworkFor<AbstInputNumber<TValue>>, IHasTextBackgroundBorderColor
    where TValue : INumber<TValue>
{
    private TValue _value = TValue.Zero;
    private bool _valueDirty;
    public TValue Value
    {
        get => _value;
        set
        {
            if (!_value.Equals(value))
            {
                _value = value;
                _valueDirty = false;
                RaiseChanged();
            }
        }
    }

    private TValue _min = TValue.Zero;
    public TValue Min
    {
        get => _min;
        set { if (!_min.Equals(value)) { _min = value; RaiseChanged(); } }
    }

    private TValue _max = TValue.Zero;
    public TValue Max
    {
        get => _max;
        set { if (!_max.Equals(value)) { _max = value; RaiseChanged(); } }
    }

    private ANumberType _numberType = ANumberType.Float;
    public ANumberType NumberType
    {
        get => _numberType;
        set { if (_numberType != value) { _numberType = value; RaiseChanged(); } }
    }

    private int _fontSize = 14;
    public int FontSize
    {
        get => _fontSize;
        set { if (_fontSize != value) { _fontSize = value; RaiseChanged(); } }
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

    public void RaiseCommit()
    {
        if (!_valueDirty)
            return;

        _valueDirty = false;
        OnCommit?.Invoke();
    }

    internal void MarkDirty()
    {
        _valueDirty = true;
        ValueChanged?.Invoke();
    }
}
