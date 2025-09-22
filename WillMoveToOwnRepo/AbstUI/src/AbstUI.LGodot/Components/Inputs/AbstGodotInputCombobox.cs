using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Components.Inputs;
using AbstUI.LGodot.Primitives;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkInputCombobox"/>.
    /// </summary>
    public partial class AbstGodotInputCombobox : OptionButton, IAbstFrameworkInputCombobox, IHasTextBackgroundBorderColor, IDisposable, IFrameworkFor<AbstInputCombobox>
    {
        private readonly List<KeyValuePair<string, string>> _items = new();
        private AMargin _margin = AMargin.Zero;
        private Action<string?>? _onChange;
        private AColor _textColor = AbstDefaultColors.InputTextColor;
        private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
        private AColor _borderColor = AbstDefaultColors.InputBorderColor;
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
        private event Action? _onValueChanged;
        private event Action? _onCommit;
        public object FrameworkNode => this;
        public IReadOnlyList<KeyValuePair<string, string>> Items => _items;



        #region Properties
        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get => !Disabled; set => Disabled = !value; }
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


        public AColor TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                AddThemeColorOverride("font_color", _textColor.ToGodotColor());
            }
        }

        public AColor BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                AddThemeColorOverride("background_color", _backgroundColor.ToGodotColor());
            }
        }

        public AColor BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                AddThemeColorOverride("border_color", _borderColor.ToGodotColor());
            }
        }
        public string? ItemFont { get => _itemFont; set { _itemFont = value; UpdatePopupStyle(); } }
        public int ItemFontSize { get => _itemFontSize; set { _itemFontSize = value; UpdatePopupStyle(); } }
        public AColor ItemTextColor { get => _itemTextColor; set { _itemTextColor = value; UpdatePopupStyle(); } }
        public AColor ItemSelectedTextColor { get => _itemSelectedTextColor; set { _itemSelectedTextColor = value; UpdatePopupStyle(); } }
        public AColor ItemSelectedBackgroundColor { get => _itemSelectedBackgroundColor; set { _itemSelectedBackgroundColor = value; UpdatePopupStyle(); } }
        public AColor ItemSelectedBorderColor { get => _itemSelectedBorderColor; set { _itemSelectedBorderColor = value; UpdatePopupStyle(); } }
        public AColor ItemHoverTextColor { get => _itemHoverTextColor; set { _itemHoverTextColor = value; UpdatePopupStyle(); } }
        public AColor ItemHoverBackgroundColor { get => _itemHoverBackgroundColor; set { _itemHoverBackgroundColor = value; UpdatePopupStyle(); } }
        public AColor ItemHoverBorderColor { get => _itemHoverBorderColor; set { _itemHoverBorderColor = value; UpdatePopupStyle(); } }
        public AColor ItemPressedTextColor { get => _itemPressedTextColor; set { _itemPressedTextColor = value; UpdatePopupStyle(); } }
        public AColor ItemPressedBackgroundColor { get => _itemPressedBackgroundColor; set { _itemPressedBackgroundColor = value; UpdatePopupStyle(); } }
        public AColor ItemPressedBorderColor { get => _itemPressedBorderColor; set { _itemPressedBorderColor = value; UpdatePopupStyle(); } }


        #endregion


        public AbstGodotInputCombobox(AbstInputCombobox input, IAbstFontManager blingoFontManager, Action<string?>? onChange)
        {
            input.Init(this);
            _onChange = onChange;
            ItemSelected += OnItemSelected;

        }
        private void UpdatePopupStyle()
        {
            var popup = GetPopup();
            popup.AddThemeFontSizeOverride("font_size", _itemFontSize);
            popup.AddThemeColorOverride("font_color", _itemTextColor.ToGodotColor());
            popup.AddThemeColorOverride("font_color_hover", _itemHoverTextColor.ToGodotColor());
            popup.AddThemeColorOverride("font_color_selected", _itemSelectedTextColor.ToGodotColor());
            popup.AddThemeColorOverride("font_color_pressed", _itemPressedTextColor.ToGodotColor());

            var selected = new StyleBoxFlat
            {
                BgColor = _itemSelectedBackgroundColor.ToGodotColor(),
                BorderColor = _itemSelectedBorderColor.ToGodotColor()
            };
            selected.SetBorderWidthAll(1);
            popup.AddThemeStyleboxOverride("selected", selected);

            var hover = new StyleBoxFlat
            {
                BgColor = _itemHoverBackgroundColor.ToGodotColor(),
                BorderColor = _itemHoverBorderColor.ToGodotColor()
            };
            hover.SetBorderWidthAll(1);
            popup.AddThemeStyleboxOverride("hover", hover);

            var pressed = new StyleBoxFlat
            {
                BgColor = _itemPressedBackgroundColor.ToGodotColor(),
                BorderColor = _itemPressedBorderColor.ToGodotColor()
            };
            pressed.SetBorderWidthAll(1);
            popup.AddThemeStyleboxOverride("pressed", pressed);

            var normal = new StyleBoxFlat
            {
                BgColor = _backgroundColor.ToGodotColor(),
                BorderColor = _borderColor.ToGodotColor()
            };
            popup.AddThemeStyleboxOverride("normal", normal);


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
            ItemSelected -= OnItemSelected;
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

