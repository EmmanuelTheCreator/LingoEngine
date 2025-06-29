using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using System;
using LingoEngine.Styles;
using LingoEngine.LGodot.Primitives;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxLabel"/>.
    /// </summary>
    public partial class LingoGodotLabel : Label, ILingoFrameworkGfxLabel, IDisposable
    {
        private readonly ILingoFontManager _fontManager;
        private LingoMargin _margin = LingoMargin.Zero;
        private string? _font;
        private LingoColor _fontColor;

        public LingoGodotLabel(LingoGfxLabel label, ILingoFontManager fontManager)
        {
            _fontManager = fontManager;
            label.Init(this);
            LabelSettings = new LabelSettings();
        }

        //public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        //public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
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

        public new string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        public int FontSize
        {
            get => (int)(LabelSettings?.FontSize ?? 0);
            set
            {
                var ls = LabelSettings ?? new LabelSettings();
                ls.FontSize = value;
                LabelSettings = ls;
            }
        }
        private int _lineHeight;
        public int LineHeight
        {
            get => _lineHeight;
            set
            {
                _lineHeight = value;
                var ls = LabelSettings ?? new LabelSettings();
                var font = ls.Font;
                if (font != null)
                {
                    // Get base line height for current font size
                    float lineHeight = font.GetHeight(ls.FontSize);

                    // Add your desired spacing in pixels
                    float spacingInPixels = value;

                    // Compute scale multiplier
                    float spacingMultiplier = (lineHeight + spacingInPixels) / lineHeight;

                    // Apply to label
                    ls.LineSpacing = spacingMultiplier;
                    LabelSettings = ls;
                }

            }
        }

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

        public LingoColor FontColor
        {
            get => _fontColor;
            set
            {
                _fontColor = value;
                var ls = LabelSettings ?? new LabelSettings();
                ls.FontColor = value.ToGodotColor();
                LabelSettings = ls;
            }
        }

        public object FrameworkNode => this;

        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }
    }
}
