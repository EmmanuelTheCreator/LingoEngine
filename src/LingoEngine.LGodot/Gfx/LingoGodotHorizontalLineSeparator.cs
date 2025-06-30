using Godot;

namespace LingoEngine.LGodot.Gfx
{
    public partial class LingoGodotHorizontalLineSeparator : Control
    {
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
    }
}
