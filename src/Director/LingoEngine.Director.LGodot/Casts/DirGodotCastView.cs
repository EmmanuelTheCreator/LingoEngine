using Godot;
using LingoEngine.Core;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.LGodot.Inspector;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastWindow : BaseGodotWindow, IDisposable
    {
        private readonly TabContainer _tabs;
        private readonly Inspector.DirGodotObjectInspector _inspector;

        private readonly ILingoMovie _lingoMovie;
        private bool _wasToggleKey;

        public ILingoCast? ActiveCastLib { get; private set; }
        public DirGodotCastView CastLibViewer { get; }

        public DirGodotCastWindow(Node parent, ILingoMovie lingoMovie, IDirectorEventMediator mediator)
            : base("Cast")
        {
            Position = new Vector2(650, 20);
            Size = new Vector2(360, 620);
            CustomMinimumSize = Size;
            _tabs = new TabContainer();
            InitTabs();
            _tabs.Position = new Vector2(0, 20);

            parent.AddChild(this);
            _lingoMovie = lingoMovie;
            foreach (var cast in lingoMovie.CastLib.GetAll())
            {
                var castLibViewer = new DirGodotCastView(OnSelectElement);
                castLibViewer.Show(cast);
                var tabContent = new VBoxContainer
                {
                    Name = cast.Name,
                };

                tabContent.AddChild(castLibViewer.Node);
                _tabs.AddChild(tabContent);
            }
            _inspector = new Inspector.DirGodotObjectInspector(mediator) { Visible = false };
            parent.AddChild(_inspector);
        }

        public void Toggle()
        {
            Visible = !Visible;
        }

        public override void _Process(double delta)
        {
            if (Input.IsKeyPressed(Key.F3) && !_wasToggleKey)
                Toggle();
            _wasToggleKey = Input.IsKeyPressed(Key.F3);
        }

        private void InitTabs()
        {
            _tabs.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _tabs.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _tabs.Size = new Vector2(350, 600);
            AddChild(_tabs);
            var existingFont = _tabs.GetThemeFont("font", "TabContainer");
            if (existingFont != null)
            {
                _tabs.AddThemeFontOverride("font", existingFont);
                _tabs.AddThemeFontSizeOverride("font_size", 10);
            }
        }

        private void OnSelectElement(DirGodotCastItem castItem)
        {
            _inspector.Visible = true;
            _inspector.ShowObject(castItem.LingoMember);
        }
        public void Activate(int castlibNum)
        {
            //ActiveCastLib = _lingoMovie.CastLib.GetCast(castlibNum);
            //if (ActiveCastLib != null)
            //    CastLibViewer.Show(ActiveCastLib);
        }

        public void Dispose()
        {
            CastLibViewer.Hide();
        }
    }
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
