using Godot;
using System;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components
{
    /// Godot implementation of <see cref="IAbstFrameworkInputSlider{TValue}"/>.
    /// </summary>
    public partial class AbstGodotInputSlider<TValue> : Control, IAbstFrameworkInputSlider<TValue>, System.IDisposable, IFrameworkFor<AbstInputSlider<TValue>>
        where TValue : struct, System.IConvertible
    {
        private readonly Slider _slider;
        private readonly System.Action<TValue>? _onChange;
        private AMargin _margin = AMargin.Zero;
        private event System.Action? _onValueChanged;
        private event System.Action? _onCommit;

        public AbstGodotInputSlider(AbstInputSlider<TValue> slider, AOrientation orientation, System.Action<TValue>? onChange)
        {
            _onChange = onChange;
            _slider = orientation == AOrientation.Horizontal ? new HSlider() : new VSlider();
            _slider.AnchorLeft = 0; _slider.AnchorRight = 1; _slider.AnchorTop = 0; _slider.AnchorBottom = 1;
            _slider.OffsetLeft = 0; _slider.OffsetRight = 0; _slider.OffsetTop = 0; _slider.OffsetBottom = 0;
            AddChild(_slider);
            slider.Init(this);
            _slider.ValueChanged += OnSliderValueChanged;
            CustomMinimumSize = new Vector2(2, 2);
            SizeFlagsHorizontal = 0;
            SizeFlagsVertical = 0;
        }

        private void OnSliderValueChanged(double v)
        {
            _onValueChanged?.Invoke();
            _onCommit?.Invoke();
            _onChange?.Invoke((TValue)Convert.ChangeType(v, typeof(TValue)));
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }

        public float Width
        {
            get => CustomMinimumSize.X;
            set { CustomMinimumSize = new Vector2(value, CustomMinimumSize.Y); }
        }
        public float Height
        {
            get => CustomMinimumSize.Y;
            set { CustomMinimumSize = new Vector2(CustomMinimumSize.X, value); }
        }

        public bool Visibility { get => Visible; set => Visible = value; }
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

        public bool Enabled { get => _slider.Editable; set => _slider.Editable = value; }
        public object FrameworkNode => this;

        event System.Action? IAbstFrameworkNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        event System.Action? IAbstFrameworkNodeInput.OnCommit
        {
            add => _onCommit += value;
            remove => _onCommit -= value;
        }

        public TValue Value { get => (TValue)Convert.ChangeType(_slider.Value, typeof(TValue)); set => _slider.Value = Convert.ToDouble(value); }
        public TValue MinValue { get => (TValue)Convert.ChangeType(_slider.MinValue, typeof(TValue)); set => _slider.MinValue = Convert.ToDouble(value); }
        public TValue MaxValue { get => (TValue)Convert.ChangeType(_slider.MaxValue, typeof(TValue)); set => _slider.MaxValue = Convert.ToDouble(value); }
        public TValue Step { get => (TValue)Convert.ChangeType(_slider.Step, typeof(TValue)); set => _slider.Step = Convert.ToDouble(value); }

        public new void Dispose()
        {
            _slider.ValueChanged -= OnSliderValueChanged;
            QueueFree();
        }
    }
}
