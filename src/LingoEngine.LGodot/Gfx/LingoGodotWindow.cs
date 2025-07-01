using Godot;
using LingoEngine.Gfx;
using LingoEngine.LGodot.Primitives;
using LingoEngine.LGodot.Styles;
using LingoEngine.Primitives;
using System;
using static Godot.Control;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxWindow"/>.
    /// </summary>
    public partial class LingoGodotWindow : Window, ILingoFrameworkGfxWindow, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        private readonly List<ILingoFrameworkGfxLayoutNode> _nodes = new();
        private readonly Panel _panel;
        private readonly StyleBoxFlat _panelStyle;
        private event Action? _onOpen;
        private event Action? _onClose;
        private event Action<float, float>? _onResize;
        private bool _isPopup;
        public LingoGodotWindow(LingoGfxWindow window, ILingoGodotStyleManager lingoGodotStyleManager)
        {
            window.Init(this);
            //Borderless = true;
            //ExtendToTitle = true;
            Theme = lingoGodotStyleManager.GetTheme(LingoGodotThemeElementType.PopupWindow);

            _panel = new Panel
            {
                Name = "WindowLayoutPanel",
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill,
            };
            _panelStyle = new StyleBoxFlat
            {
                BgColor = LingoColorList.Green.ToGodotColor(),

            };
            _panel.AddThemeStyleboxOverride("panel", _panelStyle);
            AddChild(_panel);
            CloseRequested += Hide;
        }

        public float X { get => Position.X; set => Position = new Vector2I((int)value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2I(Position.X, (int)value); }
        public float Width { get => Size.X; 
            set { 
                Size = new Vector2I((int)value, Size.Y);
                _panel.Size = Size;
            } 
        }
        public float Height
        {
            get => Size.Y; 
            set
            {
                Size = new Vector2I(Size.X, (int)value);
                _panel.Size = Size;
            }
        }
        public bool Visibility { get => Visible; set => Visible = value; }
        string ILingoFrameworkGfxNode.Name { get => Name; set => Name = value; }
        public new string Title { get => base.Title; set => base.Title = value; }
        public bool IsPopup
        {
            get => _isPopup; 
            set
            {
                _isPopup = value;
                Unresizable = value;
                Exclusive = value;
            }
        }
        public LingoColor BackgroundColor
        {
            get => _panelStyle.BgColor.ToLingoColor();
            set => _panelStyle.BgColor = value.ToGodotColor();
        }
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

        event Action? ILingoFrameworkGfxWindow.OnOpen
        {
            add => _onOpen += value;
            remove => _onOpen -= value;
        }
        event Action? ILingoFrameworkGfxWindow.OnClose
        {
            add => _onClose += value;
            remove => _onClose -= value;
        }
        event Action<float, float>? ILingoFrameworkGfxWindow.OnResize
        {
            add => _onResize += value;
            remove => _onResize -= value;
        }

        public void AddItem(ILingoFrameworkGfxLayoutNode child)
        {
            if (child.FrameworkNode is Node node)
                _panel.AddChild(node);
            _nodes.Add(child);
        }

        public void RemoveItem(ILingoFrameworkGfxLayoutNode child)
        {
            if (child.FrameworkNode is Node node)
                _panel.RemoveChild(node);
            _nodes.Remove(child);
        }

        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _nodes.ToArray();

        public void Popup()
        {
            base.Popup();
            _onOpen?.Invoke();
        }
        public void PopupCentered()
        {
            base.PopupCentered();
            _onOpen?.Invoke();
        }
        public new void Hide()
        {
            base.Hide();
            _onClose?.Invoke();
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == 1008)// NotificationResized
            {
                _panel.Size = Size;
                _onResize?.Invoke(Size.X, Size.Y);
            }
        }
        public new void Dispose()
        {
            CloseRequested -= Hide;
            QueueFree();
            base.Dispose();
        }

        
    }
}
