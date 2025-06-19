using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.LGodot;
using LingoEngine.Movies;
using LingoEngine.Texts;
using LingoEngine.Members;
using LingoEngine.Casts;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastWindow : BaseGodotWindow, IDisposable, IHasFindMemberEvent
    {
        private readonly TabContainer _tabs;
        

        private readonly ILingoMovie _lingoMovie;
        private readonly IDirectorEventMediator _mediator;
        private readonly DirectorStyle _style;
        private readonly Dictionary<int, DirGodotCastView> _castViews = new();
        private DirGodotCastItem? _selectedItem;

        public ILingoCast? ActiveCastLib { get; private set; }

        public DirGodotCastWindow(IDirectorEventMediator mediator, ILingoMovie lingoMovie, DirectorStyle style)
            : base("Cast")
        {
            _lingoMovie = lingoMovie;
            _mediator = mediator;
            _style = style;
            _mediator.SubscribeToMenu(DirectorMenuCodes.CastWindow, () => Visible = !Visible);
            _mediator.Subscribe(this);

            Size = new Vector2(360, 620);
            CustomMinimumSize = Size;


            _tabs = new TabContainer();
            InitTabs();
            _tabs.Position = new Vector2(0, TitleBarHeight + 20);

            AddChild(_tabs);

            foreach (var cast in lingoMovie.CastLib.GetAll())
            {
                var castLibViewer = new DirGodotCastView(OnSelectElement, OnItemDoubleClick, _style);
                castLibViewer.Show(cast);
                _castViews.Add(cast.Number, castLibViewer);
                var tabContent = new VBoxContainer
                {
                    Name = cast.Name,
                };

                tabContent.AddChild(castLibViewer.Node);
                _tabs.AddChild(tabContent);
            }
           
        }


        protected override void OnResizing(Vector2 size)
        {
            base.OnResizing(size);
            _tabs.Size = new Vector2(Size.X - 10, Size.Y- TitleBarHeight -30);
        }




        private void InitTabs()
        {
            _tabs.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _tabs.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _tabs.Size = new Vector2(Size.X - 10, Size.Y - TitleBarHeight - 30);
            var existingFont = _tabs.GetThemeFont("font", "TabContainer");
            if (existingFont != null)
            {
                _tabs.AddThemeFontOverride("font", existingFont);
                _tabs.AddThemeFontSizeOverride("font_size", 10);
            }
        }

        private void OnSelectElement(DirGodotCastItem castItem)
        {
            _selectedItem?.SetSelected(false);
            castItem.SetSelected(true);
            _selectedItem = castItem;
            _mediator.RaiseMemberSelected(castItem.LingoMember);

        }

        private void OnItemDoubleClick(DirGodotCastItem castItem)
        {
            if (castItem.LingoMember is ILingoMemberTextBase textMember)
            {
                _mediator.RaiseMemberSelected(textMember);
                _mediator.RaiseMenuSelected(DirectorMenuCodes.TextEditWindow);
            }
        }

        public void FindMember(ILingoMember member)
        {
            if (_castViews.TryGetValue(member.CastLibNum, out var view))
            {
                _tabs.CurrentTab = member.CastLibNum - 1;
                var item = view.FindItem(member);
                if (item != null)
                    view.SelectItem(item);
            }
        }
        public void Activate(int castlibNum)
        {
            //ActiveCastLib = _lingoMovie.CastLib.GetCast(castlibNum);
            //if (ActiveCastLib != null)
            //    CastLibViewer.Show(ActiveCastLib);
        }

        public void Dispose()
        {
            _mediator.Unsubscribe(this);
        }
    }
}
