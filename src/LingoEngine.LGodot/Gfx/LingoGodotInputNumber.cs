using Godot;
using LingoEngine.Gfx;
using System;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkInputNumber"/>.
    /// </summary>
    public partial class LingoGodotInputNumber : SpinBox, ILingoFrameworkInputNumber, IDisposable
    {
        public LingoGodotInputNumber(LingoInputNumber input)
        {
            input.Init(this);
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }

        public float Value { get => (float)base.Value; set => base.Value = value; }
        public float Min { get => (float)MinValue; set => MinValue = value; }
        public float Max { get => (float)MaxValue; set => MaxValue = value; }

        public void Dispose() => QueueFree();
    }
}
