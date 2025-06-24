using Godot;
using LingoEngine.Gfx;
using LingoEngine.Events;
using System;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkInputText"/>.
    /// </summary>
    public partial class LingoGodotInputText : LineEdit, ILingoFrameworkInputText, IDisposable
    {
        private readonly ILingoFontManager _fontManager;
        private string? _font;

        public LingoGodotInputText(LingoInputText input, ILingoFontManager fontManager)
        {
            _fontManager = fontManager;
            input.Init(this);
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }

        public string Text { get => base.Text; set => base.Text = value; }
        public int MaxLength { get => base.MaxLength; set => base.MaxLength = value; }
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

        public void Dispose() => QueueFree();
    }
}
