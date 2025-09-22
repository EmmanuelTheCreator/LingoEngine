using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components.Inputs;

public partial class AbstBlazorInputText
{
    private AbstBlazorInputTextComponent _component = default!;

    [CascadingParameter]
    private AbstBlazorComponentContainer ComponentContainer { get; set; } = default!;

    [Parameter]
    public AbstBlazorInputTextComponent Component { get; set; } = default!;

    private string _inputValue = string.Empty;
    private bool _textDirty;
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
        if (_skipDirtyReset)
        {
            _skipDirtyReset = false;
        }
        else
        {
            _inputValue = _component.Text;
            _textDirty = false;
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

    private Task HandleInput(ChangeEventArgs e)
    {
        _inputValue = e.Value?.ToString() ?? string.Empty;
        _component.Text = _inputValue;
        _component.MarkDirty();
        _textDirty = true;
        _skipDirtyReset = true;
        return Task.CompletedTask;
    }

    private Task HandleChange(ChangeEventArgs e)
    {
        _inputValue = e.Value?.ToString() ?? string.Empty;
        if (_component.Text != _inputValue)
        {
            _component.Text = _inputValue;
        }

        _component.MarkDirty();
        _textDirty = true;
        _skipDirtyReset = true;
        CommitPendingChanges();
        return Task.CompletedTask;
    }

    private Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (string.Equals(e.Key, "Enter", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e.Key, "NumpadEnter", StringComparison.OrdinalIgnoreCase))
        {
            CommitPendingChanges();
        }

        return Task.CompletedTask;
    }

    private void CommitPendingChanges()
    {
        if (!_textDirty)
            return;

        _textDirty = false;
        _component.RaiseCommit();
    }

    public override void Dispose()
    {
        _component.Changed -= OnComponentChanged;
        ComponentContainer.Unregister(_component);
        base.Dispose();
    }
}
