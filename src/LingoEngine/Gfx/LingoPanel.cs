using System;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Simple container that allows placing child nodes at arbitrary coordinates.
    /// </summary>
    public class LingoPanel : ILingoGfxNode, IDisposable
    {
#pragma warning disable CS8618
        private ILingoFrameworkPanel _framework;
#pragma warning restore CS8618

        /// <summary>Initialize with the framework specific panel.</summary>
        public void Init(ILingoFrameworkPanel framework) => _framework = framework;

        public T Framework<T>() where T : ILingoFrameworkGfxNode => (T)_framework;

        public float X { get => _framework.X; set => _framework.X = value; }
        public float Y { get => _framework.Y; set => _framework.Y = value; }
        public float Width { get => _framework.Width; set => _framework.Width = value; }
        public float Height { get => _framework.Height; set => _framework.Height = value; }
        public bool Visibility { get => _framework.Visibility; set => _framework.Visibility = value; }

        /// <summary>Adds a child to the panel and sets its position.</summary>
        public void AddChild(ILingoGfxNode node, float x, float y)
        {
            node.X = x;
            node.Y = y;
            _framework.AddChild(node.Framework<ILingoFrameworkGfxNode>());
        }

        /// <summary>Adds a child without modifying its position.</summary>
        public void AddChild(ILingoGfxNode node) => _framework.AddChild(node.Framework<ILingoFrameworkGfxNode>());

        public void Dispose() => (_framework as IDisposable)?.Dispose();
    }
}
