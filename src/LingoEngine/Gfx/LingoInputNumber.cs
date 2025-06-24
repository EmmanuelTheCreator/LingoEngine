using System;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a numeric input field.
    /// </summary>
    public class LingoInputNumber : ILingoGfxNode, IDisposable
    {
#pragma warning disable CS8618
        private ILingoFrameworkInputNumber _framework;
#pragma warning restore CS8618

        public void Init(ILingoFrameworkInputNumber framework) => _framework = framework;

        public T Framework<T>() where T : ILingoFrameworkGfxNode => (T)_framework;

        public float X { get => _framework.X; set => _framework.X = value; }
        public float Y { get => _framework.Y; set => _framework.Y = value; }
        public float Width { get => _framework.Width; set => _framework.Width = value; }
        public float Height { get => _framework.Height; set => _framework.Height = value; }
        public bool Visibility { get => _framework.Visibility; set => _framework.Visibility = value; }

        public float Value { get => _framework.Value; set => _framework.Value = value; }
        public float Min { get => _framework.Min; set => _framework.Min = value; }
        public float Max { get => _framework.Max; set => _framework.Max = value; }

        public void Dispose() => (_framework as IDisposable)?.Dispose();
    }
}
