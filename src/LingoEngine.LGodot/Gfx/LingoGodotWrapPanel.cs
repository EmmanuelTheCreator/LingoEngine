using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using static Godot.Control;

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
        private MarginContainer _marginContainer;

        public LingoGodotWrapPanel(LingoGfxWrapPanel panel, LingoOrientation orientation)
        {
            _orientation = orientation;
            _itemMargin = LingoMargin.Zero;
            _margin = LingoMargin.Zero;
            _marginContainer = new();
            AddChild(_marginContainer);
            _container = CreateContainer(orientation);
            _marginContainer.AddChild(_container);
            panel.Init(this);
        }

        private FlowContainer CreateContainer(LingoOrientation orientation)
        {
            FlowContainer container = orientation == LingoOrientation.Horizontal ? new VFlowContainer() : new HFlowContainer();
            //container.SizeFlagsVertical = SizeFlags.ExpandFill;
            //container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            container.SizeFlagsHorizontal = SizeFlags.Expand;
            container.SizeFlagsVertical = SizeFlags.Expand;

            return container;
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => CustomMinimumSize.X; set => CustomMinimumSize = new Vector2(value, CustomMinimumSize.Y); }
        public float Height { get => CustomMinimumSize.Y; set => CustomMinimumSize = new Vector2(CustomMinimumSize.X, value); }

        public bool Visibility { get => _container.Visible; set => _container.Visible = value; }

        //public SizeFlags SizeFlagsHorizontal { get => _container.SizeFlagsHorizontal; set => _container.SizeFlagsHorizontal = value; }
        //public Vector2 Position { get => _container.Position; set => _container.Position = value; }
        string ILingoFrameworkGfxNode.Name { get => Name; set
                {
                Name = value; _container.Name = value + "_Flow";
            }
        }

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
                _marginContainer.RemoveChild(_container);
                _container.QueueFree();
                _orientation = value;
                _container = CreateContainer(value);
                _marginContainer.AddChild(_container);
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

        

        public void AddChild(ILingoFrameworkGfxLayoutNode child)
        {
            if (child is not Node node)
                return;
            
            if (node is Control ctrl)
                ApplyItemMargin(ctrl);
            _container.AddChild(node);
        }
        public void RemoveChild(ILingoFrameworkGfxLayoutNode child)
        {
            if (child is not Node node)
                return;
            _container.RemoveChild(node);
        }
        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetChildren() => _container.GetChildren().OfType<ILingoFrameworkGfxLayoutNode>().ToArray();
        public ILingoFrameworkGfxLayoutNode? GetChild(int index) => _container.GetChild(index) as ILingoFrameworkGfxLayoutNode;
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
            _marginContainer.AddThemeConstantOverride("margin_left", (int)_margin.Left);
            _marginContainer.AddThemeConstantOverride("margin_right", (int)_margin.Right);
            _marginContainer.AddThemeConstantOverride("margin_top", (int)_margin.Top);
            _marginContainer.AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
        }

    }
}
