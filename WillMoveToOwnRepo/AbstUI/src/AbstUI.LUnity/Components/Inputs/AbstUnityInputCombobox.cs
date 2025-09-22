using System;
using System.Collections.Generic;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using AbstUI.LUnity.Primitives;
using AbstUI.Primitives;
using AbstUI.Styles;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Inputs;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkInputCombobox"/>.
/// </summary>
internal class AbstUnityInputCombobox : AbstUnityComponent, IAbstFrameworkInputCombobox, IFrameworkFor<AbstInputCombobox>, IHasTextBackgroundBorderColor
{
    private readonly Dropdown _dropdown;
    private readonly List<KeyValuePair<string, string>> _items = new();
    private readonly Image _image;
    private int _selectedIndex = -1;
    private AColor _textColor = AbstDefaultColors.InputTextColor;
    private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
    private AColor _borderColor = AbstDefaultColors.InputBorderColor;


    #region Properties
    public IReadOnlyList<KeyValuePair<string, string>> Items => _items;

    public bool Enabled
    {
        get => _dropdown.interactable;
        set => _dropdown.interactable = value;
    }


    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value) return;
            _selectedIndex = value;
            _dropdown.value = value;
            UpdateSelection(value);
        }
    }

    public string? SelectedKey
    {
        get => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex].Key : null;
        set
        {
            if (value is null)
            {
                SelectedIndex = -1;
                return;
            }
            var index = _items.FindIndex(it => it.Key == value);
            if (index >= 0)
                SelectedIndex = index;
        }
    }

    public string? SelectedValue
    {
        get => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex].Value : null;
        set
        {
            if (value is null)
            {
                SelectedIndex = -1;
                return;
            }
            var index = _items.FindIndex(it => it.Value == value);
            if (index >= 0)
                SelectedIndex = index;
        }
    }

    public string? ItemFont { get; set; }
    public int ItemFontSize { get; set; } = 11;
    public AColor ItemTextColor { get; set; } = AbstDefaultColors.InputTextColor;
    public AColor ItemSelectedTextColor { get; set; } = AbstDefaultColors.InputSelectionText;
    public AColor ItemSelectedBackgroundColor { get; set; } = AbstDefaultColors.InputAccentColor;
    public AColor ItemSelectedBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;
    public AColor ItemHoverTextColor { get; set; } = AbstDefaultColors.InputTextColor;
    public AColor ItemHoverBackgroundColor { get; set; } = AbstDefaultColors.ListHoverColor;
    public AColor ItemHoverBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;
    public AColor ItemPressedTextColor { get; set; } = AbstDefaultColors.InputSelectionText;
    public AColor ItemPressedBackgroundColor { get; set; } = AbstDefaultColors.InputAccentColor;
    public AColor ItemPressedBorderColor { get; set; } = AbstDefaultColors.InputBorderColor;

    public event Action? ValueChanged;
    public event Action? OnCommit;


    #endregion

    public AbstUnityInputCombobox() : base(CreateGameObject(out var dropdown, out var image))
    {
        _dropdown = dropdown;
        _image = image;
        _dropdown.onValueChanged.AddListener(OnSelectionChanged);
        _image.color = _backgroundColor.ToUnityColor();
    }

    private static GameObject CreateGameObject(out Dropdown dropdown, out Image image)
    {
        var go = new GameObject("Dropdown");
        image = go.AddComponent<Image>();
        dropdown = go.AddComponent<Dropdown>();
        return go;
    }

    private void OnSelectionChanged(int index)
    {
        if (_selectedIndex == index) return;
        _selectedIndex = index;
        UpdateSelection(index);
    }

    private void UpdateSelection(int index)
    {
        if (index >= 0 && index < _items.Count)
        {
            SelectedKey = _items[index].Key;
            SelectedValue = _items[index].Value;
        }
        else
        {
            SelectedKey = null;
            SelectedValue = null;
        }

        ValueChanged?.Invoke();
        OnCommit?.Invoke();
    }
    public void AddItem(string key, string value)
    {
        _items.Add(new KeyValuePair<string, string>(key, value));
        _dropdown.options.Add(new Dropdown.OptionData { text = value });
    }

    public void ClearItems()
    {
        _items.Clear();
        _dropdown.options.Clear();
        SelectedIndex = -1;
    }

    public AColor TextColor
    {
        get => _textColor;
        set
        {
            _textColor = value;
            if (_dropdown.captionText != null)
                _dropdown.captionText.color = value.ToUnityColor();
        }
    }

    public AColor BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            _image.color = value.ToUnityColor();
        }
    }

    public AColor BorderColor
    {
        get => _borderColor;
        set => _borderColor = value;
    }
}
