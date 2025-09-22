using System;
using System.Collections.Generic;
using System.Numerics;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using AbstUI.Primitives;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Inputs;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkInputSlider{TValue}"/>.
/// </summary>
internal class AbstUnityInputSlider<TValue> : AbstUnityComponent, IAbstFrameworkInputSlider<TValue>, IFrameworkFor<AbstInputSlider<TValue>>
    where TValue : struct, INumber<TValue>
{
    private readonly Slider _slider;
    private TValue _value = TValue.Zero;
    private TValue _min = TValue.Zero;
    private TValue _max = TValue.Zero;
    private TValue _step = TValue.Zero;
    private bool _suppress;

    public AbstUnityInputSlider(AOrientation orientation) : base(CreateGameObject(out var slider, orientation))
    {
        _slider = slider;
        _slider.onValueChanged.AddListener(OnValueChanged);
    }

    private static GameObject CreateGameObject(out Slider slider, AOrientation orientation)
    {
        var go = new GameObject("Slider");
        go.AddComponent<Image>();
        slider = go.AddComponent<Slider>();
        slider.direction = orientation == AOrientation.Horizontal
            ? Slider.Direction.LeftToRight
            : Slider.Direction.BottomToTop;
        return go;
    }

    private void OnValueChanged(float value)
    {
        if (_suppress)
            return;
        var typed = TValue.CreateChecked(value);
        typed = ApplyStep(typed);
        if (!EqualityComparer<TValue>.Default.Equals(_value, typed))
        {
            _value = typed;
            ValueChanged?.Invoke();
            OnCommit?.Invoke();
        }
    }

    private TValue ApplyStep(TValue value)
    {
        if (EqualityComparer<TValue>.Default.Equals(_step, TValue.Zero))
            return value;
        var min = float.CreateChecked(_min);
        var step = float.CreateChecked(_step);
        var val = float.CreateChecked(value);
        var result = min + Mathf.Round((val - min) / step) * step;
        result = Mathf.Clamp(result, float.CreateChecked(_min), float.CreateChecked(_max));
        return TValue.CreateChecked(result);
    }

    public bool Enabled
    {
        get => _slider.interactable;
        set => _slider.interactable = value;
    }

    public TValue Value
    {
        get => _value;
        set
        {
            var v = ApplyStep(value);
            if (EqualityComparer<TValue>.Default.Equals(_value, v))
                return;
            _value = v;
            _suppress = true;
            _slider.value = float.CreateChecked(v);
            _suppress = false;
            ValueChanged?.Invoke();
        }
    }

    public TValue MinValue
    {
        get => _min;
        set
        {
            _min = value;
            _slider.minValue = float.CreateChecked(value);
        }
    }

    public TValue MaxValue
    {
        get => _max;
        set
        {
            _max = value;
            _slider.maxValue = float.CreateChecked(value);
        }
    }

    public TValue Step
    {
        get => _step;
        set => _step = value;
    }

    public event Action? ValueChanged;
    public event Action? OnCommit;
}

