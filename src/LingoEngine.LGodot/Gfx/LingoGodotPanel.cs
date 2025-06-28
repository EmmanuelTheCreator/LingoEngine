using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using System;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxPanel"/>.
    /// </summary>
    public partial class LingoGodotPanel : Control, ILingoFrameworkGfxPanel, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        private LingoColor _background = new LingoColor(0,0,0);
        private LingoColor _border = new LingoColor(0,0,0);
        private float _borderWidth;
        private readonly StyleBoxFlat _style = new StyleBoxFlat();

        public LingoGodotPanel(LingoGfxPanel panel)
        {
            panel.Init(this);
            AddThemeStyleboxOverride("panel", _style);
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
                ApplyMargin();
            }
        }

        public void AddChild(ILingoFrameworkGfxNode child)
        {
            if (child is Node node)
                AddChild(node);
        }
        public void RemoveChild(ILingoFrameworkGfxNode child)
        {
            if (child is not Node node)
                return;
            RemoveChild(node);
        }
        public IEnumerable<ILingoFrameworkGfxNode> GetChildren() => GetChildren().OfType<ILingoFrameworkGfxNode>().ToArray();

        public LingoColor BackgroundColor
        {
            get => _background;
            set
            {
                _background = value;
                ApplyStyle();
            }
        }

        public LingoColor BorderColor
        {
            get => _border;
            set
            {
                _border = value;
                ApplyStyle();
            }
        }

        public float BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = value;
                ApplyStyle();
            }
        }

        public new void Dispose()
        {
            base.Dispose();
            QueueFree();
        }

        private void ApplyMargin()
        {
            AddThemeConstantOverride("margin_left", (int)_margin.Left);
            AddThemeConstantOverride("margin_right", (int)_margin.Right);
            AddThemeConstantOverride("margin_top", (int)_margin.Top);
            AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
        }

        private void ApplyStyle()
        {
            _style.BgColor = new Color(_background.R, _background.G, _background.B);
            _style.BorderColor = new Color(_border.R, _border.G, _border.B);
            _style.BorderWidthTop = _style.BorderWidthBottom = _style.BorderWidthLeft = _style.BorderWidthRight = (int)_borderWidth;
        }
    }
}
