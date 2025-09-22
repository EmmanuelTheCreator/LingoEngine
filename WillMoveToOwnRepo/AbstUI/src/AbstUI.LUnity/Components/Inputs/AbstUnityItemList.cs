using System;
using System.Collections.Generic;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using AbstUI.Primitives;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Inputs;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkItemList"/>.
/// </summary>
internal class AbstUnityItemList : AbstUnityComponent, IAbstFrameworkItemList, IFrameworkFor<AbstItemList>
{
    private readonly Dropdown _dropdown;
    private readonly List<KeyValuePair<string, string>> _items = new();

    private string? _itemFont;
    private int _itemFontSize;
    private AColor _itemTextColor = AColor.FromRGB(0, 0, 0);
    private AColor _itemSelectedTextColor = AColor.FromRGB(0, 0, 0);
    private AColor _itemSelectedBackgroundColor = AColor.FromRGB(255, 255, 255);
    private AColor _itemSelectedBorderColor = AColor.FromRGB(0, 0, 0);
    private AColor _itemHoverTextColor = AColor.FromRGB(0, 0, 0);
    private AColor _itemHoverBackgroundColor = AColor.FromRGB(255, 255, 255);
    private AColor _itemHoverBorderColor = AColor.FromRGB(0, 0, 0);
    private AColor _itemPressedTextColor = AColor.FromRGB(0, 0, 0);
    private AColor _itemPressedBackgroundColor = AColor.FromRGB(255, 255, 255);
    private AColor _itemPressedBorderColor = AColor.FromRGB(0, 0, 0);

    public AbstUnityItemList() : base(CreateGameObject(out var dropdown))
    {
        _dropdown = dropdown;
        _dropdown.onValueChanged.AddListener(_ =>
        {
            ValueChanged?.Invoke();
            OnCommit?.Invoke();
        });
    }

    private static GameObject CreateGameObject(out Dropdown dropdown)
    {
        var go = new GameObject("ItemList");
        dropdown = go.AddComponent<Dropdown>();
        return go;
    }

    public IReadOnlyList<KeyValuePair<string, string>> Items => _items;

    public void AddItem(string key, string value)
    {
        _items.Add(new KeyValuePair<string, string>(key, value));
        _dropdown.options.Add(new Dropdown.OptionData(value));
    }

    public void ClearItems()
    {
        _items.Clear();
        _dropdown.options.Clear();
    }

    public int SelectedIndex
    {
        get => _dropdown.value;
        set => _dropdown.value = value;
    }

    public string? SelectedKey
    {
        get => SelectedIndex >= 0 && SelectedIndex < _items.Count ? _items[SelectedIndex].Key : null;
        set
        {
            var idx = _items.FindIndex(kv => kv.Key == value);
            if (idx >= 0)
                SelectedIndex = idx;
        }
    }

    public string? SelectedValue
    {
        get => SelectedIndex >= 0 && SelectedIndex < _items.Count ? _items[SelectedIndex].Value : null;
        set
        {
            var idx = _items.FindIndex(kv => kv.Value == value);
            if (idx >= 0)
                SelectedIndex = idx;
        }
    }

    public bool Enabled
    {
        get => _dropdown.interactable;
        set => _dropdown.interactable = value;
    }

    public event Action? ValueChanged;
    public event Action? OnCommit;

    public string? ItemFont
    {
        get => _itemFont;
        set => _itemFont = value;
    }

    public int ItemFontSize
    {
        get => _itemFontSize;
        set => _itemFontSize = value;
    }

    public AColor ItemTextColor
    {
        get => _itemTextColor;
        set => _itemTextColor = value;
    }

    public AColor ItemSelectedTextColor
    {
        get => _itemSelectedTextColor;
        set => _itemSelectedTextColor = value;
    }

    public AColor ItemSelectedBackgroundColor
    {
        get => _itemSelectedBackgroundColor;
        set => _itemSelectedBackgroundColor = value;
    }

    public AColor ItemSelectedBorderColor
    {
        get => _itemSelectedBorderColor;
        set => _itemSelectedBorderColor = value;
    }

    public AColor ItemHoverTextColor
    {
        get => _itemHoverTextColor;
        set => _itemHoverTextColor = value;
    }

    public AColor ItemHoverBackgroundColor
    {
        get => _itemHoverBackgroundColor;
        set => _itemHoverBackgroundColor = value;
    }

    public AColor ItemHoverBorderColor
    {
        get => _itemHoverBorderColor;
        set => _itemHoverBorderColor = value;
    }

    public AColor ItemPressedTextColor
    {
        get => _itemPressedTextColor;
        set => _itemPressedTextColor = value;
    }

    public AColor ItemPressedBackgroundColor
    {
        get => _itemPressedBackgroundColor;
        set => _itemPressedBackgroundColor = value;
    }

    public AColor ItemPressedBorderColor
    {
        get => _itemPressedBorderColor;
        set => _itemPressedBorderColor = value;
    }
}
