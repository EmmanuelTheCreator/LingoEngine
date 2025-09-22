using System;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Numerics;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components.Inputs;

public partial class AbstBlazorInputNumber<TValue>
    where TValue : INumber<TValue>
{
    private AbstBlazorInputNumberComponent<TValue> _component = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorInputNumberComponent<TValue> Component { get; set; } = default!;

    private string _inputValue = string.Empty;
    private bool _valueDirty;
    private bool _skipDirtyReset;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _component = Component;
        SyncFromComponent();
        _component.Changed += OnComponentChanged;
    }

    private void OnComponentChanged()
    {
        SyncFromComponent();
        RequestRender();
    }

    private void SyncFromComponent()
    {
        Visibility = _component.Visibility;
        Width = _component.Width;
        Height = _component.Height;
        Margin = _component.Margin;
        Enabled = _component.Enabled;
        if (_skipDirtyReset)
        {
            _skipDirtyReset = false;
        }
        else
        {
            _inputValue = FormatValue(_component.Value);
            _valueDirty = false;
        }
    }

    private void HandleInput(ChangeEventArgs e)
    {
        _inputValue = e.Value?.ToString() ?? string.Empty;
        _valueDirty = true;
        _skipDirtyReset = true;
        _component.MarkDirty();
    }

    private void HandleChange(ChangeEventArgs e)
    {
        _inputValue = e.Value?.ToString() ?? string.Empty;
        _valueDirty = true;
        _skipDirtyReset = true;
        _component.MarkDirty();
        CommitPendingValue();
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (string.Equals(e.Key, "Enter", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e.Key, "NumpadEnter", StringComparison.OrdinalIgnoreCase))
        {
            CommitPendingValue();
        }
    }

    protected override string BuildStyle()
    {
        var style = base.BuildStyle();
        style += $"color:{_component.TextColor.ToHex()};";
        style += $"background-color:{_component.BackgroundColor.ToHex()};";
        style += $"border-color:{_component.BorderColor.ToHex()};";
        return style;
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }

    private void CommitPendingValue()
    {
        if (!_valueDirty)
            return;

        if (!TValue.TryParse(_inputValue, CultureInfo.InvariantCulture, out var parsed))
        {
            _inputValue = FormatValue(_component.Value);
            _valueDirty = false;
            RequestRender();
            _component.RaiseCommit();
            return;
        }

        var clamped = TValue.Clamp(parsed, _component.Min, _component.Max);
        if (!_component.Value.Equals(clamped))
        {
            _component.Value = clamped;
            _component.MarkDirty();
        }

        _inputValue = FormatValue(_component.Value);
        _valueDirty = false;
        RequestRender();
        _component.RaiseCommit();
    }

    private static string FormatValue(TValue value) => value.ToString(null, CultureInfo.InvariantCulture);
}
