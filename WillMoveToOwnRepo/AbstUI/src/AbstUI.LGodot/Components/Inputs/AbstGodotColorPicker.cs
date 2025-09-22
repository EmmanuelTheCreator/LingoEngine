using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using System;
using AbstUI.LGodot.Primitives;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkColorPicker"/>.
    /// </summary>
    public partial class AbstGodotColorPicker : ColorPickerButton, IAbstFrameworkColorPicker, IDisposable, IFrameworkFor<AbstColorPicker>
    {
        private AMargin _margin = AMargin.Zero;
        private readonly Action<AColor>? _onChange;
        private event Action? _onValueChanged;
        private event Action? _onCommit;

        public AbstGodotColorPicker(AbstColorPicker picker, Action<AColor>? onChange)
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
            _onCommit?.Invoke();
            _onChange?.Invoke(color.ToAbstColor());
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set { Size = new Vector2(value, Size.Y); CustomMinimumSize = new Vector2(value, Size.Y); } }
        public float Height { get => Size.Y; set { Size = new Vector2(Size.X, value); CustomMinimumSize = new Vector2(Size.X, value); } }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get => !Disabled; set => Disabled = !value; }
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

        public new AColor Color
        {
            get => base.Color.ToAbstColor();
            set => base.Color = value.ToGodotColor();
        }

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

        public object FrameworkNode => this;

        public new void Dispose()
        {
            ColorChanged -= ColorChangedHandler;
            QueueFree();
            base.Dispose();
        }
    }
}
