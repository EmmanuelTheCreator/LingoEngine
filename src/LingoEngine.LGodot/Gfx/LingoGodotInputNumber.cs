using Godot;
using LingoEngine.Gfx;
using System;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxInputNumber"/>.
    /// </summary>
    public partial class LingoGodotInputNumber : SpinBox, ILingoFrameworkGfxInputNumber, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        private LingoNumberType _numberType = LingoNumberType.Float;
        private event Action? _onValueChanged;

        public LingoGodotInputNumber(LingoGfxInputNumber input)
        {
            input.Init(this);
            this.ValueChanged += _ => _onValueChanged?.Invoke();
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get => Editable; set => Editable = value; }

        public new float Value { get => (float)base.Value; set => base.Value = value; }
        public float Min { get => (float)MinValue; set => MinValue = value; }
        public float Max { get => (float)MaxValue; set => MaxValue = value; }
        string ILingoFrameworkGfxNode.Name { get => Name; set => Name = value; }
        public LingoNumberType NumberType
        {
            get => _numberType;
            set
            {
                _numberType = value;
                Step = value == LingoNumberType.Integer ? 1 : 0.01f;
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
