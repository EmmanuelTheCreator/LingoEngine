using Godot;
using LingoEngine.Gfx;
using System;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkInputCheckbox"/>.
    /// </summary>
    public partial class LingoGodotInputCheckbox : CheckBox, ILingoFrameworkInputCheckbox, IDisposable
    {
        public LingoGodotInputCheckbox(LingoInputCheckbox input)
        {
            input.Init(this);
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }

        public bool Checked { get => ButtonPressed; set => ButtonPressed = value; }

        public void Dispose() => QueueFree();
    }
}
