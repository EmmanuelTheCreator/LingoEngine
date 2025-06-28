using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using System;

namespace LingoEngine.LGodot.Gfx
{
    public partial class LingoGodotSpinBox : SpinBox, ILingoFrameworkGfxSpinBox, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        public LingoGodotSpinBox(LingoGfxSpinBox spin)
        {
            spin.Init(this);
            this.ValueChanged += _ => _onValueChanged?.Invoke();
        }
        private event Action? _onValueChanged;

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
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

        public bool Enabled { get => Editable; set => Editable = value; }
        public new float Value { get => (float)base.Value; set => base.Value = value; }
        public float Min { get => (float)MinValue; set => MinValue = value; }
        public float Max { get => (float)MaxValue; set => MaxValue = value; }

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
