using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using System.Collections.Generic;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxWindow"/>.
    /// </summary>
    public partial class LingoGodotWindow : Window, ILingoFrameworkGfxWindow, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        private readonly List<ILingoFrameworkGfxLayoutNode> _nodes = new();

        public LingoGodotWindow(LingoGfxWindow window)
        {
            window.Init(this);
        }

        public float X { get => Position.X; set => Position = new Vector2I((int)value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2I(Position.X, (int)value); }
        public float Width { get => Size.X; set => Size = new Vector2I((int)value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2I(Size.X, (int)value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        string ILingoFrameworkGfxNode.Name { get => Name; set => Name = value; }
        public new string Title { get => base.Title; set => base.Title = value; }

        public LingoMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                AddThemeConstantOverride("margin_left", (int)_margin.Left);
                AddThemeConstantOverride("margin_right", (int)_margin.Right);
                AddThemeConstantOverride("margin_top", (int)_margin.Top);
                AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
            }
        }

        public object FrameworkNode => this;

        public void AddItem(ILingoFrameworkGfxLayoutNode child)
        {
            if (child.FrameworkNode is Node node)
                AddChild(node);
            _nodes.Add(child);
        }

        public void RemoveItem(ILingoFrameworkGfxLayoutNode child)
        {
            if (child.FrameworkNode is Node node)
                RemoveChild(node);
            _nodes.Remove(child);
        }

        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _nodes.ToArray();

        public void Popup() => base.Popup();
        public void PopupCentered() => base.PopupCentered();
        public new void Hide() => base.Hide();

        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }
    }
}
