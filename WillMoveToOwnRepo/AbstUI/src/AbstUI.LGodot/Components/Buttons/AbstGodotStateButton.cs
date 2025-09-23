using Godot;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.LGodot.Bitmaps;
using AbstUI.Components.Inputs;
using AbstUI.Components.Buttons;
using AbstUI.Styles;
using AbstUI.LGodot.Primitives;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkStateButton"/>.
    /// </summary>
    public partial class AbstGodotStateButton : Button, IAbstFrameworkStateButton, IDisposable, IFrameworkFor<AbstStateButton>
    {
        private AMargin _margin = AMargin.Zero;
        private IAbstTexture2D? _texture;
        private IAbstTexture2D? _textureOff;
        private readonly StyleBoxFlat _style = new StyleBoxFlat();
        private readonly StyleBoxFlat _styleHover = new StyleBoxFlat();
        private readonly StyleBoxFlat _stylePressed = new StyleBoxFlat();
        private readonly StyleBoxFlat _styleDisabled = new StyleBoxFlat();
        private Texture2D? _lastIcon;
        private bool _lastHasText;
        private Action<bool>? _onChange;
        private event Action? _onValueChanged;
        private event Action? _onCommit;

        public AbstGodotStateButton(AbstStateButton button, Action<bool>? onChange)
        {
            _onChange = onChange;
            ToggleMode = false;
            CustomMinimumSize = new Vector2(16, 16);
            button.Init(this);
            ResetStyle(_style);
            ResetStyle(_styleHover);
            ResetStyle(_stylePressed);
            ResetStyle(_styleDisabled);

            UpdateStyle();

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
        public float Width { get => Size.X; set { Size = new Vector2(value, Size.Y); CustomMinimumSize = new Vector2(value, CustomMinimumSize.Y); } }
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value); CustomMinimumSize = new Vector2(CustomMinimumSize.X, value); } }
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
        public IAbstTexture2D? TextureOn
        {
            get => _texture;
            set
            {
                _texture = value;
                UpdateStateIcon();
            }
        }
        public IAbstTexture2D? TextureOff
        {
            get => _textureOff;
            set
            {
                _textureOff = value;
                UpdateStateIcon();
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
                UpdateStateIcon();
                _onValueChanged?.Invoke();
                _onChange?.Invoke(IsOn);
                _onCommit?.Invoke();
                UpdateStyle();
            }
        }

        public object FrameworkNode => this;

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

            Pressed -= BtnClicked;
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

        private void UpdateStateIcon()
        {
            if (_texture != null && _texture is AbstGodotTexture2D tex)
                Icon = tex.Texture;
            if (!IsOn && _textureOff != null && _textureOff is AbstGodotTexture2D texOff)
                Icon = texOff.Texture;

            UpdateIconLayout(forceUpdate: true);
        }
        private void UpdateStyle()
        {
            _style.BgColor = _backgroundColor.ToGodotColor();
            _style.BorderColor = _borderColor.ToGodotColor();
            _style.SetBorderWidthAll(1);

            _styleHover.BgColor = _backgroundHoverColor.ToGodotColor();
            _styleHover.BorderColor = _borderHoverColor.ToGodotColor();
            _styleHover.SetBorderWidthAll(1);

            _stylePressed.BgColor = _backgroundPressedColor.ToGodotColor();
            _stylePressed.BorderColor = _borderPressedColor.ToGodotColor();
            _stylePressed.SetBorderWidthAll(1);

            AddThemeStyleboxOverride("normal", _isOn ? _stylePressed : _style);
            AddThemeStyleboxOverride("hover", _isOn ? _stylePressed : _styleHover);
            AddThemeStyleboxOverride("pressed", _stylePressed);
            AddThemeStyleboxOverride("focus", _stylePressed);
            AddThemeStyleboxOverride("disabled", _styleDisabled);
            AddThemeColorOverride("font_color", _textColor.ToGodotColor());
        }

        private void ResetStyle(StyleBoxFlat style)
        {
            style.ContentMarginLeft = style.ContentMarginRight = 0;
            style.ContentMarginTop = style.ContentMarginBottom = 0;
            style.BorderWidthBottom = style.BorderWidthRight = style.BorderWidthLeft = style.BorderWidthTop = 0;
            style.SetBorderWidthAll(0);
        }

        private AColor _borderColor = AbstDefaultColors.Button_Border_Normal;
        private AColor _borderHoverColor = AbstDefaultColors.Button_Border_Hover;
        private AColor _borderPressedColor = AbstDefaultColors.Button_Border_Pressed;
        private AColor _backgroundColor = AbstDefaultColors.Button_Bg_Normal;
        private AColor _backgroundHoverColor = AbstDefaultColors.Button_Bg_Hover;
        private AColor _backgroundPressedColor = AbstDefaultColors.Button_Bg_Pressed;
        private AColor _textColor = AColor.FromRGB(0, 0, 0);

        public AColor BorderColor { get => _borderColor; set { _borderColor = value; UpdateStyle(); } }
        public AColor BorderHoverColor { get => _borderHoverColor; set { _borderHoverColor = value; UpdateStyle(); } }
        public AColor BorderPressedColor { get => _borderPressedColor; set { _borderPressedColor = value; UpdateStyle(); } }
        public AColor BackgroundColor { get => _backgroundColor; set { _backgroundColor = value; UpdateStyle(); } }
        public AColor BackgroundHoverColor { get => _backgroundHoverColor; set { _backgroundHoverColor = value; UpdateStyle(); } }
        public AColor BackgroundPressedColor { get => _backgroundPressedColor; set { _backgroundPressedColor = value; UpdateStyle(); } }
        public AColor TextColor { get => _textColor; set { _textColor = value; UpdateStyle(); } }

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
