using Godot;

namespace LingoEngine.Director.LGodot.Gfx
{
    public partial class DirectorToolButton : PanelContainer
    {
        private TextureButton _button;
        private bool _isSelected;
        private Action _pressed = () => { };
        public Color BGColor { get; set;  } = new Color(100, 100, 100);
        public Texture2D Icon
        {
            get => _button.TextureNormal;
            set => _button.TextureNormal = value;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                UpdateBackground();
                QueueRedraw();
            }
        }

        public Action Pressed
        {
            get => _pressed;
            set => _pressed = value ?? throw new ArgumentNullException(nameof(value), "Pressed action cannot be null.");
        }
        public DirectorToolButton()
        {
            CustomMinimumSize = new Vector2(22, 22); // padding space
            AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = BGColor });

            _button = new TextureButton
            {
                CustomMinimumSize = new Vector2(16, 16),
                StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
                SizeFlagsVertical = SizeFlags.ShrinkCenter
            };
            _button.Pressed += () => _pressed();
            AddChild(_button);
        }

        private void UpdateBackground()
        {
            var bg = new StyleBoxFlat
            {
                BgColor = IsSelected ? Colors.White : BGColor,
                CornerRadiusTopLeft = 2,
                CornerRadiusTopRight = 2,
                CornerRadiusBottomLeft = 2,
                CornerRadiusBottomRight = 2
            };
            AddThemeStyleboxOverride("panel", bg);
        }
    }
}
