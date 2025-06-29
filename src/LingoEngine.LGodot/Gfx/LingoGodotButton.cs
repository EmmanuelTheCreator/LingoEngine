using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxButton"/>.
    /// </summary>
    public partial class LingoGodotButton : Button, ILingoFrameworkGfxButton, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        private event Action? _pressed;

        public object FrameworkNode => this;

        public LingoGodotButton(LingoGfxButton button)
        {
            button.Init(this);
            Pressed += () => _pressed?.Invoke();
        }

        //public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        //public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
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

        public new string Text { get => base.Text; set => base.Text = value; }
        public bool Enabled { get => !Disabled; set => Disabled = !value; }
        event Action? ILingoFrameworkGfxButton.Pressed
        {
            add => _pressed += value;
            remove => _pressed -= value;
        }

        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }

        
        
    }
}
