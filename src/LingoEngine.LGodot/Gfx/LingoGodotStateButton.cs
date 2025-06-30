using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.LGodot.Bitmaps;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxStateButton"/>.
    /// </summary>
    public partial class LingoGodotStateButton : Button, ILingoFrameworkGfxStateButton, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        private ILingoImageTexture? _texture;
        private readonly StyleBoxFlat _style = new StyleBoxFlat();
        private readonly StyleBoxFlat _styleDisabled = new StyleBoxFlat();
        private readonly StyleBoxFlat _styleActive = new StyleBoxFlat();
        private Action<bool>? _onChange;
        private event Action? _onValueChanged;

        public LingoGodotStateButton(LingoGfxStateButton button, Action<bool>? onChange)
        {
            _onChange = onChange;
            ToggleMode = false;
            CustomMinimumSize = new Vector2(16, 16);
            button.Init(this);
            _style.BgColor = Colors.Transparent;
            ResetStyle(_style);
            ResetStyle(_styleDisabled);
            ResetStyle(_styleActive);
            



            AddThemeStyleboxOverride("normal", _style);
            AddThemeStyleboxOverride("hover", _style);
            AddThemeStyleboxOverride("pressed", _style);
            AddThemeStyleboxOverride("focus", _styleActive);
            AddThemeStyleboxOverride("disabled", _styleDisabled);

            //Toggled += _toggleHandler;
            Pressed += BtnClicked;
            //IsOn = false;
        }

        private void BtnClicked()
        {
            if (Disabled) return;
            IsOn = !IsOn;
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => CustomMinimumSize.X; set => CustomMinimumSize = new Vector2(value, CustomMinimumSize.Y); }
        public float Height { get => CustomMinimumSize.Y; set => CustomMinimumSize = new Vector2(CustomMinimumSize.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled
        {
            get => !Disabled;
            set
            {
                Disabled = !value;
                Modulate = value ? Colors.White : new Color(1, 1, 1, 0.5f); // 50% transparent
            }
        }
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

        public new string Text { get => base.Text; set => base.Text = value; }
        public ILingoImageTexture? Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                if (value is LingoGodotImageTexture tex) 
                    Icon = tex.Texture;
                    
            }
        }
        private bool _isOn;
        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn == value) return;
                _isOn = value;
                _onValueChanged?.Invoke();
                _onChange?.Invoke(IsOn);
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
           
            Pressed -= BtnClicked;
            QueueFree();
            base.Dispose();
        }

        private void UpdateStyle()
        {
            ResetStyle(_style);
            ResetStyle(_styleDisabled);
            if (_isOn)
                _style.BgColor = Colors.DarkGray;
            else
                _style.BgColor = Colors.Transparent;

            
            AddThemeStyleboxOverride("normal", _style);
            AddThemeStyleboxOverride("hover", _style);
            AddThemeStyleboxOverride("pressed", _style);
            AddThemeStyleboxOverride("focus", _style);
        }

        private void ResetStyle(StyleBoxFlat style)
        {
            style.ContentMarginLeft = style.ContentMarginRight = 0;
            style.ContentMarginTop = style.ContentMarginBottom = 0;
            style.BorderWidthBottom = style.BorderWidthRight = style.BorderWidthLeft = style.BorderWidthTop = 0;
            style.SetBorderWidthAll(0);
        }
    }
}
