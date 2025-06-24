using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using System;
using System.Linq;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkWrapPanel"/>.
    /// </summary>
    public partial class LingoGodotWrapPanel : Control, ILingoFrameworkWrapPanel, IDisposable
    {
        private FlowContainer _container;
        private LingoOrientation _orientation;
        private LingoMargin _itemMargin;
        private LingoMargin _margin;

        public LingoGodotWrapPanel(LingoWrapPanel panel, LingoOrientation orientation)
        {
            _orientation = orientation;
            _itemMargin = LingoMargin.Zero;
            _margin = LingoMargin.Zero;
            _container = orientation == LingoOrientation.Horizontal ? new HFlowContainer() : new VFlowContainer();
            AddChild(_container);
            panel.Init(this);
        }

        public LingoOrientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation == value)
                    return;
                var children = _container.GetChildren().OfType<Node>().ToArray();
                foreach (var c in children)
                    _container.RemoveChild(c);
                RemoveChild(_container);
                _container.QueueFree();
                _orientation = value;
                _container = value == LingoOrientation.Horizontal ? new HFlowContainer() : new VFlowContainer();
                AddChild(_container);
                ApplyMargin();
                foreach (var c in children)
                {
                    _container.AddChild(c);
                    if (c is Control ctrl)
                        ApplyItemMargin(ctrl);
                }
            }
        }

        public LingoMargin ItemMargin
        {
            get => _itemMargin;
            set
            {
                _itemMargin = value;
                foreach (var child in _container.GetChildren().OfType<Control>())
                    ApplyItemMargin(child);
            }
        }

        public LingoMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                ApplyMargin();
            }
        }

        public void AddChild(ILingoFrameworkWrapPanel child)
        {
            var node = (LingoGodotWrapPanel)child;
            ApplyItemMargin(node);
            _container.AddChild(node);
        }

        public void Dispose() => QueueFree();

        private void ApplyItemMargin(Control ctrl)
        {
            ctrl.AddThemeConstantOverride("margin_left", (int)_itemMargin.Left);
            ctrl.AddThemeConstantOverride("margin_right", (int)_itemMargin.Right);
            ctrl.AddThemeConstantOverride("margin_top", (int)_itemMargin.Top);
            ctrl.AddThemeConstantOverride("margin_bottom", (int)_itemMargin.Bottom);
        }

        private void ApplyMargin()
        {
            _container.AddThemeConstantOverride("margin_left", (int)_margin.Left);
            _container.AddThemeConstantOverride("margin_right", (int)_margin.Right);
            _container.AddThemeConstantOverride("margin_top", (int)_margin.Top);
            _container.AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
        }
    }
}
