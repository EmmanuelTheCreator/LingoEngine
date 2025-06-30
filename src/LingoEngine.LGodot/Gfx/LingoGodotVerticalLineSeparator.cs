using Godot;

namespace LingoEngine.LGodot.Gfx
{
    public partial class LingoGodotVerticalLineSeparator : Control
    {
        public override void _Ready()
        {
            CustomMinimumSize = new Vector2(2, 0); // or (2, 0) for vertical
        }

        public override void _Draw()
        {
            DrawLine(new Vector2(0, 0), new Vector2(0, Size.Y), new Color(1, 1, 1), 1); // left light
            DrawLine(new Vector2(1, 0), new Vector2(1, Size.Y), new Color(0.4f, 0.4f, 0.4f), 1); // right dark

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
