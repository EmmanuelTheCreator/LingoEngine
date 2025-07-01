using Godot;
using LingoEngine.Gfx;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Primitives;
using System;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxColorPicker"/>.
    /// </summary>
    public partial class LingoGodotColorPicker : ColorPickerButton, ILingoFrameworkGfxColorPicker, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        private readonly Action<LingoColor>? _onChange;
        private event Action? _onValueChanged;

        public LingoGodotColorPicker(LingoGfxColorPicker picker, Action<LingoColor>? onChange)
        {
            _onChange = onChange;
            picker.Init(this);
            Width = 20;
            Height = 20;
            ColorChanged += ColorChangedHandler;
        }

        private void ColorChangedHandler(Color color)
        {
            _onValueChanged?.Invoke();
            _onChange?.Invoke(color.ToLingoColor());
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set { Size = new Vector2(value, Size.Y); CustomMinimumSize = new Vector2(value, Size.Y); } } 
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value); CustomMinimumSize = new Vector2(Size.X, value); } }
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

        public new LingoColor Color
        {
            get => base.Color.ToLingoColor();
            set => base.Color = value.ToGodotColor();
        }

        event Action? ILingoFrameworkGfxNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        public object FrameworkNode => this;

        public new void Dispose()
        {
            ColorChanged -= ColorChangedHandler;
            QueueFree();
            base.Dispose();
        }
    }
}
