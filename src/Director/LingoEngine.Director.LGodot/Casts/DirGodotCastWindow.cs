using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Movies;
using LingoEngine.Members;
using LingoEngine.Casts;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Core;
using LingoEngine.Director.LGodot;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Core;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastWindow : BaseGodotWindow, IDisposable, IHasFindMemberEvent, IDirFrameworkCastWindow
    {
        private readonly TabContainer _tabs;
        

        private readonly IDirectorEventMediator _mediator;
        private readonly DirectorStyle _style;
        private readonly Dictionary<int, DirGodotCastView> _castViews = new();
        private DirGodotCastItem? _selectedItem;
        private ILingoPlayer _player;
        private readonly ILingoCommandManager _commandManager;

        public ILingoCast? ActiveCastLib { get; private set; }

        public DirGodotCastWindow(IDirectorEventMediator mediator, DirectorStyle style, DirectorCastWindow directorCastWindow, ILingoPlayer player, IDirGodotWindowManager windowManager, ILingoCommandManager commandManager)
            : base(DirectorMenuCodes.CastWindow, "Cast", windowManager)
        {
            _mediator = mediator;
            _style = style;
            directorCastWindow.Init(this);
            _player = player;
            _commandManager = commandManager;
            _player.ActiveMovieChanged += OnActiveMovieChanged;
            _mediator.Subscribe(this);

            Size = new Vector2(360, 620);
            CustomMinimumSize = Size;
            
            _tabs = new TabContainer();
            _tabs.Position = new Vector2(0, TitleBarHeight );

            InitTabs();
            AddChild(_tabs);
        }


        protected override void OnResizing(Vector2 size)
        {
            base.OnResizing(size);
            _tabs.Size = new Vector2(Size.X - 10, Size.Y- TitleBarHeight -30);
        }

        private void OnActiveMovieChanged(ILingoMovie? movie)
        {
            SetActiveMovie(movie as LingoMovie);
        }

        public void SetActiveMovie(ILingoMovie? lingoMovie)
        {
            for (int i = _tabs.GetChildCount() - 1; i >= 0; i--)
            {
                var child = _tabs.GetChild(i);
                _tabs.RemoveChild(child);
                child.QueueFree();
            }
            if (lingoMovie == null) return;
            foreach (var cast in lingoMovie.CastLib.GetAll())
            {
                var castLibViewer = new DirGodotCastView(OnSelectElement, _style, _commandManager);
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

        public new void Dispose()
        {
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            _mediator.Unsubscribe(this);
            base.Dispose();
        }

       
    }
}
