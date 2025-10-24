using System;
using System.Collections.Generic;
using AbstUI.Commands;
using AbstUI.Components.Containers;
using AbstUI.Inputs;
using AbstUI.Windowing;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Events;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.Director.Core.Casts
{
    public class DirectorCastWindow : DirectorWindow<IDirFrameworkCastWindow> , IHasFindMemberEvent
    {
        private readonly IDirectorEventMediator _mediator;
        private readonly AbstTabContainer _tabs;
        private readonly BlingoPlayer _player;
        private readonly Dictionary<string, DirCastTab> _tabMap = new();
        private readonly IAbstCommandManager _commandManager;
        private readonly IDirectorIconManager _iconManager;
        private readonly ILogger<DirectorCastWindow> _logger;
        private IBlingoMember? _selected;
        private IAbstMouseSubscription? _mouseSub;
        private IBlingoMovie? _movie;

        public AbstTabContainer TabContainer => _tabs;
        public IBlingoMember? SelectedMember => _selected;

        public DirectorCastWindow(
            IServiceProvider serviceProvider,
            IBlingoFrameworkFactory factory,
            IDirectorEventMediator mediator,
            IAbstCommandManager commandManager,
            IDirectorIconManager iconManager,
            IBlingoPlayer player, ILogger<DirectorCastWindow> logger) : base(serviceProvider, DirectorMenuCodes.CastWindow)
        {
            _player = (BlingoPlayer)player;
            _player.ActiveMovieChanged += OnActiveMovieChanged;
            _mediator = mediator;
            _commandManager = commandManager;
            _iconManager = iconManager;
            _logger = logger;
            MinimumWidth = 360;
            MinimumHeight = 100;
            Width = 370;
            Height = 620;
            X = 830;
            Y = 22;
            _tabs = factory.CreateTabContainer("CastTabs");
            //_tabs.Height = Height;
            _mediator.Subscribe(this);

        }
       
        protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
        {
            base.OnInit(frameworkWindow);
            Title = "Cast";
            _mouseSub = MouseT.OnMouseEvent(OnMouseEvent);
            Content = _tabs;
        }

        protected override void OnDispose()
        {
            _mediator.Unsubscribe(this);
            _mouseSub?.Release();
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            ClearTabs();
            base.OnDispose();
        }

        private void OnActiveMovieChanged(IBlingoMovie? movie)
        {
            if (movie == _movie)
                return;
            _movie = movie;
            SetActiveMovie(movie);
        }

        private void SetActiveMovie(IBlingoMovie? movie)
        {
            ClearTabs();
            if (movie == null)
                return;

            foreach (var cast in movie.CastLib.GetAll())
            {
                var tab = new DirCastTab(
                    _factory,
                    cast,
                    _iconManager,
                    _commandManager,
                    _mediator,
                    _player,
                    () => CreateContextMenu(),_logger);
                tab.SetViewportSize((int)_tabs.Width, (int)_tabs.Height);
                _tabs.AddTab(tab.TabItem);
                _tabMap[tab.TabItem.Title] = tab;
                tab.MemberSelected += (m, i) => OnMemberSelected(tab, m, i);
                tab.LoadAllMembers();
            }
        }

        private void ClearTabs()
        {
            foreach (var tab in _tabMap.Values)
            {
                tab.Dispose();
            }
            _tabMap.Clear();
            _tabs.ClearTabs();
        }
        protected override void OnResizing(bool firstLoad, int width, int height)
        {
            _tabs.Width = width;
            _tabs.Height = height-10;
            foreach (var tab in _tabMap.Values)
                tab.SetViewportSize(width, height-10);
        }

        private void OnMouseEvent(AbstMouseEvent e)
        {
            var tabName = _tabs.SelectedTabName;
            if (tabName == null) return;
            if (_tabMap.TryGetValue(tabName, out var tab))
                tab.HandleMouseEvent(e);
        }

        private void OnMemberSelected(DirCastTab source, IBlingoMember member, IDirCastItem item)
        {
            _selected = member;
            foreach (var kv in _tabMap)
                if (kv.Value != source)
                    kv.Value.Select(null);
            _mediator.RaiseMemberSelected(member);
        }

        public void FindMember(IBlingoMember member)
        {
            foreach (var kv in _tabMap)
            {
                int idx = kv.Value.IndexOfMember(member);
                if (idx >= 0)
                {
                    _tabs.SelectTabByName(kv.Key);
                    kv.Value.ScrollToIndex(idx);
                    _selected = member;
                    kv.Value.Select(kv.Value.GetItem(idx));
                    break;
                }
            }
        }
    }
}


