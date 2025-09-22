using System;
using System.Collections.Generic;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Inputs;

public class AbstBlazorInputComboboxComponent : AbstBlazorComponentModelBase, IAbstFrameworkInputCombobox, IFrameworkFor<AbstInputCombobox>, IHasTextBackgroundBorderColor
{
    private readonly List<KeyValuePair<string, string>> _items = new();
    public IReadOnlyList<KeyValuePair<string, string>> Items => _items;

    public void AddItem(string key, string value)
    {
        _items.Add(new KeyValuePair<string, string>(key, value));
        RaiseChanged();
    }

    public void ClearItems()
    {
        _items.Clear();
        SelectedIndex = -1;
        SelectedKey = null;
        SelectedValue = null;
        RaiseChanged();
    }

    private int _selectedIndex = -1;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set { if (_selectedIndex != value) { _selectedIndex = value; RaiseChanged(); } }
    }

    private string? _selectedKey;
    public string? SelectedKey
    {
        get => _selectedKey;
        set { if (_selectedKey != value) { _selectedKey = value; RaiseChanged(); } }
    }

    private string? _selectedValue;
    public string? SelectedValue
    {
        get => _selectedValue;
        set { if (_selectedValue != value) { _selectedValue = value; RaiseChanged(); } }
    }

    private string? _itemFont;
    public string? ItemFont
    {
        get => _itemFont;
        set { if (_itemFont != value) { _itemFont = value; RaiseChanged(); } }
    }

    private int _itemFontSize = 11;
    public int ItemFontSize
    {
        get => _itemFontSize;
        set { if (_itemFontSize != value) { _itemFontSize = value; RaiseChanged(); } }
    }

    private AColor _itemTextColor = AbstDefaultColors.InputTextColor;
    public AColor ItemTextColor
    {
        get => _itemTextColor;
        set { if (!_itemTextColor.Equals(value)) { _itemTextColor = value; RaiseChanged(); } }
    }

    private AColor _itemSelectedTextColor = AbstDefaultColors.InputSelectionText;
    public AColor ItemSelectedTextColor
    {
        get => _itemSelectedTextColor;
        set { if (!_itemSelectedTextColor.Equals(value)) { _itemSelectedTextColor = value; RaiseChanged(); } }
    }

    private AColor _itemSelectedBackgroundColor = AbstDefaultColors.InputAccentColor;
    public AColor ItemSelectedBackgroundColor
    {
        get => _itemSelectedBackgroundColor;
        set { if (!_itemSelectedBackgroundColor.Equals(value)) { _itemSelectedBackgroundColor = value; RaiseChanged(); } }
    }

    private AColor _itemSelectedBorderColor = AbstDefaultColors.InputBorderColor;
    public AColor ItemSelectedBorderColor
    {
        get => _itemSelectedBorderColor;
        set { if (!_itemSelectedBorderColor.Equals(value)) { _itemSelectedBorderColor = value; RaiseChanged(); } }
    }

    private AColor _itemHoverTextColor = AbstDefaultColors.InputTextColor;
    public AColor ItemHoverTextColor
    {
        get => _itemHoverTextColor;
        set { if (!_itemHoverTextColor.Equals(value)) { _itemHoverTextColor = value; RaiseChanged(); } }
    }

    private AColor _itemHoverBackgroundColor = AbstDefaultColors.ListHoverColor;
    public AColor ItemHoverBackgroundColor
    {
        get => _itemHoverBackgroundColor;
        set { if (!_itemHoverBackgroundColor.Equals(value)) { _itemHoverBackgroundColor = value; RaiseChanged(); } }
    }

    private AColor _itemHoverBorderColor = AbstDefaultColors.InputBorderColor;
    public AColor ItemHoverBorderColor
    {
        get => _itemHoverBorderColor;
        set { if (!_itemHoverBorderColor.Equals(value)) { _itemHoverBorderColor = value; RaiseChanged(); } }
    }

    private AColor _itemPressedTextColor = AbstDefaultColors.InputSelectionText;
    public AColor ItemPressedTextColor
    {
        get => _itemPressedTextColor;
        set { if (!_itemPressedTextColor.Equals(value)) { _itemPressedTextColor = value; RaiseChanged(); } }
    }

    private AColor _itemPressedBackgroundColor = AbstDefaultColors.InputAccentColor;
    public AColor ItemPressedBackgroundColor
    {
        get => _itemPressedBackgroundColor;
        set { if (!_itemPressedBackgroundColor.Equals(value)) { _itemPressedBackgroundColor = value; RaiseChanged(); } }
    }

    private AColor _itemPressedBorderColor = AbstDefaultColors.InputBorderColor;
    public AColor ItemPressedBorderColor
    {
        get => _itemPressedBorderColor;
        set { if (!_itemPressedBorderColor.Equals(value)) { _itemPressedBorderColor = value; RaiseChanged(); } }
    }

    private AColor _textColor = AColors.Black;
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
