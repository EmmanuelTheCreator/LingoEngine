using Godot;
using LingoEngine.Gfx;
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

        public LingoGodotWrapPanel(LingoWrapPanel panel, LingoOrientation orientation)
        {
            _orientation = orientation;
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
                foreach (var c in children)
                    _container.AddChild(c);
            }
        }

        public void AddChild(ILingoFrameworkWrapPanel child)
        {
            var node = (LingoGodotWrapPanel)child;
            _container.AddChild(node);
        }

        public void Dispose() => QueueFree();
    }
}
