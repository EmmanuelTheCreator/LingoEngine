using Godot;
using LingoEngine.Core;
using LingoEngine.Director.Core.Casts;

namespace LingoEngine.Director.LGodot.Casts
{
    internal class DirGodotCastView : IDirFrameworkCast
    {
        private readonly HFlowContainer _elementsContainer;
        private readonly ScrollContainer _ScrollContainer;
        private readonly List<DirGodotCastItem> _elements = new List<DirGodotCastItem>();
        private readonly Action<DirGodotCastItem> _onSelectItem;

        public Node Node => _ScrollContainer;

        public DirGodotCastView(Action<DirGodotCastItem> onSelect)
        {
            _ScrollContainer = new ScrollContainer();
            _ScrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _ScrollContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

            _elementsContainer = new HFlowContainer();
            _elementsContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _elementsContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _ScrollContainer.AddChild(_elementsContainer);
            _onSelectItem = onSelect;
        }

        public void Show(ILingoCast cast)
        {
            Hide();
            var i = 0;
            foreach (var castItem in cast.GetAll())
            {
                var dirCastItem = new DirGodotCastItem(castItem, i+1, _onSelectItem);
                dirCastItem.Init();
                _elements.Add(dirCastItem);
                _elementsContainer.AddChild(dirCastItem);
                i++;
            }
        }
        public void Hide()
        {
            foreach (var element in _elements)
            {
                element.Dispose();
            }
            _elements.Clear();
        }
       

    }
}
