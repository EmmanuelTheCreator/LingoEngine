using System;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Inputs;

public class AbstBlazorColorPickerComponent : AbstBlazorComponentModelBase, IAbstFrameworkColorPicker, IFrameworkFor<AbstColorPicker>
{
    private AColor _color = AColor.FromRGB(0, 0, 0);
    public AColor Color
    {
        get => _color;
        set { if (!_color.Equals(value)) { _color = value; RaiseChanged(); } }
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
