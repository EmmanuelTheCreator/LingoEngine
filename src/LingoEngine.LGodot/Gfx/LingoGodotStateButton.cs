using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Bitmaps;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxStateButton"/>.
    /// </summary>
    public partial class LingoGodotStateButton : Button, ILingoFrameworkGfxStateButton, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        private Bitmaps.ILingoTexture2D? _texture;
        private readonly StyleBoxFlat _style = new StyleBoxFlat();
        private readonly Action<bool> _toggleHandler;
        private Action<bool>? _onChange;
        private event Action? _onValueChanged;

        public LingoGodotStateButton(LingoGfxStateButton button, Action<bool>? onChange)
        {
            _onChange = onChange;
            ToggleMode = true;
            CustomMinimumSize = new Vector2(2, 2);
            button.Init(this);
            _toggleHandler = pressed =>
            {
                UpdateStyle();
                _onValueChanged?.Invoke();
                _onChange?.Invoke(pressed);
            };
            Toggled += _toggleHandler;
            UpdateStyle();
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => CustomMinimumSize.X; set => CustomMinimumSize = new Vector2(value, CustomMinimumSize.Y); }
        public float Height { get => CustomMinimumSize.Y; set => CustomMinimumSize = new Vector2(CustomMinimumSize.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get => !Disabled; set => Disabled = !value; }
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

        public string Text { get => base.Text; set => base.Text = value; }
        public Bitmaps.ILingoTexture2D? Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                if (value is Bitmaps.ILingoTexture2D tex && tex is LingoEngine.LGodot.Bitmaps.LingoGodotTexture2D godot)
                    Icon = godot.Texture;
            }
        }
        public bool IsOn
        {
            get => ButtonPressed;
            set
            {
                ButtonPressed = value;
                UpdateStyle();
            }
        }

        public object FrameworkNode => this;

        event Action? ILingoFrameworkGfxNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        public new void Dispose()
        {
            Toggled -= _toggleHandler;
            QueueFree();
            base.Dispose();
        }

        private void UpdateStyle()
        {
            _style.ContentMarginLeft = _style.ContentMarginRight = 0;
            _style.ContentMarginTop = _style.ContentMarginBottom = 0;
            if (ButtonPressed)
            {
                _style.BgColor = Colors.DarkGray;
                _style.BorderWidthAll = 1;
                _style.BorderColor = Colors.White;
            }
            else
            {
                _style.BgColor = Colors.Transparent;
                _style.BorderWidthAll = 0;
            }

            AddThemeStyleboxOverride("normal", _style);
            AddThemeStyleboxOverride("hover", _style);
        }
    }
}
