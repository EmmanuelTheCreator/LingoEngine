using Godot;
using AbstUI.Primitives;
using AbstUI.LGodot.Primitives;
using AbstUI.Inputs;
using Microsoft.Extensions.DependencyInjection;
using AbstUI.Windowing;
using AbstUI.Styles;
using AbstUI.Commands;
using AbstUI.LGodot.Inputs;
using AbstUI.Components;

namespace AbstEngine.Director.LGodot
{
    public abstract partial class BaseGodotWindow : Panel, IAbstFrameworkWindow
    {

        private readonly IAbstWindowManager _windowManager;
        private readonly IHistoryManager? _historyManager;
        //protected readonly AbstUIGodotMouse _MouseFrameworkObj;
        protected bool _dragging;
        protected bool _resizing;
        private bool _useGuiInput = false;
        private float _wantedWidth;
        private float _wantedHeight = 10;

        private readonly Label _label = new Label
        {
            Name = "WindowTextTitle",
        };

        private readonly Button _closeButton = new Button
        {
            Name = "WindowCloseButton",
        };
        protected IAbstWindowInternal _window = null!;
        protected int TitleBarHeight = 20;
        private int _resizeHandle = 10;
        private Vector2 _dragOffset;
        private Vector2 _resizeStartSize;
        private Vector2 _resizeStartMousePos;
        private IAbstGodotMouseHandler _mouseFrameworkObj = null!;


        #region Properties

        protected StyleBoxFlat Style = new StyleBoxFlat();
        public string WindowCode => _window.WindowCode;
        public string WindowName { get; private set; }
        public string Title { get => _label.Text; set => _label.Text = value; }
        public bool IsOpen => Visible;
        public bool IsActiveWindow => _windowManager.ActiveWindow == _window;
        public AColor _backgroundColor;
        public AColor BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                Style.BgColor = value.ToGodotColor();

                AddThemeStyleboxOverride("panel", Style);
            }
        }
        public AColor BackgroundTitleColor { get; set; }


        public IAbstMouse Mouse => _window.Mouse;
        public IAbstMouse<AbstMouseEvent> MouseT => (IAbstMouse<AbstMouseEvent>)_window.Mouse;
        public IAbstKey AbstKey => _window.Key;

        private IAbstFrameworkNode? _content;
        public IAbstFrameworkNode? Content
        {
            get => _content;
            set
            {
                if (_content == value) return;
                if (_content?.FrameworkNode is Node oldNode)
                    RemoveChild(oldNode);
                _content = value;
                if (_content?.FrameworkNode is Node newNode)
                {
                    AddChild(newNode);
                    if (_content?.FrameworkNode is Node2D newNode2D)
                        newNode2D.Position = new Vector2(0, TitleBarHeight);
                    else if (_content?.FrameworkNode is Control control)
                        control.Position = new Vector2(0, TitleBarHeight);
                }
                
                _window.SetContentFromFW(value);
            }
        }

        string IAbstFrameworkNode.Name { get => Name; set => Name = value; }
        public bool Visibility { get => Visible; set => Visible = value; }
        public float Width
        {
            get => Size.X;
            set
            {
                _wantedWidth = value;
                CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
                Size = new Vector2(value, _wantedHeight);

                var test = Size;

            }
        }

        public float Height
        {
            get => Size.Y;
            set
            {
                _wantedHeight = Height;
                CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
                Size = new Vector2(_wantedWidth, value);
            }
        }
        public AMargin Margin { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public object FrameworkNode => throw new NotImplementedException();

        #endregion


        public BaseGodotWindow(string name, IServiceProvider serviceProvider)
        {
            // IAbstGodotWindowManager windowManager, IHistoryManager? historyManager = null
            Name = $"Window {name}";
            WindowName = name;
            _windowManager = serviceProvider.GetRequiredService<IAbstWindowManager>();
            _historyManager = serviceProvider.GetRequiredService<IHistoryManager>();

            //MouseFilter = MouseFilterEnum.Stop;
            FocusMode = FocusModeEnum.All;
            AddChild(_label);
            _label.Position = new Vector2(5, 1);
            _label.LabelSettings = new LabelSettings();
            _label.LabelSettings.FontSize = 12;
            _label.LabelSettings.FontColor = Colors.Black;
            _label.Text = name;

            Style.BorderColor = AbstDefaultColors.Window_Border.ToGodotColor();
            Style.BorderWidthLeft = 1;
            Style.BorderWidthRight = 1;
            Style.BorderWidthBottom = 1;
            BackgroundColor = AbstDefaultColors.BG_WhiteMenus;

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

        }

        public virtual void Init(IAbstWindow instance)
        {
            _window = (IAbstWindowInternal)instance;
            instance.Init(this);
            instance.WindowTitleHeight = TitleBarHeight;
            if (instance.Width > 0 && instance.Height > 0)
            {
                Size = new Vector2(instance.Width, instance.Height);
                CustomMinimumSize = Size;
            }
            if (instance.X > 0 && instance.Y > 0)
                Position = new Vector2(instance.X, instance.Y);
            _mouseFrameworkObj = ((IAbstMouse<AbstMouseEvent>)instance.Mouse).Framework<IAbstGodotMouseHandler>();
            OnInit(instance);
            
        }
        public virtual void OnInit(IAbstWindow instance) { }

        public override void _Draw()
        {
            var titleColor = IsActiveWindow ? AbstDefaultColors.Window_Title_BG_Active : AbstDefaultColors.Window_Title_BG_Inactive;
            DrawRect(new Rect2(0, 0, Size.X, TitleBarHeight), titleColor.ToGodotColor());
            DrawLine(new Vector2(0, TitleBarHeight), new Vector2(Size.X, TitleBarHeight), AbstDefaultColors.Window_Title_Line_Under.ToGodotColor());
            _closeButton.Position = new Vector2(Size.X - 18, 1);
            // draw resize handle
            DrawLine(new Vector2(Size.X - _resizeHandle, Size.Y), new Vector2(Size.X, Size.Y - _resizeHandle), Colors.DarkGray);
            DrawLine(new Vector2(Size.X - _resizeHandle / 2f, Size.Y), new Vector2(Size.X, Size.Y - _resizeHandle / 2f), Colors.DarkGray);
        }



        protected void DontUseInputInsteadOfGuiInput()
        {
            // todo : fix this
            _useGuiInput = false;
        }
        public new APoint GetPosition() => Position.ToAbstPoint();

        public new APoint GetSize() => Size.ToAbstPoint();
        public override void _Input(InputEvent @event)
        {
            base._Input(@event);
            if (_useGuiInput || !Visible || !(@event is InputEventFromWindow)) return;
            //if (!_dragging && !_resizing && !GetGlobalRect().HasPoint(GetGlobalMousePosition()))
            //    return;
            OnHandleTheEvent(@event);
        }
        //public override void _GuiInput(InputEvent @event)
        //{
        //    base._GuiInput(@event);
        //    if (!useGuiInput || !Visible) return;
        //    OnHandleTheEvent(@event);
        //}
        protected virtual void OnHandleTheEvent(InputEvent @event)
        {
            if (_window.IsActivated)
            {
                if (@event is InputEventKey key && key.Pressed && _historyManager != null)
                {
                    if (key.Keycode == Key.Z && key.CtrlPressed)
                    {
                        _historyManager.Undo();
                        return;
                    }
                    if (key.Keycode == Key.Y && key.CtrlPressed)
                    {
                        _historyManager.Redo();
                        return;
                    }
                }
            }

            var isInsideRect = GetGlobalRect().HasPoint(GetGlobalMousePosition());
            var mousePos = GetLocalMousePosition();
            // Handle mouse button events (MouseDown and MouseUp)
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                var code = this.WindowCode;
                if (!IsActiveWindow && isInsideRect)
                {
                    ActivateMe();
                    //_window.SetActivated(true);
                    //_windowManager.SetActiveWindow(WindowCode); //.SetActiveWindow(this, GetGlobalMousePosition());
                }
                if (!IsActiveWindow)
                    return;

                // _MouseFrameworkObj.HandleMouseButtonEvent(mouseButtonEvent, isInsideRect, mousePos.X, mousePos.Y - TitleBarHeight);
                //Console.WriteLine(Name + ":" + mousePos.X + "x" + mousePos.Y+":"+ isInsideRect);
            }
            // Handle Mouse Motion (MouseMove)
            //else if (@event is InputEventMouseMotion mouseMotionEvent)
            //    _MouseFrameworkObj.HandleMouseMoveEvent(mouseMotionEvent, isInsideRect, mousePos.X, mousePos.Y - TitleBarHeight);
            if (!_dragging && !_resizing)
            {
                if (!isInsideRect)
                    return;
            }

            if (@event is InputEventMouseButton mb)
            {

                var pressed = mb.Pressed;

                if (mb.ButtonIndex == MouseButton.Left)
                {
                    if (pressed && isInsideRect)
                    {
                        ActivateMe();
                        //_window.SetActivated(true);
                        //_windowManager.SetActiveWindow(WindowCode);  //_windowManager.SetActiveWindow(this, GetGlobalMousePosition());
                    }
                    Vector2 pos = GetLocalMousePosition();

                    if (pressed)
                    {
                        if (pos.Y < TitleBarHeight)
                        {
                            _dragging = true;
                            _resizing = false;
                            _dragOffset = pos;
                        }
                        else if (pos.X >= Size.X - _resizeHandle && pos.Y >= Size.Y - _resizeHandle)
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
                    var pos = globalMousePos - _dragOffset;
                    SetThePosition(pos);
                }
                else if (_resizing)
                {
                    var delta = GetGlobalMousePosition() - _resizeStartMousePos;
                    var newSize = _resizeStartSize + delta;

                    // Optional: clamp minimum size
                    newSize.X = Mathf.Max(newSize.X, _window.MinimumWidth);
                    newSize.Y = Mathf.Max(newSize.Y, _window.MinimumHeight);

                    Size = newSize;
                    OnResizing(false, newSize);
                    CustomMinimumSize = newSize;
                    QueueRedraw();
                }
            }
        }
        public void ActivateMe()
        {
            if (_windowManager.ActiveWindow == _window) return;
            var mousePoint = GetGlobalMousePosition();
            if (_windowManager.ActiveWindow != null)
            {
                var godottCurrent = (BaseGodotWindow)_windowManager.ActiveWindow.FrameworkObj;
                if (godottCurrent.GetGlobalRect().HasPoint(mousePoint))
                {
                    // if the active window is clicked, we do not change the active window
                    // this is to prevent flickering when clicking on the active window
                    return;
                }
            }

            _window.SetActivated(true);
            _windowManager.SetActiveWindow(WindowCode);  //_windowManager.SetActiveWindow(this, GetGlobalMousePosition());
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
                    SetThePosition(globalMousePos - _dragOffset);
                }
                else if (_resizing)
                {
                    var mouseDelta = GetGlobalMousePosition() - _resizeStartMousePos;
                    var newSize = _resizeStartSize + mouseDelta;

                    // Clamp minimum size
                    newSize.X = Mathf.Max(newSize.X, _window.MinimumWidth);
                    newSize.Y = Mathf.Max(newSize.Y, _window.MinimumHeight);

                    Size = newSize;
                    OnResizing(false, Size);
                    CustomMinimumSize = newSize;
                    QueueRedraw();
                }
            }
        }
        protected virtual void OnResizing(bool firstResize,Vector2 size)
        {
            var contentSize = new Vector2I((int)size.X,(int) size.Y - TitleBarHeight);
            _window?.ResizingContentFromFW(firstResize, contentSize.X, contentSize.Y);
        }


        public virtual void OpenWindow()
        {
            Visible = true;
            EnsureInBounds();
            OnResizing(true, Size);
        }
        public virtual void CloseWindow() => Visible = false;
        public virtual void MoveWindow(int x, int y) => Position = new Vector2(x, y);
        public virtual void SetPositionAndSize(int x, int y, int width, int height)
        {
            SetThePosition(new Vector2(x, y));
            var size = new Vector2(width, height); // not title bar height, because this is loaded from saved settings
            Size = size;
            CustomMinimumSize = Size;
        }
        public void SetThePosition(Vector2 pos)
        {
            Position = pos;
            _window?.SetPositionFromFW((int)Position.X, (int)Position.Y);
        }

        public virtual void SetSize(int width, int height)
        {
            var size = new Vector2(width, height + TitleBarHeight);
            Size = size;
            CustomMinimumSize = Size;
        }
        public void EnsureInBounds()
        {
            var viewPort = GetViewport();
            var viewportRect = viewPort.GetVisibleRect();
            Vector2 pos = Position;
            Vector2 size = Size;

            if (pos.X < viewportRect.Position.X)
                pos.X = viewportRect.Position.X + 2;
            if (pos.Y < viewportRect.Position.Y)
                pos.Y = viewportRect.Position.Y + 2;

            if (pos.X + size.X > viewportRect.Position.X + viewportRect.Size.X)
                pos.X = viewportRect.Position.X + viewportRect.Size.X - size.X - 2;
            if (pos.Y + size.Y > viewportRect.Position.Y + viewportRect.Size.Y)
                pos.Y = viewportRect.Position.Y + viewportRect.Size.Y - size.Y - 2;

            SetThePosition(pos);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

    }
}
