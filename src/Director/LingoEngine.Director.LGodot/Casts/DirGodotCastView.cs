using Godot;
using LingoEngine.Casts;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.LGodot;
using LingoEngine.Members;
using LingoEngine.Core;
using System.Linq;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.LGodot.Casts
{
    internal class DirGodotCastView : IDirFrameworkCast
    {
        private readonly HFlowContainer _elementsContainer;
        private readonly ScrollContainer _ScrollContainer;
        private readonly List<DirGodotCastItem> _elements = new List<DirGodotCastItem>();
        private readonly Action<DirGodotCastItem> _onSelectItem;
        private readonly DirectorStyle _style;
        private readonly ILingoCommandManager _commandManager;
        private readonly IDirGodotIconManager _iconManager;
        private readonly ILingoFrameworkFactory _factory;

        public Node Node => _ScrollContainer;

        public DirGodotCastView(Action<DirGodotCastItem> onSelect, DirectorStyle style, ILingoCommandManager commandManager, IDirGodotIconManager iconManager, ILingoFrameworkFactory factory)
        {
            _ScrollContainer = new ScrollContainer();
            _ScrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _ScrollContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

            _elementsContainer = new HFlowContainer();
            _elementsContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _elementsContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _ScrollContainer.AddChild(_elementsContainer);
            _onSelectItem = onSelect;
            _style = style;
            _commandManager = commandManager;
            _iconManager = iconManager;
            _factory = factory;
        }

        public void Show(ILingoCast cast)
        {
            Hide();
            var i = 0;
            foreach (var castItem in cast.GetAll())
            {
                var dirCastItem = new DirGodotCastItem(castItem, i+1, _onSelectItem, _style.SelectedColor, _commandManager, _factory, _iconManager);
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
