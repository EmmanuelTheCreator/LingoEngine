using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Simple container that allows placing child nodes at arbitrary coordinates.
    /// </summary>
    public class LingoPanel : LingoGfxNodeBase<ILingoFrameworkGfxPanel>
    {
        /// <summary>Adds a child to the panel and sets its position.</summary>
        public void AddChild(ILingoGfxNode node, float x, float y)
        {
            node.X = x;
            node.Y = y;
            _framework.AddChild(node.Framework<ILingoFrameworkGfxNode>());
        }

        /// <summary>Adds a child without modifying its position.</summary>
        public void AddChild(ILingoGfxNode node) => _framework.AddChild(node.Framework<ILingoFrameworkGfxNode>());

        public LingoColor BackgroundColor { get => _framework.BackgroundColor; set => _framework.BackgroundColor = value; }
        public LingoColor BorderColor { get => _framework.BorderColor; set => _framework.BorderColor = value; }
        public float BorderWidth { get => _framework.BorderWidth; set => _framework.BorderWidth = value; }
    }
}
