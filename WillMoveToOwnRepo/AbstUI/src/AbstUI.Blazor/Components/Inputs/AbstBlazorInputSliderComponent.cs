using System;
using System.Numerics;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Inputs;

public class AbstBlazorInputSliderComponent<TValue> : AbstBlazorComponentModelBase, IAbstFrameworkInputSlider<TValue>, IFrameworkFor<AbstInputSlider<TValue>>
    where TValue : INumber<TValue>
{
    private TValue _value = TValue.Zero;
    public TValue Value
    {
        get => _value;
        set { if (!_value.Equals(value)) { _value = value; RaiseChanged(); } }
    }

    private TValue _minValue = TValue.Zero;
    public TValue MinValue
    {
        get => _minValue;
        set { if (!_minValue.Equals(value)) { _minValue = value; RaiseChanged(); } }
    }

    private TValue _maxValue = TValue.One;
    public TValue MaxValue
    {
        get => _maxValue;
        set { if (!_maxValue.Equals(value)) { _maxValue = value; RaiseChanged(); } }
    }

    private TValue _step = TValue.One;
    public TValue Step
    {
        get => _step;
        set { if (!_step.Equals(value)) { _step = value; RaiseChanged(); } }
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
