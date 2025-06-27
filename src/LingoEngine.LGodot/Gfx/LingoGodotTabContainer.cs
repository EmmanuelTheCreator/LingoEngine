using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using System;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxTabContainer"/>.
    /// </summary>
    public partial class LingoGodotTabContainer : TabContainer, ILingoFrameworkGfxTabContainer, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;

        public LingoGodotTabContainer(LingoGfxTabContainer tab)
        {
            tab.Init(this);
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        string ILingoFrameworkGfxNode.Name { get => Name; set => Name = value; }

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

        public void AddTab(string title, ILingoFrameworkGfxNode content)
        {
            if (content is Node node)
                AddTab(title, node);
        }

        public void AddTab(string title, Node node)
        {
            AddChild(node);
            SetTabTitle(GetChildCount() - 1, title);
        }

        public void ClearTabs()
        {
            foreach (Node child in GetChildren())
            {
                RemoveChild(child);
                child.QueueFree();
            }
        }

        public new void Dispose()
        {
            base.Dispose();
            QueueFree();
        }
    }
}
