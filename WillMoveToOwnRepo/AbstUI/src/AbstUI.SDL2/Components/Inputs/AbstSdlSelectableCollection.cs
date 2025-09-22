using System;
using System.Collections.Generic;
using AbstUI.Primitives;
using AbstUI.Components.Inputs;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.SDL2.Components.Containers;
using AbstUI.Styles;

namespace AbstUI.SDL2.Components.Inputs
{
    internal abstract class AbstSdlSelectableCollection : AbstSdlScrollViewer, IAbstSdlHasSelectableCollectionStyle
    {
        protected AbstSdlSelectableCollection(AbstSdlComponentFactory factory, AbstSDLComponentContext? parent = null) : base(factory, parent)
        {
        }

        public bool Enabled { get; set; } = true;

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

        protected readonly List<KeyValuePair<string, string>> ItemsInternal = new();
        public IReadOnlyList<KeyValuePair<string, string>> Items => ItemsInternal;

        private int _selectedIndex = -1;
        public virtual int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                if (value >= 0 && value < ItemsInternal.Count)
                {
                    SelectedKey = ItemsInternal[value].Key;
                    SelectedValue = ItemsInternal[value].Value;
                }
                else
                {
                    SelectedKey = null;
                    SelectedValue = null;
                }
                ComponentContext.QueueRedraw(this);
                ValueChanged?.Invoke();
                OnCommit?.Invoke();
            }
        }

        public string? SelectedKey { get; set; }
        public string? SelectedValue { get; set; }

        public event Action? ValueChanged;
        public event Action? OnCommit;
        public object FrameworkNode => this;

        public virtual void AddItem(string key, string value)
        {
            ItemsInternal.Add(new KeyValuePair<string, string>(key, value));
            ComponentContext.QueueRedraw(this);
        }

        public virtual bool RemoveItem(string key)
        {
            int idx = ItemsInternal.FindIndex(it => it.Key == key);
            if (idx < 0) return false;
            ItemsInternal.RemoveAt(idx);
            if (_selectedIndex == idx)
            {
                _selectedIndex = -1;
                SelectedKey = null;
                SelectedValue = null;
            }
            else if (_selectedIndex > idx)
            {
                _selectedIndex--;
            }
            ComponentContext.QueueRedraw(this);
            return true;
        }

        public virtual void ClearItems()
        {
            ItemsInternal.Clear();
            _selectedIndex = -1;
            SelectedKey = null;
            SelectedValue = null;
            ComponentContext.QueueRedraw(this);
        }
    }
}
