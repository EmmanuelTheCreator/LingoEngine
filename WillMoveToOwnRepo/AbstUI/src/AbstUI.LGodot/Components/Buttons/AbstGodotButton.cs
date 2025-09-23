using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.LGodot.Bitmaps;
using AbstUI.Components.Buttons;
using AbstUI.LGodot.Primitives;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkButton"/>.
    /// </summary>
    public partial class AbstGodotButton : Button, IAbstFrameworkButton, IDisposable, IFrameworkFor<AbstButton>
    {
        private AMargin _margin = AMargin.Zero;
        private IAbstTexture2D? _texture;
        private readonly StyleBoxFlat _style = new StyleBoxFlat();
        private readonly StyleBoxFlat _styleHover = new StyleBoxFlat();
        private readonly StyleBoxFlat _stylePressed = new StyleBoxFlat();
        private readonly StyleBoxFlat _styleDisabled = new StyleBoxFlat();
        private Texture2D? _lastIcon;
        private bool _lastHasText;

        private AColor _borderColor = AbstDefaultColors.Button_Border_Normal;
        private AColor _backgroundColor = AbstDefaultColors.Button_Bg_Normal;
        private AColor _backgroundHoverColor = AbstDefaultColors.Button_Bg_Hover;
        private AColor _textColor = AColor.FromRGB(0, 0, 0);

        private event Action? _pressed;

        public object FrameworkNode => this;

        public AbstGodotButton(AbstButton button, IAbstFontManager blingoFontManager)
        {
            button.Init(this);
            ResetStyle(_style);
            ResetStyle(_styleHover);
            ResetStyle(_stylePressed);
            ResetStyle(_styleDisabled);
            UpdateStyle();
            Pressed += () => _pressed?.Invoke();
        }

        //public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        //public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set { Size = new Vector2(value, Size.Y); CustomMinimumSize = new Vector2(value, Size.Y); } }
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value); CustomMinimumSize = new Vector2(Size.X, value); } }

        public bool Visibility { get => Visible; set => Visible = value; }


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

        public new string Text
        {
            get => base.Text;
            set
            {
                base.Text = value ?? string.Empty;
                UpdateIconLayout(forceUpdate: true);
            }
        }
        public bool Enabled { get => !Disabled; set => Disabled = !value; }
        public AColor BorderColor { get => _borderColor; set { _borderColor = value; UpdateStyle(); } }
        public AColor BackgroundColor { get => _backgroundColor; set { _backgroundColor = value; UpdateStyle(); } }
        public AColor BackgroundHoverColor { get => _backgroundHoverColor; set { _backgroundHoverColor = value; UpdateStyle(); } }
        public AColor TextColor { get => _textColor; set { _textColor = value; UpdateStyle(); } }
        event Action? IAbstFrameworkButton.Pressed
        {
            add => _pressed += value;
            remove => _pressed -= value;
        }

        public IAbstTexture2D? IconTexture
        {
            get => _texture;
            set
            {
                _texture = value;
                if (value is AbstGodotTexture2D tex)
                    Icon = tex.Texture;
                else
                    Icon = null;
                UpdateIconLayout(forceUpdate: true);
            }
        }

        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }

        public override void _Ready()
        {
            base._Ready();
            SetProcess(true);
            UpdateIconLayout(forceUpdate: true);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (_lastIcon != Icon || _lastHasText != !string.IsNullOrWhiteSpace(base.Text))
            {
                UpdateIconLayout();
            }
        }

        private void UpdateStyle()
        {
            _style.BgColor = _backgroundColor.ToGodotColor();
            _style.BorderColor = _borderColor.ToGodotColor();
            _style.SetBorderWidthAll(1);

            _styleHover.BgColor = _backgroundHoverColor.ToGodotColor();
            _styleHover.BorderColor = _borderColor.ToGodotColor();
            _styleHover.SetBorderWidthAll(1);

            _stylePressed.BgColor = AbstDefaultColors.Button_Bg_Pressed.ToGodotColor();
            _stylePressed.BorderColor = _borderColor.ToGodotColor();
            _stylePressed.SetBorderWidthAll(1);

            _styleDisabled.BgColor = AbstDefaultColors.Button_Bg_Disabled.ToGodotColor();
            _styleDisabled.BorderColor = _borderColor.ToGodotColor();
            _styleDisabled.SetBorderWidthAll(1);

            AddThemeStyleboxOverride("normal", _style);
            AddThemeStyleboxOverride("hover", _styleHover);
            AddThemeStyleboxOverride("pressed", _stylePressed);
            AddThemeStyleboxOverride("focus", _stylePressed);
            AddThemeStyleboxOverride("disabled", _styleDisabled);
            AddThemeColorOverride("font_color", _textColor.ToGodotColor());
        }

        private static void ResetStyle(StyleBoxFlat style)
        {
            style.ContentMarginLeft = style.ContentMarginRight = 0;
            style.ContentMarginTop = style.ContentMarginBottom = 0;
            style.BorderWidthBottom = style.BorderWidthRight = style.BorderWidthLeft = style.BorderWidthTop = 0;
            style.SetBorderWidthAll(0);
        }

        private void UpdateIconLayout(bool forceUpdate = false)
        {
            bool hasText = !string.IsNullOrWhiteSpace(base.Text);

            if (forceUpdate || _lastIcon != Icon || _lastHasText != hasText)
            {
                IconAlignment = hasText ? HorizontalAlignment.Left : HorizontalAlignment.Center;
                _lastIcon = Icon;
                _lastHasText = hasText;
            }
        }

    }
}

