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
        private readonly ILingoFontManager _fontManager;
        private string? _font;
        private LingoMargin _margin = LingoMargin.Zero;
        private event Action? _onValueChanged;

        public LingoGodotInputText(LingoGfxInputText input, ILingoFontManager fontManager)
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill;

            _fontManager = fontManager;
            input.Init(this);
            TextChanged += _ => _onValueChanged?.Invoke();
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
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

        public new void Dispose()
        {
            base.Dispose();
            QueueFree();
        }
    }
}
