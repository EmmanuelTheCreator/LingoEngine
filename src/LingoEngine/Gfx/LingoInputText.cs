using System;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a single line text input.
    /// </summary>
    public class LingoInputText : ILingoGfxNode, IDisposable
    {
#pragma warning disable CS8618
        private ILingoFrameworkInputText _framework;
#pragma warning restore CS8618

        public void Init(ILingoFrameworkInputText framework) => _framework = framework;

        public T Framework<T>() where T : ILingoFrameworkGfxNode => (T)_framework;

        public float X { get => _framework.X; set => _framework.X = value; }
        public float Y { get => _framework.Y; set => _framework.Y = value; }
        public float Width { get => _framework.Width; set => _framework.Width = value; }
        public float Height { get => _framework.Height; set => _framework.Height = value; }
        public bool Visibility { get => _framework.Visibility; set => _framework.Visibility = value; }

        public string Text { get => _framework.Text; set => _framework.Text = value; }
        public int MaxLength { get => _framework.MaxLength; set => _framework.MaxLength = value; }
        public string? Font { get => _framework.Font; set => _framework.Font = value; }

        public void Dispose() => (_framework as IDisposable)?.Dispose();
    }
}
