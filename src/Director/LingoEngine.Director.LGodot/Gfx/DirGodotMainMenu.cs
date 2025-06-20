using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Movies;
using LingoEngine.Director.Core;
using LingoEngine.Director.Core.Menus;

namespace LingoEngine.Director.LGodot;

/// <summary>
/// Top application menu bar with a follow-up icon bar.
/// </summary>
internal partial class DirGodotMainMenu : Control
{
    private readonly HBoxContainer _menuBar = new HBoxContainer();
    private readonly MenuButton _fileMenu = new MenuButton() { Text = "File" };
    private readonly MenuButton _editMenu = new MenuButton() { Text = "Edit" };
    private readonly MenuButton _WindowMenu = new MenuButton() { Text = "Window" };
    private readonly HBoxContainer _iconBar = new HBoxContainer();
    private readonly IDirectorWindowManager _windowManager;
    private readonly DirectorProjectManager _projectManager;
    private readonly Button _rewindButton;
    private readonly Button _playButton;
    private readonly ILingoMovie _lingoMovie;

    public DirGodotMainMenu(IDirectorWindowManager windowManager, DirectorProjectManager projectManager, ILingoMovie lingoMovie)
    {
        _windowManager = windowManager;
        _projectManager = projectManager;
        _lingoMovie = lingoMovie;
        _lingoMovie.PlayStateChanged += OnPlayStateChanged;

        AddChild(_menuBar);
        _menuBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        ComposeMenu();

        AddChild(_iconBar);
        _iconBar.Position = new Vector2(300, 0);
        _iconBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _rewindButton = new Button { Text = "|<" };
        _rewindButton.CustomMinimumSize = new Vector2(20, 16);
        _rewindButton.Position = new Vector2(3, 2);
        _rewindButton.Pressed += () => _lingoMovie.GoTo(1);
        _iconBar.AddChild(_rewindButton);

        _playButton = new Button { Text = "Play", };
        _playButton.CustomMinimumSize = new Vector2(40, 16);
        _playButton.Position = new Vector2(26, 2);
        _playButton.Pressed += OnPlayPressed;
        _iconBar.AddChild(_playButton);

        UpdatePlayButton();
    }

    private void ComposeMenu()
    {
        // FileMenu
        _menuBar.AddChild(_fileMenu);
        var popupFile = _fileMenu.GetPopup();
        popupFile.AddItem("Load", 1);
        popupFile.AddItem("Save", 2);
        popupFile.AddItem("Quit", 3);
        popupFile.IdPressed += id =>
        {
            switch (id)
            {
                case 1: _projectManager.LoadMovie(); break;
                case 2: _projectManager.SaveMovie(); break;
                case 3:
                    // TODO: check project for unsaved changes before quitting
                    GetTree().Quit();
                    break;
            }
        };

        _menuBar.AddChild(_editMenu);
        var popupEdit = _editMenu.GetPopup();
        popupEdit.AddItem("Project Settings", 20);
        popupEdit.IdPressed += id =>
        {
            if (id == 20) _windowManager.OpenWindow(DirectorMenuCodes.ProjectSettingsWindow);
        };

        // Window Menu
        _menuBar.AddChild(_WindowMenu);
        var popupWindow = _WindowMenu.GetPopup();
        //popupWindow.AddItem("Script", 5);
        popupWindow.AddItem("Stage\tCtrl+1", 6);
        //popupWindow.AddItem("Control Panel\tCtrl+2", 7);
        popupWindow.AddItem("Cast\tCtrl+3", 8);
        popupWindow.AddItem("Score\tCtrl+4", 9);
        popupWindow.AddItem("Property Inspector\tCtrl+Alt+S", 15);
        popupWindow.AddItem("Tools\tCtrl+7", 16);
        popupWindow.AddItem("Binary Viewer", 17);
        popupWindow.IdPressed += id =>
        {
            switch (id)
            {
                case 5: _windowManager.OpenWindow(DirectorMenuCodes.ScriptWindow); break;
                case 6: _windowManager.OpenWindow(DirectorMenuCodes.StageWindow); break;
                case 7: _windowManager.OpenWindow(DirectorMenuCodes.ControlPanel); break;
                case 8: _windowManager.OpenWindow(DirectorMenuCodes.CastWindow); break;
                case 9: _windowManager.OpenWindow(DirectorMenuCodes.ScoreWindow); break;
                case 15: _windowManager.OpenWindow(DirectorMenuCodes.PropertyInspector); break;
                case 16: _windowManager.OpenWindow(DirectorMenuCodes.ToolsWindow); break;
                case 17: _windowManager.OpenWindow(DirectorMenuCodes.BinaryViewerWindow); break;
                default:
                    break;
            }
        };
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

    public HBoxContainer IconBar => _iconBar;

    protected override void Dispose(bool disposing)
    {
        _lingoMovie.PlayStateChanged -= OnPlayStateChanged;
        base.Dispose(disposing);
    }
}
