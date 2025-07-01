using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxItemList"/>.
    /// </summary>
    public partial class LingoGodotItemList : ItemList, ILingoFrameworkGfxItemList, IDisposable
    {
        private readonly List<KeyValuePair<string,string>> _items = new();
        private LingoMargin _margin = LingoMargin.Zero;
        private Action<string?>? _onChange;
        private event Action? _onValueChanged;
        private ItemSelectedEventHandler? _onItemSelected;

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set { Size = new Vector2(value, Size.Y); CustomMinimumSize = new Vector2(value, Size.Y); } }
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value); CustomMinimumSize = new Vector2(Size.X, value); } }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get; set; } // { get => !Disabled; set => Disabled = !value; }
        string ILingoFrameworkGfxNode.Name { get => Name; set => Name = value; }

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

        public object FrameworkNode => this;
        public IReadOnlyList<KeyValuePair<string, string>> Items => _items;

        public LingoGodotItemList(LingoGfxItemList list, Action<string?>? onChange)
        {
            _onChange = onChange;
            list.Init(this);
            _onItemSelected = idx =>
            {
                _onValueChanged?.Invoke();
                _onChange?.Invoke(SelectedKey);
            };
            ItemSelected += _onItemSelected;
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
            SizeFlagsVertical = SizeFlags.ExpandFill;
            CustomMinimumSize = new Vector2(100, 50);
        }


        public void AddItem(string key, string value)
        {
            _items.Add(new KeyValuePair<string,string>(key, value));
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

        event Action? ILingoFrameworkGfxNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        public new void Dispose()
        {
            if (_onItemSelected != null) ItemSelected -= _onItemSelected;
            QueueFree();
            base.Dispose();
        }
    }
}
