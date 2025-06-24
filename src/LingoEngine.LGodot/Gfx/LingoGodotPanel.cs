using Godot;
using LingoEngine.Gfx;
using System;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkPanel"/>.
    /// </summary>
    public partial class LingoGodotPanel : Control, ILingoFrameworkPanel, IDisposable
    {
        public LingoGodotPanel(LingoPanel panel)
        {
            panel.Init(this);
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }

        public void AddChild(ILingoFrameworkGfxNode child)
        {
            if (child is Node node)
                base.AddChild(node);
        }

        public void Dispose() => QueueFree();
    }
}
