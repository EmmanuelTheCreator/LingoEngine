using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxHorizontalLineSeparator"/>.
    /// </summary>
    public partial class LingoGodotHorizontalLineSeparator : Control, ILingoFrameworkGfxHorizontalLineSeparator, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;

        public LingoGodotHorizontalLineSeparator(LingoGfxHorizontalLineSeparator separator)
        {
            separator.Init(this);
        }

        public override void _Ready()
        {
            CustomMinimumSize = new Vector2(0, 2); // 2px tall
        }

        public override void _Draw()
        {
            // Top: light gray
            DrawLine(new Vector2(0, 0), new Vector2(Size.X, 0), new Color(1, 1, 1), 1);
            // Bottom: dark gray
            DrawLine(new Vector2(0, 1), new Vector2(Size.X, 1), new Color(0.4f, 0.4f, 0.4f), 1);
        }

        public override void _Notification(int what)
        {
            if (what == NotificationResized)
            {
                QueueRedraw();
            }
        }

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

        public object FrameworkNode => this;

        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }
    }
}
