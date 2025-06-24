using System;
using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a panel that arranges children with wrapping.
    /// </summary>
    public class LingoWrapPanel : IDisposable
    {
#pragma warning disable CS8618
        private ILingoFrameworkWrapPanel _framework;
#pragma warning restore CS8618

        /// <summary>Initialize with the framework specific panel.</summary>
        public void Init(ILingoFrameworkWrapPanel framework) => _framework = framework;

        public T Framework<T>() where T : ILingoFrameworkWrapPanel => (T)_framework;

        public LingoOrientation Orientation
        {
            get => _framework.Orientation;
            set => _framework.Orientation = value;
        }

        public LingoMargin ItemMargin
        {
            get => _framework.ItemMargin;
            set => _framework.ItemMargin = value;
        }

        public LingoMargin Margin
        {
            get => _framework.Margin;
            set => _framework.Margin = value;
        }

        public void AddChild(LingoWrapPanel panel) => _framework.AddChild(panel.Framework<ILingoFrameworkWrapPanel>());

        public void Dispose() => (_framework as IDisposable)?.Dispose();
    }
}
