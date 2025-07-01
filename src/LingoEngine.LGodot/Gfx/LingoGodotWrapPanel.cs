using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxWrapPanel"/>.
    /// </summary>
    public partial class LingoGodotWrapPanel : MarginContainer, ILingoFrameworkGfxWrapPanel, IDisposable
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
            FlowContainer container = orientation == LingoOrientation.Horizontal ? new VFlowContainer() : new HFlowContainer();
            //container.SizeFlagsVertical = SizeFlags.ExpandFill;
            //container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            //container.SizeFlagsHorizontal = SizeFlags.Expand;
            //container.SizeFlagsVertical = SizeFlags.Expand;
            container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            container.SizeFlagsVertical = SizeFlags.ExpandFill;
            return container;
        }

        public override void _Ready()
        {
            base._Ready();
            ApplyMargin();
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => CustomMinimumSize.X; set => CustomMinimumSize = new Vector2(value, CustomMinimumSize.Y); }
        public float Height { get => CustomMinimumSize.Y; set => CustomMinimumSize = new Vector2(CustomMinimumSize.X, value); }

        public bool Visibility { get => Visible; set => Visible = value; }

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
        public object FrameworkNode => this;

        private readonly List<ILingoFrameworkGfxNode> _nodes = new List<ILingoFrameworkGfxNode>();
        public void AddItem(ILingoFrameworkGfxNode child)
        {
            if (child.FrameworkNode is not Node node)
                return;
            
            if (node is Control ctrl)
                ApplyItemMargin(ctrl);
            _container.AddChild(node);
            _nodes.Add(child);
        }
        public void RemoveItem(ILingoFrameworkGfxNode child)
        {
            if (child.FrameworkNode is not Node node)
                return;
            _container.RemoveChild(node);
            _nodes.Remove(child);
        }
        public IEnumerable<ILingoFrameworkGfxNode> GetItems() => _nodes.ToArray();
        public ILingoFrameworkGfxNode? GetItem(int index) => _nodes[index];
        public void RemoveAll()
        {
            foreach (var child in GetItems())
            {
                if (child != GetItem(0))
                    RemoveItem(child);
            }
        }


        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }

        private void ApplyItemMargin(Control ctrl)
        {
            //ctrl.AddThemeConstantOverride("margin_left", (int)_itemMargin.Left);
            //ctrl.AddThemeConstantOverride("margin_right", (int)_itemMargin.Right);
            //ctrl.AddThemeConstantOverride("margin_top", (int)_itemMargin.Top);
            //ctrl.AddThemeConstantOverride("margin_bottom", (int)_itemMargin.Bottom);
        }

        private void ApplyMargin()
        {
           AddThemeConstantOverride("margin_left", (int)_margin.Left);
           AddThemeConstantOverride("margin_right", (int)_margin.Right);
           AddThemeConstantOverride("margin_top", (int)_margin.Top);
           AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
        }

        
    }
}
