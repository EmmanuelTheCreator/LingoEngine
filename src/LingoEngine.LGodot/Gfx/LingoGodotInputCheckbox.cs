using Godot;
using LingoEngine.Gfx;
using System;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxInputCheckbox"/>.
    /// </summary>
    public partial class LingoGodotInputCheckbox : CheckBox, ILingoFrameworkGfxInputCheckbox, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        private event Action? _onValueChanged;
        public LingoGodotInputCheckbox(LingoInputCheckbox input)
        {
            input.Init(this);
            Toggled += _ => _onValueChanged?.Invoke();
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get => !Disabled; set => Disabled = !value; }

        public bool Checked { get => ButtonPressed; set => ButtonPressed = value; }

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

        public void Dispose() => QueueFree();
    }
}
