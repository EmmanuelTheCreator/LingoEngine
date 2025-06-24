using Godot;
using LingoEngine.Gfx;
using System;
using System.Collections.Generic;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkInputCombobox"/>.
    /// </summary>
    public partial class LingoGodotInputCombobox : OptionButton, ILingoFrameworkInputCombobox, IDisposable
    {
        private readonly List<KeyValuePair<string,string>> _items = new();
        private LingoMargin _margin = LingoMargin.Zero;
        private event Action? _onValueChanged;

        public LingoGodotInputCombobox(LingoInputCombobox input)
        {
            input.Init(this);
            ItemSelected += idx => _onValueChanged?.Invoke();
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get => !Disabled; set => Disabled = !value; }

        public LingoMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                AddThemeConstantOverride("margin_left", (int)_margin.Left);
                AddThemeConstantOverride("margin_right", (int)_margin.Right);
                AddThemeConstantOverride("margin_top", (int)_margin.Top);
                AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
            }
        }

        public IReadOnlyList<KeyValuePair<string,string>> Items => _items;
        public void AddItem(string key, string value)
        {
            _items.Add(new KeyValuePair<string,string>(key,value));
            int idx = ItemCount;
            base.AddItem(value);
            SetItemMetadata(idx, key);
        }
        public void ClearItems()
        {
            _items.Clear();
            Clear();
        }
        public int SelectedIndex { get => Selected; set => Selected = value; }
        public string? SelectedKey
        {
            get => Selected >= 0 ? (string?)GetItemMetadata(Selected) : null;
            set
            {
                if (value is null) { Selected = -1; return; }
                for (int i = 0; i < ItemCount; i++)
                {
                    if ((string?)GetItemMetadata(i) == value)
                    {
                        Selected = i;
                        break;
                    }
                }
            }
        }
        public string? SelectedValue
        {
            get => Selected >= 0 ? GetItemText(Selected) : null;
            set
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if (GetItemText(i) == value)
                    {
                        Selected = i;
                        break;
                    }
                }
            }
        }

        event Action? ILingoFrameworkInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        public void Dispose() => QueueFree();
    }
}
