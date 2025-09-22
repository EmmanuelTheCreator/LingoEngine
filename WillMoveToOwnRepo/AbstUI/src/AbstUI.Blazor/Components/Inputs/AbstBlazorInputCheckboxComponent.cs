using System;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Inputs;

public class AbstBlazorInputCheckboxComponent : AbstBlazorComponentModelBase, IAbstFrameworkInputCheckbox, IFrameworkFor<AbstInputCheckbox>
{
    private bool _checked;
    public bool Checked
    {
        get => _checked;
        set { if (_checked != value) { _checked = value; RaiseChanged(); } }
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
