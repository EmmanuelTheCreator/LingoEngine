using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Styles;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxInputNumber"/>.
    /// </summary>
    public partial class LingoGodotInputNumber : LineEdit, ILingoFrameworkGfxInputNumber, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        private LingoNumberType _numberType = LingoNumberType.Float;
        private Action<float>? _onChange;
        private readonly ILingoFontManager _fontManager;

        private event Action? _onValueChanged;

        public LingoGodotInputNumber(LingoGfxInputNumber input, ILingoFontManager fontManager, Action<float>? onChange)
        {
            _onChange = onChange;
            _fontManager = fontManager;
            input.Init(this);
            TextChanged += _ => _onValueChanged?.Invoke();
            if (_onChange != null) TextChanged += _ => _onChange(Value);
            CustomMinimumSize = new Vector2(2, 2);
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
            SizeFlagsVertical = SizeFlags.ShrinkBegin;
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        private float _wantedWidth = 10;
        public float Width { get => Size.X;
            set
            {
                _wantedWidth = value;
                CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
                Size = new Vector2(value, _wantedHeight);
                
                var test = Size;
                
            } }
        private float _wantedHeight = 10;
        public float Height
        {
            get => Size.Y;
            set
            {
                _wantedHeight = Height;
                CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
                Size = new Vector2(_wantedWidth, value);
            }
        }

        public override void _Ready()
        {
            base._Ready();
            CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
            Size = new Vector2(_wantedWidth, _wantedHeight);
            // these are needed in the styling:
            //theme.SetConstant("minimum_height", controlType, 10);
            //theme.SetConstant("minimum_width", controlType, 5);
            //theme.SetConstant("minimum_spaces", controlType, 1);
            //theme.SetConstant("minimum_character_width", controlType, 0);
        }

        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get => Editable; set => Editable = value; }

        private float _value;
        public float Value
        {
            get => _value; set
            {
                _value = value;
                if (_value > Max) _value = Max;
                if (_value < Min) _value = Min;
                Text = _value.ToString();
            }
        }
        public float Min { get; set; }
        public float Max { get; set; }
        string ILingoFrameworkGfxNode.Name { get => Name; set => Name = value; }
        public LingoNumberType NumberType
        {
            get => _numberType;
            set
            {
                _numberType = value;
            }
        }

        private string? _font;
        public string? Font
        {
            get => _font;
            set
            {
                _font = value;
                if (string.IsNullOrEmpty(value))
                {
                    RemoveThemeFontOverride("font");
                }
                else
                {
                    var font = _fontManager.Get<FontFile>(value);
                    if (font != null)
                        AddThemeFontOverride("font", font);
                }
            }
        }
        private int _fontSize;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                Font? baseFont;
                if (string.IsNullOrEmpty(_font))
                    baseFont = _fontManager.GetDefaultFont<Font>();
                else
                    baseFont = _fontManager.Get<Font>(_font);

                if (baseFont == null)
                    return;

                // Create a FontVariation with size applied through theme variation
                var variation = new FontVariation
                {
                    BaseFont = baseFont
                };

                // Set the size override via theme properties
                var theme = new Theme();
                theme.SetFont("font", "LineEdit", variation);
                theme.SetFontSize("font_size", "LineEdit", _fontSize);
                Theme = theme;
            }
        }
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


        event Action? ILingoFrameworkGfxNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }
        public object FrameworkNode => this;
        public new void Dispose()
        {
            TextChanged -= _ => _onValueChanged?.Invoke();
            if (_onChange != null) TextChanged -= _ => _onChange(Value);
            QueueFree();
            base.Dispose();
        }
    }
}
