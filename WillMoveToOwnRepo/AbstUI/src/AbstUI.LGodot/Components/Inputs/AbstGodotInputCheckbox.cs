using Godot;
using System;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkInputCheckbox"/>.
    /// </summary>
    public partial class AbstGodotInputCheckbox : CheckBox, IAbstFrameworkInputCheckbox, IDisposable, IFrameworkFor<AbstInputCheckbox>
    {
        private AMargin _margin = AMargin.Zero;
        private Action<bool>? _onChange;

        private event Action? _onValueChanged;
        private event Action? _onCommit;
        public AbstGodotInputCheckbox(AbstInputCheckbox input, Action<bool>? onChange)
        {
            _onChange = onChange;
            input.Init(this);
            Toggled += OnToggled;
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        public bool Enabled { get => !Disabled; set => Disabled = !value; }

        public bool Checked { get => ButtonPressed; set => ButtonPressed = value; }

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

        string IAbstFrameworkNode.Name { get => Name; set => Name = value; }

        public object FrameworkNode => this;


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

        public new void Dispose()
        {
            Toggled -= OnToggled;
            QueueFree();
            base.Dispose();
        }

        private void OnToggled(bool pressed)
        {
            _onValueChanged?.Invoke();
            _onCommit?.Invoke();
            _onChange?.Invoke(pressed);
        }
    }
}
