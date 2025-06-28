using Godot;
using LingoEngine.Gfx;
using LingoEngine.LGodot.Primitives;
using LingoEngine.Primitives;
using System;
using System.ComponentModel;
using System.Linq;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxPanel"/>.
    /// </summary>
    public partial class LingoGodotPanel : PanelContainer, ILingoFrameworkGfxPanel, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        private LingoColor _background = new LingoColor(0,0,0);
        private LingoColor _border = new LingoColor(0,0,0);
        private float _borderWidth;
        private readonly StyleBoxFlat _style = new StyleBoxFlat();

        public LingoGodotPanel(LingoGfxPanel panel)
        {
            panel.Init(this);
            SizeFlagsVertical = SizeFlags.ExpandFill;
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
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

        public void AddChild(ILingoFrameworkGfxLayoutNode child)
        {
            if (child is Node node)
                AddChild(node);
        }
        public void RemoveChild(ILingoFrameworkGfxLayoutNode child)
        {
            if (child is not Node node)
                return;
            RemoveChild(node);
        }
        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetChildren()
            => base.GetChildren().OfType<Node>()
                .OfType<ILingoFrameworkGfxLayoutNode>().ToArray();

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
            _style.BgColor = _background.ToGodotColor();
            _style.BorderColor = _border.ToGodotColor();
            _style.BorderWidthTop = _style.BorderWidthBottom = _style.BorderWidthLeft = _style.BorderWidthRight = (int)_borderWidth;
        }
    }
    public partial class LingoGodotLayoutWrapper : MarginContainer, ILingoFrameworkGfxLayoutWrapper
    {
        private LingoGfxLayoutWrapper _lingoLayoutWrapper;

        public LingoGodotLayoutWrapper(LingoGfxLayoutWrapper layoutWrapper)
        {
            _lingoLayoutWrapper = layoutWrapper;
            layoutWrapper.Init(this);
            var content = layoutWrapper.Content.FrameworkObj;

            if (content is Control control)
            {
                AddChild(control);
                control.SizeFlagsHorizontal = SizeFlags.Fill;
                control.SizeFlagsVertical = SizeFlags.Fill;
            }
            else
            {
                // todo: use ILogger
                GD.PushError($"Content of layout wrapper '{layoutWrapper.Name}' is not a Control: {content.GetType().Name}");
            }
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width
        {
            get => Size.X;
            set => CustomMinimumSize = new Vector2(value, CustomMinimumSize.Y);
        }
        public float Height
        {
            get => Size.Y;
            set => CustomMinimumSize = new Vector2(CustomMinimumSize.X, value);
        }

        public LingoMargin Margin
        {
            get => new LingoMargin(
                GetThemeConstant("margin_top"),
                GetThemeConstant("margin_right"),
                GetThemeConstant("margin_bottom"),
                GetThemeConstant("margin_left"));
            set
            {
                AddThemeConstantOverride("margin_top", (int)value.Top);
                AddThemeConstantOverride("margin_right", (int)value.Right);
                AddThemeConstantOverride("margin_bottom", (int)value.Bottom);
                AddThemeConstantOverride("margin_left", (int)value.Left);
            }
        }

        public new string Name { get => base.Name; set => base.Name = value; }
        public bool Visibility { get => Visible; set => Visible = value; }

        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }
    }

}
