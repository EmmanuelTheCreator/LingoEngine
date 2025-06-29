using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Styles;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxInputText"/>.
    /// </summary>
    public partial class LingoGodotInputText : LineEdit, ILingoFrameworkGfxInputText, IDisposable
    {
        private readonly Action<string>? _onChange;
        private readonly ILingoFontManager _fontManager;
        private string? _font;
        private LingoMargin _margin = LingoMargin.Zero;
        private event Action? _onValueChanged;

        public LingoGodotInputText(LingoGfxInputText input, ILingoFontManager fontManager, Action<string>? onChange)
        {
            _onChange = onChange;
            _fontManager = fontManager;
            input.Init(this);
            TextChanged += _ => _onValueChanged?.Invoke();
            CustomMinimumSize = new Vector2(2, 2);
            //Height = 8;
            //Width = 100;
            SizeFlagsHorizontal =0;
            SizeFlagsVertical =0;
            //Size = new Vector2(200, 30);

        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y;
            set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get => Editable; set => Editable = value; }

        public new string Text { get => base.Text; set => base.Text = value; }
        public new int MaxLength { get => base.MaxLength; set => base.MaxLength = value; }
        string ILingoFrameworkGfxNode.Name { get => Name; set => Name = value; }
        public string? Font
        {
            get => _font;
            set
            {
                _font = value;
                //if (string.IsNullOrEmpty(value))
                //{
                //    RemoveThemeFontOverride("font");
                //}
                //else
                //{
                //    var font = _fontManager.Get<FontFile>(value);
                //    if (font != null)
                //        AddThemeFontOverride("font", font);
                //}
            }
        }
        private int _fontSize;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;

                //Font? baseFont = null;
                //if (string.IsNullOrEmpty(_font))
                //    baseFont = _fontManager.GetDefaultFont<Font>();
                //else
                //    baseFont = _fontManager.Get<Font>(_font);
                //if (baseFont == null)
                //    return;

                //// Create a FontVariation with size applied through theme variation
                //var variation = new FontVariation
                //{
                //    BaseFont = baseFont
                //};

                //// Set the size override via theme properties
                //var theme = new Theme();
                //theme.SetFont("font", "LineEdit", variation);
                //theme.SetFontSize("font_size", "LineEdit", _fontSize);
                //Theme = theme;
            }
        }
        private Color _fontColor = Colors.Black;

        public Color FontColor
        {
            get => _fontColor;
            set
            {
                _fontColor = value;
                //AddThemeColorOverride("font_color", _fontColor);
            }
        }

        public LingoMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                //AddThemeConstantOverride("margin_left", (int)_margin.Left);
                //AddThemeConstantOverride("margin_right", (int)_margin.Right);
                //AddThemeConstantOverride("margin_top", (int)_margin.Top);
                //AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
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
            QueueFree();
            base.Dispose();
        }
    }
}
