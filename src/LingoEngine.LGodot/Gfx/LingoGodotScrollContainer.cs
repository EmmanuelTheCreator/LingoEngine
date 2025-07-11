using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using System.Linq;

namespace LingoEngine.LGodot.Gfx
{
    public partial class LingoGodotScrollContainer : ScrollContainer, ILingoFrameworkGfxScrollContainer, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        public LingoGodotScrollContainer(LingoGfxScrollContainer container)
        {
            container.Init(this);
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        string ILingoFrameworkGfxNode.Name { get => Name; set => Name = value; }

        public new float ScrollHorizontal
        {
            get => base.ScrollHorizontal;
            set => base.ScrollHorizontal = (int)value;
        }
        public new float ScrollVertical
        {
            get => base.ScrollVertical;
            set => base.ScrollVertical = (int)value;
        }
        public new bool ClipContents
        {
            get => base.ClipContents;
            set => base.ClipContents = value;
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


        private readonly List<ILingoFrameworkGfxLayoutNode> _nodes = new List<ILingoFrameworkGfxLayoutNode>();
        public void AddItem(ILingoFrameworkGfxLayoutNode child)
        {
            if (child.FrameworkNode is Node node)
                base.AddChild(node);
            _nodes.Add(child);
        }
        public void RemoveItem(ILingoFrameworkGfxLayoutNode lingoFrameworkGfxNode)
        {
            if (lingoFrameworkGfxNode.FrameworkNode is Node node)
                RemoveChild(node);
            _nodes.Remove(lingoFrameworkGfxNode);
        }

        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _nodes.ToArray();



        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }

       
    }
}
