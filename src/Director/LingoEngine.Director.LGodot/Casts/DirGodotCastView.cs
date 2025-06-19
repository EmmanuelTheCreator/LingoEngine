using Godot;
using LingoEngine.Casts;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.LGodot;
using LingoEngine.Members;
using System.Linq;

namespace LingoEngine.Director.LGodot.Casts
{
    internal class DirGodotCastView : IDirFrameworkCast
    {
        private readonly HFlowContainer _elementsContainer;
        private readonly ScrollContainer _ScrollContainer;
        private readonly List<DirGodotCastItem> _elements = new List<DirGodotCastItem>();
        private readonly Action<DirGodotCastItem> _onSelectItem;
        private readonly Action<DirGodotCastItem>? _onDoubleClickItem;
        private readonly DirectorStyle _style;

        public Node Node => _ScrollContainer;

        public DirGodotCastView(Action<DirGodotCastItem> onSelect, Action<DirGodotCastItem>? onDoubleClick, DirectorStyle style)
        {
            _ScrollContainer = new ScrollContainer();
            _ScrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _ScrollContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

            _elementsContainer = new HFlowContainer();
            _elementsContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _elementsContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _ScrollContainer.AddChild(_elementsContainer);
            _onSelectItem = onSelect;
            _onDoubleClickItem = onDoubleClick;
            _style = style;
        }

        public void Show(ILingoCast cast)
        {
            Hide();
            var i = 0;
            foreach (var castItem in cast.GetAll())
            {
                var dirCastItem = new DirGodotCastItem(castItem, i + 1, _onSelectItem, _style.SelectedColor, _onDoubleClickItem);
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

        public DirGodotCastItem? FindItem(ILingoMember member)
        {
            return _elements.FirstOrDefault(e => e.LingoMember == member);
        }

        public void SelectItem(DirGodotCastItem item)
        {
            _onSelectItem(item);
        }


    }
}
