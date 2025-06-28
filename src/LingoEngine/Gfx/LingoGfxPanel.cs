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
        public ILingoGfxNode AddChild(ILingoGfxNode node, float x, float y)
        {
            if (node is ILingoFrameworkGfxLayoutNode layoutNode)
            {
                layoutNode.X = x;
                layoutNode.Y = y;
                _framework.AddChild(node.Framework<ILingoFrameworkGfxLayoutNode>());
                return node;
            }
            else
            {
                LingoGfxLayoutWrapper item = _factory.CreateLayoutWrapper(node ,x, y);

                _framework.AddChild(item.FrameworkWrapper<ILingoFrameworkGfxLayoutWrapper>());
                return item;
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


        public override bool Visibility { get => Content.Visibility; set => Content.Visibility = value; }
        public override string Name { get => Content.Name; set => Content.Name = value; }
        public override LingoMargin Margin { get => Content.Margin; set => Content.Margin = value; }
        public override float Width { get => Content.Width; set => Content.Width = value; }
        public override float Height { get => Content.Height; set => Content.Height = value; }


        public virtual T FrameworkWrapper<T>() where T : ILingoFrameworkGfxLayoutWrapper => (T)(object)_framework;
        public virtual ILingoFrameworkGfxNode FrameworkObjWrapper => _framework;


        public override T Framework<T>()
        {
            if (typeof(T) == typeof(ILingoFrameworkGfxLayoutNode) || typeof(T) == typeof(ILingoFrameworkGfxLayoutWrapper))
                return (T)(object)_framework;
            return Content.Framework<T>();
        }

        public override ILingoFrameworkGfxNode FrameworkObj => _framework;



        public LingoGfxLayoutWrapper(ILingoGfxNode content)
        {
            Content = content;
            
        }
    



    }
}

