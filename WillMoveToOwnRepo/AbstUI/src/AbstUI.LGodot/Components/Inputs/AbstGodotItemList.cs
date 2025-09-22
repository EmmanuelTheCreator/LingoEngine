using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Components.Inputs;
using AbstUI.Styles;
using AbstUI.LGodot.Primitives;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkItemList"/>.
    /// </summary>
    public partial class AbstGodotItemList : ItemList, IAbstFrameworkItemList, IDisposable, IFrameworkFor<AbstItemList>
    {
        private readonly List<KeyValuePair<string, string>> _items = new();
        private AMargin _margin = AMargin.Zero;
        private Action<string?>? _onChange;
        private event Action? _onValueChanged;
        private event Action? _onCommit;
        private ItemSelectedEventHandler? _onItemSelected;

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set { Size = new Vector2(value, Size.Y); CustomMinimumSize = new Vector2(value, Size.Y); } }
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value); CustomMinimumSize = new Vector2(Size.X, value); } }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get; set; } // { get => !Disabled; set => Disabled = !value; }
        string IAbstFrameworkNode.Name { get => Name; set => Name = value; }

        public AMargin Margin
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

        public object FrameworkNode => this;
        public IReadOnlyList<KeyValuePair<string, string>> Items => _items;

        public AbstGodotItemList(AbstItemList list, Action<string?>? onChange)
        {
            _onChange = onChange;
            list.Init(this);
            _onItemSelected = OnItemSelected;
            ItemSelected += _onItemSelected;
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
            SizeFlagsVertical = SizeFlags.ExpandFill;
            CustomMinimumSize = new Vector2(100, 50);
            UpdateItemStyle();
        }


        public void AddItem(string key, string value)
        {
            _items.Add(new KeyValuePair<string, string>(key, value));
            int idx = ItemCount;
            base.AddItem(value);
            SetItemMetadata(idx, key);
        }
        public void ClearItems()
        {
            _items.Clear();
            Clear();
        }
        public int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value < 0 || value >= ItemCount)
                {
                    DeselectAll();
                }
                else
                {
                    _selectedIndex = value;
                    Select(value);
                }
            }
        }
        public string? SelectedKey
        {
            get => SelectedIndex >= 0 ? (string?)GetItemMetadata(SelectedIndex) : null;
            set
            {
                if (value is null) { DeselectAll(); return; }
                for (int i = 0; i < ItemCount; i++)
                {
                    if ((string?)GetItemMetadata(i) == value)
                    {
                        Select(i);
                        break;
                    }
                }
            }
        }
        public string? SelectedValue
        {
            get => SelectedIndex >= 0 ? GetItemText(SelectedIndex) : null;
            set
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if (GetItemText(i) == value)
                    {
                        Select(i);
                        break;
                    }
                }
            }
        }

        private string? _itemFont;
        private int _itemFontSize = 11;
        private AColor _itemTextColor = AbstDefaultColors.InputTextColor;
        private AColor _itemSelectedTextColor = AbstDefaultColors.InputSelectionText;
        private AColor _itemSelectedBackgroundColor = AbstDefaultColors.InputAccentColor;
        private AColor _itemSelectedBorderColor = AbstDefaultColors.InputBorderColor;
        private AColor _itemHoverTextColor = AbstDefaultColors.InputTextColor;
        private AColor _itemHoverBackgroundColor = AbstDefaultColors.ListHoverColor;
        private AColor _itemHoverBorderColor = AbstDefaultColors.InputBorderColor;
        private AColor _itemPressedTextColor = AbstDefaultColors.InputSelectionText;
        private AColor _itemPressedBackgroundColor = AbstDefaultColors.InputAccentColor;
        private AColor _itemPressedBorderColor = AbstDefaultColors.InputBorderColor;

        public string? ItemFont { get => _itemFont; set { _itemFont = value; UpdateItemStyle(); } }
        public int ItemFontSize { get => _itemFontSize; set { _itemFontSize = value; UpdateItemStyle(); } }
        public AColor ItemTextColor { get => _itemTextColor; set { _itemTextColor = value; UpdateItemStyle(); } }
        public AColor ItemSelectedTextColor { get => _itemSelectedTextColor; set { _itemSelectedTextColor = value; UpdateItemStyle(); } }
        public AColor ItemSelectedBackgroundColor { get => _itemSelectedBackgroundColor; set { _itemSelectedBackgroundColor = value; UpdateItemStyle(); } }
        public AColor ItemSelectedBorderColor { get => _itemSelectedBorderColor; set { _itemSelectedBorderColor = value; UpdateItemStyle(); } }
        public AColor ItemHoverTextColor { get => _itemHoverTextColor; set { _itemHoverTextColor = value; UpdateItemStyle(); } }
        public AColor ItemHoverBackgroundColor { get => _itemHoverBackgroundColor; set { _itemHoverBackgroundColor = value; UpdateItemStyle(); } }
        public AColor ItemHoverBorderColor { get => _itemHoverBorderColor; set { _itemHoverBorderColor = value; UpdateItemStyle(); } }
        public AColor ItemPressedTextColor { get => _itemPressedTextColor; set { _itemPressedTextColor = value; UpdateItemStyle(); } }
        public AColor ItemPressedBackgroundColor { get => _itemPressedBackgroundColor; set { _itemPressedBackgroundColor = value; UpdateItemStyle(); } }
        public AColor ItemPressedBorderColor { get => _itemPressedBorderColor; set { _itemPressedBorderColor = value; UpdateItemStyle(); } }

        private void UpdateItemStyle()
        {
            AddThemeFontSizeOverride("font_size", _itemFontSize);
            AddThemeColorOverride("font_color", _itemTextColor.ToGodotColor());
            AddThemeColorOverride("font_color_hover", _itemHoverTextColor.ToGodotColor());
            AddThemeColorOverride("font_color_selected", _itemSelectedTextColor.ToGodotColor());
            AddThemeColorOverride("font_color_pressed", _itemPressedTextColor.ToGodotColor());

            var selected = new StyleBoxFlat
            {
                BgColor = _itemSelectedBackgroundColor.ToGodotColor(),
                BorderColor = _itemSelectedBorderColor.ToGodotColor()
            };
            selected.SetBorderWidthAll(1);
            AddThemeStyleboxOverride("selected", selected);

            var hover = new StyleBoxFlat
            {
                BgColor = _itemHoverBackgroundColor.ToGodotColor(),
                BorderColor = _itemHoverBorderColor.ToGodotColor()
            };
            hover.SetBorderWidthAll(1);
            AddThemeStyleboxOverride("hover", hover);

            var pressed = new StyleBoxFlat
            {
                BgColor = _itemPressedBackgroundColor.ToGodotColor(),
                BorderColor = _itemPressedBorderColor.ToGodotColor()
            };
            pressed.SetBorderWidthAll(1);
            AddThemeStyleboxOverride("pressed", pressed);
        }

        event Action? IAbstFrameworkNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        event Action? IAbstFrameworkNodeInput.OnCommit
        {
            add => _onCommit += value;
            remove => _onCommit -= value;
        }

        public new void Dispose()
        {
            if (_onItemSelected != null) ItemSelected -= _onItemSelected;
            QueueFree();
            base.Dispose();
        }

        private void OnItemSelected(long index)
        {
            _onValueChanged?.Invoke();
            _onCommit?.Invoke();

            if (index >= 0 && index < _items.Count)
            {
                _onChange?.Invoke(_items[(int)index].Key);
            }
        }
    }
}
