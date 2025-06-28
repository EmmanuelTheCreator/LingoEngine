using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Simple container that allows placing child nodes at arbitrary coordinates.
    /// </summary>
    public class LingoGfxPanel : LingoGfxNodeLayoutBase<ILingoFrameworkGfxPanel>
    {
        private readonly ILingoFrameworkFactory _factory;

        public LingoGfxPanel(ILingoFrameworkFactory factory)
        {
            _factory = factory;
        }


        /// <summary>Adds a child to the panel and sets its position.</summary>
        public void AddChild(ILingoGfxNode node, float x, float y)
        {
            if (node is ILingoFrameworkGfxLayoutNode layoutNode)
            {
                layoutNode.X = x;
                layoutNode.Y = y;
                _framework.AddChild(node.Framework<ILingoFrameworkGfxLayoutNode>());
            }
            else
            {
                LingoGfxLayoutWrapper item = _factory.CreateLayoutWrapper(node ,x, y);
               
                _framework.AddChild(item.Framework<ILingoFrameworkGfxLayoutNode>());
            }
            
        }

        /// <summary>Adds a child without modifying its position.</summary>
        public void AddChild(ILingoGfxNode node) => _framework.AddChild(node.Framework<ILingoFrameworkGfxLayoutNode>());
        public void AddChild(ILingoFrameworkGfxLayoutNode node) => _framework.AddChild(node);
        public void RemoveChild(ILingoGfxNode node) => _framework.RemoveChild(node.Framework<ILingoFrameworkGfxLayoutNode>());
        public void RemoveChild(ILingoFrameworkGfxLayoutNode node) => _framework.RemoveChild(node);
        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetChildren() => _framework.GetChildren();

        public LingoColor BackgroundColor { get => _framework.BackgroundColor; set => _framework.BackgroundColor = value; }
        public LingoColor BorderColor { get => _framework.BorderColor; set => _framework.BorderColor = value; }
        public float BorderWidth { get => _framework.BorderWidth; set => _framework.BorderWidth = value; }
    }





    public class LingoGfxLayoutWrapper : LingoGfxNodeLayoutBase<ILingoFrameworkGfxLayoutWrapper>
    {
        public ILingoGfxNode Content { get; set; }

        public override string Name { get => Content.Name; set => Content.Name = value; }
        public override bool Visibility { get => Content.Visibility; set => Content.Visibility = value; }

        public LingoGfxLayoutWrapper(ILingoGfxNode content)
        {
            Content = content;
        }

        public override T Framework<T>()
        {
            if (typeof(T) == typeof(ILingoFrameworkGfxLayoutNode))
                return (T)(object)FrameworkObj;
            return Content.Framework<T>();
        }

        public override ILingoFrameworkGfxNode FrameworkObj => _wrapped ??= new WrapperFramework(Content.FrameworkObj);

        private ILingoFrameworkGfxLayoutNode? _wrapped;

        private class WrapperFramework : ILingoFrameworkGfxLayoutNode
        {
            private readonly ILingoFrameworkGfxNode _inner;
            private readonly LingoMargin _margin = LingoMargin.Zero;
            private float _x, _y, _w, _h;

            public WrapperFramework(ILingoFrameworkGfxNode inner)
            {
                _inner = inner;
            }

            public float X { get => _x; set => _x = value; }
            public float Y { get => _y; set => _y = value; }
            public float Width { get => _w; set => _w = value; }
            public float Height { get => _h; set => _h = value; }
            public LingoMargin Margin { get => _margin; set { } } // optional: ignore or store

            public string Name { get => _inner.Name; set => _inner.Name = value; }
            public bool Visibility { get => _inner.Visibility; set => _inner.Visibility = value; }

            public void Dispose() => _inner.Dispose();
        }
    }
}

