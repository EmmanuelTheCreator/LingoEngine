using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Components.Inputs;
using AbstUI.LGodot.Primitives;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components
{
    public partial class AbstGodotSpinBox : SpinBox, IAbstFrameworkSpinBox, IHasTextBackgroundBorderColor, IDisposable, IFrameworkFor<AbstInputSpinBox>
    {
        private AMargin _margin = AMargin.Zero;
        private Action<float>? _onChange;
        private AColor _textColor = AbstDefaultColors.InputTextColor;
        private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
        private AColor _borderColor = AbstDefaultColors.InputBorderColor;
        private event Action? _onCommit;

        public AbstGodotSpinBox(AbstInputSpinBox spin, IAbstFontManager blingoFontManager, Action<float>? onChange)
        {
            _onChange = onChange;
            spin.Init(this);
            ValueChanged += OnValueChanged;
        }
        private event Action? _onValueChanged;

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width
        {
            get => Size.X;
            set
            {
                Size = new Vector2(value, Size.Y);
                CustomMinimumSize = new Vector2(value, CustomMinimumSize.Y);
            }
        }
        public float Height
        {
            get => Size.Y;
            set
            {
                Size = new Vector2(Size.X, value);
                CustomMinimumSize = new Vector2(CustomMinimumSize.X, value);
            }
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

        public bool Enabled { get => Editable; set => Editable = value; }
        public new float Value { get => (float)base.Value; set => base.Value = value; }
        public float Min { get => (float)MinValue; set => MinValue = value; }
        public float Max { get => (float)MaxValue; set => MaxValue = value; }

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

        public AColor TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                AddThemeColorOverride("font_color", _textColor.ToGodotColor());
            }
        }

        public AColor BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                AddThemeColorOverride("background_color", _backgroundColor.ToGodotColor());
            }
        }

        public AColor BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                AddThemeColorOverride("border_color", _borderColor.ToGodotColor());
            }
        }

        public new void Dispose()
        {
            ValueChanged -= OnValueChanged;
            QueueFree();
            base.Dispose();
        }

        private void OnValueChanged(double _)
        {
            _onValueChanged?.Invoke();
            _onCommit?.Invoke();
            _onChange?.Invoke(Value);
        }
    }
}

