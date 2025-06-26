using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Movies;
using LingoEngine.Director.Core;
using LingoEngine.Director.Core.Menus;
using LingoEngine.Core;
using LingoEngine.Director.Core.Gfx;
using LingoEngine.Director.Core.Stages;
using System.Collections.Generic;

namespace LingoEngine.Director.LGodot;

/// <summary>
/// Top application menu bar with a follow-up icon bar.
/// </summary>
internal partial class DirGodotMainMenu : Control, IDirFrameworkMainMenuWindow
{
    private readonly HBoxContainer _menuBar = new HBoxContainer();
    private readonly MenuButton _fileMenu = new MenuButton() { Text = "File" };
    private readonly MenuButton _editMenu = new MenuButton() { Text = "Edit" };
    private readonly MenuButton _WindowMenu = new MenuButton() { Text = "Window" };
    private readonly HBoxContainer _iconBar = new HBoxContainer();
    private readonly IDirectorWindowManager _windowManager;
    private readonly DirectorProjectManager _projectManager;
    private readonly LingoPlayer _player;
    private readonly IDirectorShortCutManager _shortCutManager;
    private readonly IHistoryManager _historyManager;
    private readonly List<ShortCutInfo> _shortCuts = new();
    private readonly Button _rewindButton;
    private readonly Button _playButton;
    private int _undoIndex;
    private int _redoIndex;
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

    public DirGodotMainMenu(IDirectorWindowManager windowManager, DirectorProjectManager projectManager, LingoPlayer player, IDirectorShortCutManager shortCutManager, IHistoryManager historyManager, DirectorMainMenu directorMainMenu)
    {
        _windowManager = windowManager;
        _projectManager = projectManager;
        _player = player;
        _shortCutManager = shortCutManager;
        _historyManager = historyManager;
        _player.ActiveMovieChanged += OnActiveMovieChanged;
        _shortCutManager.ShortCutAdded += OnShortCutAdded;
        _shortCutManager.ShortCutRemoved += OnShortCutRemoved;
        directorMainMenu.Init(this);

        AddChild(_menuBar);
        _menuBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        ComposeMenu();

        AddChild(_iconBar);
        _iconBar.Position = new Vector2(300, 0);
        _iconBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _rewindButton = new Button { Text = "|<" };
        _rewindButton.CustomMinimumSize = new Vector2(20, 16);
        _rewindButton.Position = new Vector2(3, 2);
        _iconBar.AddChild(_rewindButton);

        _playButton = new Button { Text = "Play", };
        _playButton.CustomMinimumSize = new Vector2(40, 16);
        _playButton.Position = new Vector2(26, 2);
        _playButton.Pressed += OnPlayPressed;
        _iconBar.AddChild(_playButton);

        UpdatePlayButton();

        foreach (var sc in _shortCutManager.GetShortCuts())
            _shortCuts.Add(ParseShortCut(sc));
    }

    private void ComposeMenu()
    {
        // FileMenu
        _menuBar.AddChild(_fileMenu);
        var popupFile = _fileMenu.GetPopup();
        popupFile.AddItem("Load", 1);
        popupFile.AddItem("Save", 2);
        popupFile.AddItem("Import/Export", 4);
        popupFile.AddItem("Quit", 3);
        popupFile.IdPressed += id =>
        {
            switch (id)
            {
                case 1: _projectManager.LoadMovie(); break;
                case 2: _projectManager.SaveMovie(); break;
                case 4: _windowManager.OpenWindow(DirectorMenuCodes.ImportExportWindow); break;
                case 3:
                    // TODO: check project for unsaved changes before quitting
                    GetTree().Quit();
                    break;
            }
        };

        _menuBar.AddChild(_editMenu);
        var popupEdit = _editMenu.GetPopup();
        _undoIndex = popupEdit.ItemCount;
        popupEdit.AddItem("Undo\tCTRL+Z", 1);
        _redoIndex = popupEdit.ItemCount;
        popupEdit.AddItem("Redo\tCTRL+Y", 2);
        popupEdit.SetItemDisabled(_undoIndex, !_historyManager.CanUndo);
        popupEdit.SetItemDisabled(_redoIndex, !_historyManager.CanRedo);
        popupEdit.AddSeparator();
        popupEdit.AddItem("Project Settings", 20);
        popupEdit.AboutToPopup += () =>
        {
            popupEdit.SetItemDisabled(_undoIndex, !_historyManager.CanUndo);
            popupEdit.SetItemDisabled(_redoIndex, !_historyManager.CanRedo);
        };
        popupEdit.IdPressed += id =>
        {
            switch (id)
            {
                case 1: _historyManager.Undo(); break;
                case 2: _historyManager.Redo(); break;
                case 20: _windowManager.OpenWindow(DirectorMenuCodes.ProjectSettingsWindow); break;
            }
        };

        // Window Menu
        _menuBar.AddChild(_WindowMenu);
        var popupWindow = _WindowMenu.GetPopup();
        //popupWindow.AddItem("Script", 5);
        popupWindow.AddItem("Stage  \tCTRL+1", 6);
        //popupWindow.AddItem("Control Panel  \tCTRL+2", 7);
        popupWindow.AddItem("Cast  \tCTRL+3", 8);
        popupWindow.AddItem("Score  \tCTRL+4", 9);
        popupWindow.AddItem("Text \tCTRL+T", 13);
        popupWindow.AddItem("Property Inspector  \tCTRL+ALT+S", 15);
        popupWindow.AddItem("Tools  \tCTRL+7", 16);
        popupWindow.AddItem("Binary Viewer", 17);
        popupWindow.AddItem("Paint  \tCTRL+5", 18);
        popupWindow.IdPressed += id =>
        {
            switch (id)
            {
                case 5: _windowManager.OpenWindow(DirectorMenuCodes.ScriptWindow); break;
                case 6: _windowManager.OpenWindow(DirectorMenuCodes.StageWindow); break;
                case 7: _windowManager.OpenWindow(DirectorMenuCodes.ControlPanel); break;
                case 8: _windowManager.OpenWindow(DirectorMenuCodes.CastWindow); break;
                case 9: _windowManager.OpenWindow(DirectorMenuCodes.ScoreWindow); break;
                case 13: _windowManager.OpenWindow(DirectorMenuCodes.TextEditWindow); break;
                case 15: _windowManager.OpenWindow(DirectorMenuCodes.PropertyInspector); break;
                case 16: _windowManager.OpenWindow(DirectorMenuCodes.ToolsWindow); break;
                case 17: _windowManager.OpenWindow(DirectorMenuCodes.BinaryViewerWindow); break;
                case 18: _windowManager.OpenWindow(DirectorMenuCodes.PictureEditWindow); break;
                default:
                    break;
            }
        };
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
    }

    private void OnPlayPressed()
    {
        if (_lingoMovie == null) return;
        if (_lingoMovie.IsPlaying)
            _lingoMovie.Halt();
        else
            _lingoMovie.Play();
    }

    private void OnPlayStateChanged(bool isPlaying)
    {
        UpdatePlayButton();
    }

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

    public HBoxContainer IconBar => _iconBar;

    public bool IsOpen => throw new NotImplementedException();

    protected override void Dispose(bool disposing)
    {
        OnActiveMovieChanged(null);
        _player.ActiveMovieChanged -= OnActiveMovieChanged;
        _shortCutManager.ShortCutAdded -= OnShortCutAdded;
        _shortCutManager.ShortCutRemoved -= OnShortCutRemoved;
        base.Dispose(disposing);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey k && k.Pressed && !k.Echo)
        {
            var label = k.KeyLabel.ToString().ToUpperInvariant();
            bool ctrl = Input.IsKeyPressed(Key.Ctrl);
            bool alt = Input.IsKeyPressed(Key.Alt);
            bool shift = Input.IsKeyPressed(Key.Shift);
            bool meta = Input.IsKeyPressed(Key.Meta);

            foreach (var sc in _shortCuts)
            {
                if (sc.Key == label && sc.Ctrl == ctrl && sc.Alt == alt && sc.Shift == shift && sc.Meta == meta)
                {
                    _shortCutManager.Execute(sc.Map.KeyCombination);
                    GetViewport().SetInputAsHandled();
                    break;
                }
            }
        }
    }

    public void OpenWindow()
    {
        // not allowed
    }

    public void CloseWindow()
    {
        // not allowed
    }

    public void MoveWindow(int x, int y)
    {
        // not allowed
    }
}
