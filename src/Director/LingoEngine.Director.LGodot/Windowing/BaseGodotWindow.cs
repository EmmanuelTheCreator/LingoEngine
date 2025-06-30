using Godot;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Primitives;

namespace LingoEngine.Director.LGodot
{
    public abstract partial class BaseGodotWindow : Panel
    {
        private readonly IDirGodotWindowManager _windowManager;
        protected bool _dragging;
        protected bool _resizing;
        private readonly Label _label = new Label
        {
            Name = "WindowTextTitle",
        };

        private readonly Button _closeButton = new Button
        {
            Name = "WindowCloseButton",
        };
        protected int TitleBarHeight = 20;
        private int ResizeHandle = 10;
        private Vector2 _dragOffset;
        private Vector2 _resizeStartSize;
        private Vector2 _resizeStartMousePos;

        protected StyleBoxFlat Style = new StyleBoxFlat();
        public string WindowCode { get; private set; }
        public string WindowName { get; private set; }

        public BaseGodotWindow(string windowCode,string name, IDirGodotWindowManager windowManager)
        {
            Name = $"Window {name}";
            WindowName = name;
            WindowCode = windowCode;
            _windowManager = windowManager;
            MouseFilter = MouseFilterEnum.Stop;
            FocusMode = FocusModeEnum.All;
            AddChild(_label);
            _label.Position = new Vector2(5, 1);
            _label.LabelSettings = new LabelSettings();
            _label.LabelSettings.FontSize = 12;
            _label.LabelSettings.FontColor = Colors.Black;
            _label.Text = name;
            BackgroundColor = DirectorColors.BG_WhiteMenus;
            AddChild(_closeButton);
            _closeButton.Text = "X";
            _closeButton.CustomMinimumSize = new Vector2(16, 16);
            _closeButton.Pressed += () => Visible = false;
            _closeButton.ThemeTypeVariation = "CloseButton";
            //AddThemeStyleboxOverride("panel", Style);
            //_closeButton.AddThemeStyleboxOverride().Theme.GetFontList()

            // Draw background
            //DrawRect(GetRect(), new Color(0xff, 0xff, 0xff), true);
            //var styleBox = new StyleBoxFlat
            //{
            //    BgColor = new Color(1, 1, 1, 1.0f) // RGBA
            //};
            //AddThemeStyleboxOverride("panel", styleBox);
            windowManager.Register(this);
        }
        public LingoColor BackgroundColor {
            set
            {
                Style.BgColor = value.ToGodotColor();
                AddThemeStyleboxOverride("panel", Style);
            }
        }

        public override void _Draw()
        {
            DrawRect(new Rect2(0, 0, Size.X, TitleBarHeight), DirectorColors.Window_Title_BG.ToGodotColor());
            DrawLine(new Vector2(0, TitleBarHeight), new Vector2(Size.X, TitleBarHeight), DirectorColors.Window_Title_Line_Under.ToGodotColor());
            _closeButton.Position = new Vector2(Size.X - 18, 1);
            // draw resize handle
            DrawLine(new Vector2(Size.X - ResizeHandle, Size.Y), new Vector2(Size.X, Size.Y - ResizeHandle), Colors.DarkGray);
            DrawLine(new Vector2(Size.X - ResizeHandle/2f, Size.Y), new Vector2(Size.X, Size.Y - ResizeHandle/2f), Colors.DarkGray);
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb)
            {
                var pressed = mb.Pressed;
                if (pressed)
                    _windowManager.SetActiveWindow(this);
                else
                {

                }

                if (mb.ButtonIndex == MouseButton.Left)
                {
                    Vector2 pos = GetLocalMousePosition();

                    if (pressed)
                    {
                        if (pos.Y < TitleBarHeight)
                        {
                            _dragging = true;
                            _resizing = false;
                            _dragOffset = pos;
                        }
                        else if (pos.X >= Size.X - ResizeHandle && pos.Y >= Size.Y - ResizeHandle)
                        {
                            _resizing = true;
                            _dragging = false;
                            _resizeStartMousePos = GetGlobalMousePosition();
                            _resizeStartSize = Size;
                        }
                    }
                    else // Mouse button released
                    {
                        _dragging = false;
                        _resizing = false;
                    }
                }
            }
            else if (@event is InputEventMouseMotion)
            {
                if (_dragging)
                {
                    var globalMousePos = GetGlobalMousePosition();
                    Position = globalMousePos - _dragOffset;
                }
                else if (_resizing)
                {
                    var delta = GetGlobalMousePosition() - _resizeStartMousePos;
                    var newSize = _resizeStartSize + delta;

                    // Optional: clamp minimum size
                    newSize.X = Mathf.Max(newSize.X, 100);
                    newSize.Y = Mathf.Max(newSize.Y, 100);

                    Size = newSize;
                    OnResizing(Size);
                    CustomMinimumSize = newSize;
                    QueueRedraw();
                }
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb)
            {
                if (mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
                {
                    _dragging = false;
                    _resizing = false;
                }
            }
            else if (@event is InputEventMouseMotion)
            {
                if (_dragging)
                {
                    var globalMousePos = GetGlobalMousePosition();
                    Position = globalMousePos - _dragOffset;
                }
                else if (_resizing)
                {
                    var mouseDelta = GetGlobalMousePosition() - _resizeStartMousePos;
                    var newSize = _resizeStartSize + mouseDelta;

                    // Clamp minimum size
                    newSize.X = Mathf.Max(newSize.X, 100);
                    newSize.Y = Mathf.Max(newSize.Y, 100);

                    Size = newSize;
                    OnResizing(Size);
                    CustomMinimumSize = newSize;
                    QueueRedraw();
                }
            }
        }
        protected virtual void OnResizing(Vector2 size)
        {

        }

        public bool IsOpen => Visible;
        public bool IsActiveWindow => _windowManager.ActiveWindow == this;

        

        public virtual void OpenWindow()
        {
            Visible = true;
            EnsureInBounds();
        }
        public virtual void CloseWindow() => Visible = false;
        public virtual void MoveWindow(int x, int y) => Position = new Vector2(x, y);

        private void EnsureInBounds()
        {
            var viewportRect = GetViewport().GetVisibleRect();
            Vector2 pos = Position;
            Vector2 size = Size;

            if (pos.X < viewportRect.Position.X)
                pos.X = viewportRect.Position.X;
            if (pos.Y < viewportRect.Position.Y)
                pos.Y = viewportRect.Position.Y;

            if (pos.X + size.X > viewportRect.Position.X + viewportRect.Size.X)
                pos.X = viewportRect.Position.X + viewportRect.Size.X - size.X;
            if (pos.Y + size.Y > viewportRect.Position.Y + viewportRect.Size.Y)
                pos.Y = viewportRect.Position.Y + viewportRect.Size.Y - size.Y;

            Position = pos;
        }
    }
}
