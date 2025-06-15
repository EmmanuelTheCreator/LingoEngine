using Godot;

namespace LingoEngine.Director.LGodot
{
    public  abstract partial class BaseGodotWindow : Control
    {
        private bool _dragging;
        private bool _resizing;
        private readonly Label _label = new Label();
        private readonly Button _closeButton = new Button();
        protected const int TitleBarHeight = 20;
        private const int ResizeHandle = 10;

        public BaseGodotWindow(string name)
        {
            AddChild(_label);
            _label.Position = new Vector2(30, 3);
            _label.LabelSettings = new LabelSettings();
            _label.LabelSettings.FontSize = 12;
            _label.LabelSettings.FontColor = new Color(1, 0.3f, 0.2f);
            _label.Text = name;

            AddChild(_closeButton);
            _closeButton.Text = "X";
            _closeButton.CustomMinimumSize = new Vector2(20, 16);
            _closeButton.Pressed += () => Visible = false;
        }

        public override void _Draw()
        {
            DrawRect(new Rect2(0, 0, Size.X, TitleBarHeight), new Color("#d2e0ed"));
            DrawLine(new Vector2(0, TitleBarHeight), new Vector2(Size.X, TitleBarHeight), Colors.Black);
            _closeButton.Position = new Vector2(Size.X - 22, 2);
            // draw resize handle
            DrawLine(new Vector2(Size.X - ResizeHandle, Size.Y), new Vector2(Size.X, Size.Y - ResizeHandle), Colors.DarkGray);
            DrawLine(new Vector2(Size.X - ResizeHandle/2f, Size.Y), new Vector2(Size.X, Size.Y - ResizeHandle/2f), Colors.DarkGray);
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
            {
                Vector2 pos = GetLocalMousePosition();
                if (pos.Y < TitleBarHeight)
                {
                    _dragging = mb.Pressed;
                    _resizing = false;
                }
                else if (pos.X >= Size.X - ResizeHandle && pos.Y >= Size.Y - ResizeHandle)
                {
                    _resizing = mb.Pressed;
                    _dragging = false;
                }
            }
            else if (@event is InputEventMouseMotion motion)
            {
                if (_dragging)
                    Position += motion.Relative;
                else if (_resizing)
                {
                    Size += new Vector2(motion.Relative.X, motion.Relative.Y);
                    CustomMinimumSize = Size;
                    QueueRedraw();
                }
            }
        }
    }
}
