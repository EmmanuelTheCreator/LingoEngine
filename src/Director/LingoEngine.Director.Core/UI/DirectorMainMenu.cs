using LingoEngine.Core;
using LingoEngine.Director.Core.Menus;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Windows;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Movies;
using LingoEngine.Inputs;
using System.Collections.Generic;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Gfx
{
    /// <summary>
    /// Framework independent implementation of the Director main menu.
    /// </summary>
    public class DirectorMainMenu : DirectorWindow<IDirFrameworkMainMenuWindow>, ILingoKeyEventHandler
    {
        private readonly LingoGfxWrapPanel _menuBar;
        private readonly LingoGfxWrapPanel _iconBar;
        private readonly LingoGfxMenu _fileMenu;
        private readonly LingoGfxMenu _editMenu;
        private readonly LingoGfxMenu _windowMenu;
        private readonly LingoGfxButton _fileButton;
        private readonly LingoGfxButton _editButton;
        private readonly LingoGfxButton _windowButton;
        private readonly LingoGfxButton _rewindButton;
        private readonly LingoGfxButton _playButton;
        private readonly IDirectorWindowManager _windowManager;
        private readonly DirectorProjectManager _projectManager;
        private readonly LingoPlayer _player;
        private readonly IDirectorShortCutManager _shortCutManager;
        private readonly IHistoryManager _historyManager;
        private readonly List<ShortCutInfo> _shortCuts = new();
        private LingoGfxMenuItem _undoItem;
        private LingoGfxMenuItem _redoItem;
        private ILingoMovie? _lingoMovie;

        private class ShortCutInfo
        {
            public DirectorShortCutMap Map { get; init; } = null!;
            public string Key { get; init; } = string.Empty;
            public bool Ctrl { get; init; }
            public bool Alt { get; init; }
            public bool Shift { get; init; }
            public bool Meta { get; init; }
        }

        public DirectorMainMenu(IDirectorWindowManager windowManager,
            DirectorProjectManager projectManager,
            LingoPlayer player,
            IDirectorShortCutManager shortCutManager,
            IHistoryManager historyManager,
            ILingoFrameworkFactory factory)
        {
            _windowManager = windowManager;
            _projectManager = projectManager;
            _player = player;
            _shortCutManager = shortCutManager;
            _historyManager = historyManager;

            _menuBar = factory.CreateWrapPanel(LingoOrientation.Horizontal, "MenuBar");
            _iconBar = factory.CreateWrapPanel(LingoOrientation.Horizontal, "IconBar");
            _fileMenu = factory.CreateMenu("FileMenu");
            _editMenu = factory.CreateMenu("EditMenu");
            _windowMenu = factory.CreateMenu("WindowMenu");
            _fileButton = factory.CreateButton("FileButton", "File");
            _editButton = factory.CreateButton("EditButton", "Edit");
            _windowButton = factory.CreateButton("WindowButton", "Window");
            _rewindButton = factory.CreateButton("RewindButton", "|<");
            _playButton = factory.CreateButton("PlayButton", "Play");

            _menuBar.AddChild(_fileButton);
            _menuBar.AddChild(_editButton);
            _menuBar.AddChild(_windowButton);
            _fileButton.Pressed += () => ShowMenu(_fileMenu, _fileButton);
            _editButton.Pressed += () => { UpdateUndoRedoState(); ShowMenu(_editMenu, _editButton); };
            _windowButton.Pressed += () => ShowMenu(_windowMenu, _windowButton);

            ComposeMenu(factory);

            _playButton.Pressed += OnPlayPressed;
            _player.ActiveMovieChanged += OnActiveMovieChanged;
            _shortCutManager.ShortCutAdded += OnShortCutAdded;
            _shortCutManager.ShortCutRemoved += OnShortCutRemoved;
            _player.Key.Subscribe(this);

            UpdatePlayButton();
            foreach (var sc in _shortCutManager.GetShortCuts())
                _shortCuts.Add(ParseShortCut(sc));
        }

        private void ComposeMenu(ILingoFrameworkFactory factory)
        {
            // File Menu
            var load = factory.CreateMenuItem("Load");
            load.Activated += () => _projectManager.LoadMovie();
            _fileMenu.AddItem(load);

            var save = factory.CreateMenuItem("Save");
            save.Activated += () => _projectManager.SaveMovie();
            _fileMenu.AddItem(save);

            var ie = factory.CreateMenuItem("Import/Export");
            ie.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.ImportExportWindow);
            _fileMenu.AddItem(ie);

            var quit = factory.CreateMenuItem("Quit");
            quit.Activated += () => System.Environment.Exit(0);
            _fileMenu.AddItem(quit);

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

            var paint = factory.CreateMenuItem("Paint  \tCTRL+5");
            paint.Activated += () => _windowManager.OpenWindow(DirectorMenuCodes.PictureEditWindow);
            _windowMenu.AddItem(paint);
        }

        private void OnActiveMovieChanged(ILingoMovie? movie)
        {
            if (_lingoMovie != null)
            {
                _lingoMovie.PlayStateChanged -= OnPlayStateChanged;
                _rewindButton.Pressed -= () => _lingoMovie.GoTo(1);
            }
            _lingoMovie = movie;
            if (_lingoMovie != null)
            {
                _lingoMovie.PlayStateChanged += OnPlayStateChanged;
                _rewindButton.Pressed += () => _lingoMovie.GoTo(1);
            }
            UpdatePlayButton();
        }

        private void OnPlayPressed()
        {
            if (_lingoMovie == null) return;
            if (_lingoMovie.IsPlaying)
                _lingoMovie.Halt();
            else
                _lingoMovie.Play();
        }

        private void OnPlayStateChanged(bool isPlaying) => UpdatePlayButton();

        private void UpdatePlayButton()
        {
            _playButton.Text = _lingoMovie != null && _lingoMovie.IsPlaying ? "Stop" : "Play";
        }

        private void OnShortCutAdded(DirectorShortCutMap map)
            => _shortCuts.Add(ParseShortCut(map));

        private void OnShortCutRemoved(DirectorShortCutMap map)
            => _shortCuts.RemoveAll(s => s.Map == map);

        private ShortCutInfo ParseShortCut(DirectorShortCutMap map)
        {
            bool ctrl = false, alt = false, shift = false, meta = false;
            string key = string.Empty;
            var parts = map.KeyCombination.Split('+');
            foreach (var p in parts)
            {
                var token = p.Trim();
                if (token.Equals("CTRL", System.StringComparison.OrdinalIgnoreCase)) ctrl = true;
                else if (token.Equals("ALT", System.StringComparison.OrdinalIgnoreCase)) alt = true;
                else if (token.Equals("SHIFT", System.StringComparison.OrdinalIgnoreCase)) shift = true;
                else if (token.Equals("CMD", System.StringComparison.OrdinalIgnoreCase) || token.Equals("META", System.StringComparison.OrdinalIgnoreCase)) meta = true;
                else key = token;
            }
            return new ShortCutInfo { Map = map, Key = key.ToUpperInvariant(), Ctrl = ctrl, Alt = alt, Shift = shift, Meta = meta };
        }

        private void ShowMenu(LingoGfxMenu menu, LingoGfxButton button)
        {
            menu.PositionPopup(button);
            menu.Popup();
        }

        public void UpdateUndoRedoState()
        {
            _undoItem.Enabled = _historyManager.CanUndo;
            _redoItem.Enabled = _historyManager.CanRedo;
        }

        public LingoGfxWrapPanel MenuBar => _menuBar;
        public LingoGfxWrapPanel IconBar => _iconBar;
        public LingoGfxMenu FileMenu => _fileMenu;
        public LingoGfxMenu EditMenu => _editMenu;
        public LingoGfxMenu WindowMenu => _windowMenu;
        public LingoGfxButton FileButton => _fileButton;
        public LingoGfxButton EditButton => _editButton;
        public LingoGfxButton WindowButton => _windowButton;
        public LingoGfxButton RewindButton => _rewindButton;
        public LingoGfxButton PlayButton => _playButton;

        public bool IsOpen => false;

        public override void Dispose()
        {
            _player.ActiveMovieChanged -= OnActiveMovieChanged;
            _shortCutManager.ShortCutAdded -= OnShortCutAdded;
            _shortCutManager.ShortCutRemoved -= OnShortCutRemoved;
            _player.Key.Unsubscribe(this);
            base.Dispose();
        }

        public void RaiseKeyDown(LingoKey key)
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

        public void RaiseKeyUp(LingoKey key) { }
    }
}
