using System;
using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Base class for all engine level graphics nodes exposing common properties.
    /// </summary>
    public abstract class LingoGfxNodeBase<TFramework> : ILingoGfxNode, IDisposable
        where TFramework : ILingoFrameworkGfxNode
    {
#pragma warning disable CS8618
        protected TFramework _framework;
#pragma warning restore CS8618

        public float X { get => _framework.X; set => _framework.X = value; }
        public float Y { get => _framework.Y; set => _framework.Y = value; }
        public float Width { get => _framework.Width; set => _framework.Width = value; }
        public float Height { get => _framework.Height; set => _framework.Height = value; }
        public bool Visibility { get => _framework.Visibility; set => _framework.Visibility = value; }
        public LingoMargin Margin { get => _framework.Margin; set => _framework.Margin = value; }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public T Framework<T>() where T : ILingoFrameworkGfxNode => (T)(object)_framework;

        public void Init(TFramework framework) => _framework = framework;


        public virtual void Dispose() => (_framework as IDisposable)?.Dispose();
    }
}
