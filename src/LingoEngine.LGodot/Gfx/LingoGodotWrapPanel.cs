using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxWrapPanel"/>.
    /// </summary>
    public partial class LingoGodotWrapPanel : Control, ILingoFrameworkGfxWrapPanel, IDisposable
    {
        private FlowContainer _container;
        private LingoOrientation _orientation;
        private LingoMargin _itemMargin;
        private LingoMargin _margin;

        public LingoGodotWrapPanel(LingoGfxWrapPanel panel, LingoOrientation orientation)
        {
            _orientation = orientation;
            _itemMargin = LingoMargin.Zero;
            _margin = LingoMargin.Zero;
            _container = CreateContainer(orientation);
            AddChild(_container);
            panel.Init(this);
        }

        private FlowContainer CreateContainer(LingoOrientation orientation)
        {
            FlowContainer container = orientation == LingoOrientation.Horizontal ? new HFlowContainer() : new VFlowContainer();
            container.SizeFlagsVertical = SizeFlags.ExpandFill;
            container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            return container;
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }

        public LingoOrientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation == value)
                    return;
                var children = _container.GetChildren().OfType<Node>().ToArray();
                foreach (var c in children)
                    _container.RemoveChild(c);
                RemoveChild(_container);
                _container.QueueFree();
                _orientation = value;
                _container = CreateContainer(value);
                AddChild(_container);
                ApplyMargin();
                foreach (var c in children)
                {
                    _container.AddChild(c);
                    if (c is Control ctrl)
                        ApplyItemMargin(ctrl);
                }
            }
        }

        public LingoMargin ItemMargin
        {
            get => _itemMargin;
            set
            {
                _itemMargin = value;
                foreach (var child in _container.GetChildren().OfType<Control>())
                    ApplyItemMargin(child);
            }
        }

        public LingoMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                ApplyMargin();
            }
        }

        public void AddChild(ILingoFrameworkGfxNode child)
        {
            if (child is Node node)
            {
                if (node is Control ctrl)
                    ApplyItemMargin(ctrl);
                _container.AddChild(node);
            }
        }

        public new void Dispose()
        {
            base.Dispose();
            QueueFree();
        }

        private void ApplyItemMargin(Control ctrl)
        {
            ctrl.AddThemeConstantOverride("margin_left", (int)_itemMargin.Left);
            ctrl.AddThemeConstantOverride("margin_right", (int)_itemMargin.Right);
            ctrl.AddThemeConstantOverride("margin_top", (int)_itemMargin.Top);
            ctrl.AddThemeConstantOverride("margin_bottom", (int)_itemMargin.Bottom);
        }

        private void ApplyMargin()
        {
            _container.AddThemeConstantOverride("margin_left", (int)_margin.Left);
            _container.AddThemeConstantOverride("margin_right", (int)_margin.Right);
            _container.AddThemeConstantOverride("margin_top", (int)_margin.Top);
            _container.AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
        }
    }
}
