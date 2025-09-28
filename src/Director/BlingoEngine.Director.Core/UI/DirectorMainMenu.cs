using System;
using System.Collections.Generic;
using System.ComponentModel;
using AbstUI.Commands;
using AbstUI.Components.Buttons;
using AbstUI.Components.Containers;
using AbstUI.Components.Menus;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Tools;
using AbstUI.Windowing;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Compilers.Commands;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Importer.Commands;
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.Core.Remote;
using BlingoEngine.Director.Core.Remote.Commands;
using BlingoEngine.Director.Core.Tools.Commands;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.Events;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Inputs;
using BlingoEngine.Movies;
using BlingoEngine.Net.RNetContracts;

namespace BlingoEngine.Director.Core.UI
{
    /// <summary>
    /// Framework independent implementation of the Director main menu.
    /// </summary>
    public class DirectorMainMenu : DirectorWindow<IDirFrameworkMainMenuWindow>
    {
        private readonly AbstWrapPanel _menuBar;
        private readonly AbstWrapPanel _iconBar;
        private readonly AbstMenu _fileMenu;
        private readonly AbstMenu _editMenu;
        private readonly AbstMenu _insertMenu;
        private readonly AbstMenu _modifyMenu;
        private readonly AbstMenu _controlMenu;
        private readonly AbstMenu _windowMenu;
        private readonly AbstMenu _remoteMenu;
        private readonly AbstButton _fileButton;
        private readonly AbstButton _editButton;
        private readonly AbstButton _insertButton;
        private AbstButton _ModifyButton;
        private readonly AbstButton _ControlButton;
        private readonly AbstButton _windowButton;
        private readonly AbstButton _remoteButton;
        private AbstStateButton _playButton;
        private readonly IAbstWindowManager _windowManager;
        private readonly DirectorProjectManager _projectManager;
        private readonly BlingoPlayer _player;
        private readonly IAbstShortCutManager _shortCutManager;
        private readonly IHistoryManager _historyManager;
        private readonly IAbstCommandManager _commandManager;
        private readonly DirectorRNetServer _rnetServer;
        private readonly DirectorRNetClient _client;
        private readonly IBlingoFrameworkFactory _factory;
        private readonly IRNetConfiguration _rnetConfiguration;
        private readonly List<ShortCutInfo> _shortCuts = new();
        private AbstMenuItem _undoItem;
        private AbstMenuItem _redoItem;
        private AbstMenuItem _settingsItem = null!;
        private AbstMenuItem _hostItem = null!;
        private AbstMenuItem _clientItem = null!;
        private IBlingoMovie? _blingoMovie;
        private List<AbstButton> _topMenuButtons = new List<AbstButton>();
        private List<AbstMenu> _topMenus = new List<AbstMenu>();
        private bool _playPauseState;

        public AbstWrapPanel MenuBar => _menuBar;
        public AbstWrapPanel IconBar => _iconBar;

        public bool PlayPauseState
        {
            get => _playPauseState;
            set
            {
                var hasChanged = _playPauseState != value;
                _playPauseState = value;
                if (hasChanged)
                    SetPlayState(value);
            }
        }

        private class ShortCutInfo
        {
            public AbstShortCutMap Map { get; init; } = null!;
            public string Key { get; init; } = string.Empty;
            public bool Ctrl { get; init; }
            public bool Alt { get; init; }
            public bool Shift { get; init; }
            public bool Meta { get; init; }
        }

        public DirectorMainMenu(IServiceProvider serviceProvider, IAbstWindowManager windowManager, DirectorProjectManager projectManager, BlingoPlayer player, IAbstShortCutManager shortCutManager,
            IHistoryManager historyManager, IDirectorIconManager directorIconManager, IAbstCommandManager commandManager, IBlingoFrameworkFactory factory,
            DirectorRNetServer server, DirectorRNetClient client, IRNetConfiguration configuration) : base(serviceProvider, DirectorMenuCodes.MainMenu)
        {
            _windowManager = windowManager;
            _projectManager = projectManager;
            _player = player;
            _shortCutManager = shortCutManager;
            _historyManager = historyManager;
            _commandManager = commandManager;
            _rnetServer = server;
            _client = client;
            _factory = factory;
            _rnetConfiguration = configuration;

            _rnetServer.ConnectionStatusChanged += OnServerStateChanged;
            _client.ConnectionStatusChanged += OnClientStateChanged;

            _menuBar = factory.CreateWrapPanel(AOrientation.Horizontal, "MenuBar");
            _iconBar = factory.CreateWrapPanel(AOrientation.Horizontal, "IconBar");
            //_iconBar.Height = 20;
            _menuBar.Width = 800;
            _iconBar.Width = 400;
            _iconBar.X = 400;
            _iconBar.Y = 1;
            _menuBar.X = 10;
            _menuBar.Y = 1; 

            _fileMenu = factory.CreateMenu("FileMenu");
            _editMenu = factory.CreateMenu("EditMenu");
            _insertMenu = factory.CreateMenu("InsertMenu");
            _modifyMenu = factory.CreateMenu("ModifyMenu");
            _controlMenu = factory.CreateMenu("ControlMenu");
            _windowMenu = factory.CreateMenu("WindowMenu");
            _remoteMenu = factory.CreateMenu("RemoteMenu");
            // menu buttons
            _fileButton = factory.CreateButton("FileButton", "File");
            _editButton = factory.CreateButton("EditButton", "Edit");
            _insertButton = factory.CreateButton("InsertButton", "Insert");
            _ModifyButton = factory.CreateButton("ModifyButton", "Modify");
            _ControlButton = factory.CreateButton("ControlButton", "Control");
            _windowButton = factory.CreateButton("WindowButton", "Window");
            _remoteButton = factory.CreateButton("RemoteButton", "Remote");

            // icon buttons
            _iconBar
                .ComposeForToolBar()
                .AddButton("CompileButton", "", () => _commandManager.Handle(new CompileProjectCommand()), c => c.IconTexture = directorIconManager.Get(DirectorIcon.Script))
                .AddVLine()
                .AddButton("RewindButton", "", DoRewind, c => c.IconTexture = directorIconManager.Get(DirectorIcon.Rewind))
                .AddStateButton("RewindButton", this, directorIconManager.Get(DirectorIcon.Stop), p => p.PlayPauseState,
                    c =>
                    {
                        c.TextureOff = directorIconManager.Get(DirectorIcon.Play);
                        _playButton = c;
                    })
                .AddVLine()
                .AddButton("Show" + DirectorMenuCodes.StageWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.StageWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowStage))
                .AddButton("Show" + DirectorMenuCodes.CastWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.CastWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowCast))
                .AddButton("Show" + DirectorMenuCodes.ScoreWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.ScoreWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowScore))
                .AddButton("Show" + DirectorMenuCodes.PropertyInspector, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.PropertyInspector), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowProperty))
                .AddVLine()
                .AddButton("Show" + DirectorMenuCodes.PictureEditWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.PictureEditWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowPaint))
                .AddButton("Show" + DirectorMenuCodes.ShapeEditWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.ShapeEditWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowPath))
                .AddButton("Show" + DirectorMenuCodes.TextEditWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.TextEditWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowText))

            //.AddVLine("VLine4", 16, 2)
            //.AddButton("Show"+ DirectorMenuCodes.TextEditWindow, "", () => _windowManager.SwapWindowOpenState(DirectorMenuCodes.TextEditWindow), c => c.IconTexture = directorIconManager.Get(DirectorIcon.WindowText))
            ;

            _topMenus.Add(_fileMenu);
            _topMenus.Add(_editMenu);
            _topMenus.Add(_insertMenu);
            _topMenus.Add(_modifyMenu);
            _topMenus.Add(_controlMenu);
            _topMenus.Add(_windowMenu);
            _topMenus.Add(_remoteMenu);

            _topMenuButtons.Add(_fileButton);
            _topMenuButtons.Add(_editButton);
            _topMenuButtons.Add(_insertButton);
            _topMenuButtons.Add(_ModifyButton);
            _topMenuButtons.Add(_ControlButton);
            _topMenuButtons.Add(_windowButton);
            _topMenuButtons.Add(_remoteButton);


            CallOnAllTopMenuButtons(x =>
            {
                _menuBar.AddItem(x);
            });

            _fileButton.Pressed += () => ShowMenu(_fileMenu, _fileButton);
            _editButton.Pressed += () => { UpdateUndoRedoState(); ShowMenu(_editMenu, _editButton); };
            _insertButton.Pressed += () => ShowMenu(_insertMenu, _insertButton);
            _ModifyButton.Pressed += () => ShowMenu(_modifyMenu, _ModifyButton);
            _ControlButton.Pressed += () => ShowMenu(_controlMenu, _ControlButton);
            _windowButton.Pressed += () => ShowMenu(_windowMenu, _windowButton);
            _remoteButton.Pressed += () =>
            {
                UpdateRemoteMenuActions();
                ShowMenu(_remoteMenu, _remoteButton);
            };

            ComposeMenu(factory);

            _player.ActiveMovieChanged += OnActiveMovieChanged;
            _shortCutManager.ShortCutAdded += OnShortCutAdded;
            _shortCutManager.ShortCutRemoved += OnShortCutRemoved;
            _player.Key.Subscribe(this);

            UpdatePlayButton();
            foreach (var sc in _shortCutManager.GetShortCuts())
                _shortCuts.Add(ParseShortCut(sc));
        }



        public void CallOnAllTopMenuButtons(Action<AbstButton> btnAction)
        {
            foreach (var item in _topMenuButtons)
                btnAction(item);
        }
        public void CallOnAllTopMenus(Action<AbstMenu> action)
        {
            foreach (var item in _topMenus)
                action(item);
        }

        private void ComposeMenu(IBlingoFrameworkFactory factory)
        {
            CreateFileMenu(factory);
            CreateEditMenu(factory);
            CreateInsertMenu(factory);
            CreateModifyMenu(factory);
            CreateControlMenu(factory);
            CreateWindowMenu(factory);
            CreateRemoteMenu(factory);
        }

        private void CreateFileMenu(IBlingoFrameworkFactory factory)
        {
            // File Menu
            var newProject = factory.CreateMenuItem("New...");
            newProject.Activated += OpenNewProjectSettings;
            _fileMenu.AddItem(newProject);

            var load = factory.CreateMenuItem("Load");
            load.Activated += () => _projectManager.LoadMovie();
            _fileMenu.AddItem(load);

            var save = factory.CreateMenuItem("Save");
            save.Activated += () => _projectManager.SaveMovie();
            _fileMenu.AddItem(save);

            var ie = factory.CreateMenuItem("Import/Export");
            ie.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.ImportExportWindow);
            _fileMenu.AddItem(ie);

            var importer = factory.CreateMenuItem("Lingo code importer");
            importer.Activated += () => _commandManager.Handle(new OpenBlingoCodeImporterCommand());
            _fileMenu.AddItem(importer);

            var quit = factory.CreateMenuItem("Quit");
            quit.Activated += () => Environment.Exit(0);
            _fileMenu.AddItem(quit);
        }

        private void OpenNewProjectSettings()
        {
            _windowManager.OpenWindow<DirectorProjectSettingsWindow>(
                DirectorMenuCodes.ProjectSettingsWindow,
                settingsWindow => settingsWindow.PrepareForNewProject());
        }

        private void CreateEditMenu(IBlingoFrameworkFactory factory)
        {
            // Edit Menu
            _undoItem = factory.CreateMenuItem("Undo\tCTRL+Z");
            _undoItem.Activated += () => _historyManager.Undo();
            _editMenu.AddItem(_undoItem);

            _redoItem = factory.CreateMenuItem("Redo\tCTRL+Y");
            _redoItem.Activated += () => _historyManager.Redo();
            _editMenu.AddItem(_redoItem);

            var projectSettings = factory.CreateMenuItem("Project Settings");
            projectSettings.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.ProjectSettingsWindow);
            _editMenu.AddItem(projectSettings);

            var cConverter = factory.CreateMenuItem("Lingo to # Converter");
            cConverter.Activated += () => _commandManager.Handle(new OpenBlingoCSharpConverterCommand());
            _editMenu.AddItem(cConverter);
        }



        private void CreateInsertMenu(IBlingoFrameworkFactory factory)
        {
            var menuItem = factory.CreateMenuItem("Menu TODO");
            menuItem.Activated += () => { };
            _insertMenu.AddItem(menuItem); ;
        }
        private void CreateModifyMenu(IBlingoFrameworkFactory factory)
        {
            var menuItem = factory.CreateMenuItem("Menu TODO");
            menuItem.Activated += () => { };
            _modifyMenu.AddItem(menuItem);
        }
        private void CreateControlMenu(IBlingoFrameworkFactory factory)
        {
            var menuItem = factory.CreateMenuItem("Menu TODO");
            menuItem.Activated += () => { };
            _controlMenu.AddItem(menuItem);
        }
        private void CreateWindowMenu(IBlingoFrameworkFactory factory)
        {
            // Window Menu
            var stage = factory.CreateMenuItem("Stage  \tCTRL+1");
            stage.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.StageWindow);
            _windowMenu.AddItem(stage);

            var cast = factory.CreateMenuItem("Cast  \tCTRL+3");
            cast.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.CastWindow);
            _windowMenu.AddItem(cast);

            var score = factory.CreateMenuItem("Score  \tCTRL+4");
            score.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.ScoreWindow);
            _windowMenu.AddItem(score);

            var text = factory.CreateMenuItem("Text \tCTRL+T");
            text.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.TextEditWindow);
            _windowMenu.AddItem(text);

            var inspector = factory.CreateMenuItem("Property Inspector  \tCTRL+ALT+S");
            inspector.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.PropertyInspector);
            _windowMenu.AddItem(inspector);

            var tools = factory.CreateMenuItem("Tools  \tCTRL+7");
            tools.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.ToolsWindow);
            _windowMenu.AddItem(tools);

            var binary = factory.CreateMenuItem("Binary Viewer");
            binary.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.BinaryViewerWindow);
            _windowMenu.AddItem(binary);

            var binaryV2 = factory.CreateMenuItem("Binary Viewer V2");
            binaryV2.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.BinaryViewerWindowV2);
            _windowMenu.AddItem(binaryV2);

            var paint = factory.CreateMenuItem("Paint  \tCTRL+5");
            paint.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.PictureEditWindow);
            _windowMenu.AddItem(paint);

            var behaviorInspectorWindow = factory.CreateMenuItem("Behavior Inspector \tCTRL+5");
            behaviorInspectorWindow.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.BehaviorInspectorWindow);
            _windowMenu.AddItem(behaviorInspectorWindow);
        }

        private void CreateRemoteMenu(IBlingoFrameworkFactory factory)
        {
            _settingsItem = factory.CreateMenuItem("Settings");
            _settingsItem.Activated += () =>
            {
                _commandManager.Handle(new OpenRNetSettingsCommand());
            };

            _hostItem = factory.CreateMenuItem(_rnetServer.IsEnabled ? "Stop Host" : "Start Host");
            _hostItem.Activated += () =>
                _commandManager.Handle(_rnetServer.IsEnabled
                    ? new DisconnectRNetServerCommand()
                    : new ConnectRNetServerCommand());

            _clientItem = factory.CreateMenuItem(_client.IsConnected ? "Disconnect from Host" : "Connect to Host");
            _clientItem.Activated += () =>
                _commandManager.Handle(_client.IsConnected
                    ? new DisconnectRNetClientCommand()
                    : new ConnectRNetClientCommand());

            UpdateRemoteMenuActions();
        }

        private void UpdateRemoteMenuActions()
        {
            _remoteMenu.ClearItems();
            _remoteMenu.AddItem(_settingsItem);

            if (_rnetConfiguration.RemoteRole == RNetRemoteRole.Host)
            {
                _hostItem.Name = _rnetServer.IsEnabled ? "Stop Host" : "Start Host";
                _hostItem.Enabled = _rnetServer.IsEnabled || _rnetServer.CanExecute(new ConnectRNetServerCommand());
                _remoteMenu.AddItem(_hostItem);
            }
            else
            {
                _clientItem.Name = _client.IsConnected ? "Disconnect from Host" : "Connect to Host";
                var canConnect = _client.CanExecute(new ConnectRNetClientCommand());
                var canDisconnect = _client.CanExecute(new DisconnectRNetClientCommand());
                _clientItem.Enabled = canConnect || canDisconnect;
                _remoteMenu.AddItem(_clientItem);
            }
        }

        private void OnServerStateChanged(BlingoNetConnectionState state)
        {
            _player.RunOnUIThread(() =>
            {
                _hostItem.Name = state == BlingoNetConnectionState.Connected ? "Stop Host" : "Start Host";
                if (_rnetConfiguration.RemoteRole == RNetRemoteRole.Host)
                    UpdateRemoteMenuActions();
            });
        }

        private void OnClientStateChanged(BlingoNetConnectionState state)
        {
            _player.RunOnUIThread(() =>
            {
                _clientItem.Name = state == BlingoNetConnectionState.Connected ? "Disconnect from Host" : "Connect to Host";
                if (_rnetConfiguration.RemoteRole == RNetRemoteRole.Client)

                    UpdateRemoteMenuActions();
            });
        }


        private void OnActiveMovieChanged(IBlingoMovie? movie)
        {
            if (_blingoMovie != null)
            {
                _blingoMovie.PlayStateChanged -= OnPlayStateChanged;
            }
            _blingoMovie = movie;
            if (_blingoMovie != null)
            {
                _blingoMovie.PlayStateChanged += OnPlayStateChanged;
            }
            UpdatePlayButton();
        }
        private void DoRewind()
        {
            _blingoMovie?.GoTo(1);
        }

        private void OnPlayStateChanged(bool isPlaying) => UpdatePlayButton();

        private bool _playPauseFromEvent;
        private void UpdatePlayButton()
        {
            _playPauseFromEvent = true;
            PlayPauseState = _blingoMovie != null && _blingoMovie.IsPlaying;
            _playButton.IsOn = PlayPauseState;
            _playPauseFromEvent = false;
        }
        private void SetPlayState(bool state)
        {
            if (_playPauseFromEvent) return; // Prevent recursive calls from the button state change
            if (_blingoMovie == null) return;
            if (_blingoMovie.IsPlaying && state) return;
            if (_blingoMovie.IsPlaying)
                _blingoMovie.Halt();
            else
                _blingoMovie.Play();
        }

        private void OnShortCutAdded(AbstShortCutMap map)
            => _shortCuts.Add(ParseShortCut(map));

        private void OnShortCutRemoved(AbstShortCutMap map)
            => _shortCuts.RemoveAll(s => s.Map == map);

        private ShortCutInfo ParseShortCut(AbstShortCutMap map)
        {
            bool ctrl = false, alt = false, shift = false, meta = false;
            string key = string.Empty;
            var parts = map.KeyCombination.Split('+');
            foreach (var p in parts)
            {
                var token = p.Trim();
                if (token.Equals("CTRL", StringComparison.OrdinalIgnoreCase)) ctrl = true;
                else if (token.Equals("ALT", StringComparison.OrdinalIgnoreCase)) alt = true;
                else if (token.Equals("SHIFT", StringComparison.OrdinalIgnoreCase)) shift = true;
                else if (token.Equals("CMD", StringComparison.OrdinalIgnoreCase) || token.Equals("META", StringComparison.OrdinalIgnoreCase)) meta = true;
                else key = token;
            }
            return new ShortCutInfo { Map = map, Key = key.ToUpperInvariant(), Ctrl = ctrl, Alt = alt, Shift = shift, Meta = meta };
        }

        private void ShowMenu(AbstMenu menu, AbstButton button)
        {
            menu.PositionPopup(button);
            menu.Popup();
        }

        public void UpdateUndoRedoState()
        {
            _undoItem.Enabled = _historyManager.CanUndo;
            _redoItem.Enabled = _historyManager.CanRedo;
        }

        protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
        {
            base.OnInit(frameworkWindow);
            CallOnAllTopMenus(menu => Framework.RegisterTopMenu(menu));
        }



        protected override void OnDispose()
        {
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            _shortCutManager.ShortCutAdded -= OnShortCutAdded;
            _shortCutManager.ShortCutRemoved -= OnShortCutRemoved;
            _player.Key.Unsubscribe(this);
            _rnetServer.ConnectionStatusChanged -= OnServerStateChanged;
            _client.ConnectionStatusChanged -= OnClientStateChanged;
            base.OnDispose();
        }

        protected override void OnRaiseKeyDown(AbstKeyEvent key)
        {
            var label = key.Key.ToUpperInvariant();
            bool ctrl = key.ControlDown;
            bool alt = key.OptionDown;
            bool shift = key.ShiftDown;
            bool meta = key.CommandDown;

            foreach (var sc in _shortCuts)
            {
                if (sc.Key == label && sc.Ctrl == ctrl && sc.Alt == alt && sc.Shift == shift && sc.Meta == meta)
                {
                    _shortCutManager.Execute(sc.Map.KeyCombination);
                    break;
                }
            }
        }

        protected override void OnRaiseKeyUp(AbstKeyEvent key) { }
    }
}

