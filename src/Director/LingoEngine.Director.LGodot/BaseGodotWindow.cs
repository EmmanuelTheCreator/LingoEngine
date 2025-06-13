using Godot;

namespace LingoEngine.Director.LGodot
{
    public  abstract partial class BaseGodotWindow : Control
    {
        private bool _dragging;
        private readonly Label _label = new Label();

        public BaseGodotWindow(string name)
        {
            AddChild(_label);
            _label.Position = new Vector2(30, 3);
            _label.LabelSettings = new LabelSettings();
            _label.LabelSettings.FontSize = 12;
            _label.LabelSettings.FontColor = new Color(1, 0.3f, 0.2f);
            _label.Text = name;
        }

        public override void _Draw()
        {
            DrawRect(new Rect2(0, 0, Size.X, 20), new Color(0.3f, 0.3f, 0.2f));
            DrawLine(new Vector2(0, 20), new Vector2(Size.X, 20), Colors.Black);
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
            {
                Vector2 pos = GetLocalMousePosition();
                if (pos.Y < 20)
                {
                    if (mb.Pressed)
                        _dragging = true;
                    else
                        _dragging = false;
                }
            }
            else if (@event is InputEventMouseMotion motion && _dragging)
                Position += motion.Relative;
        }
    }
}
