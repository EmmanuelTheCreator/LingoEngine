using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Movies;

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
    private IDirectorEventMediator _mediator;
    private readonly Button _rewindButton;
    private readonly Button _playButton;
    private readonly ILingoMovie _lingoMovie;

    public DirGodotMainMenu(IDirectorEventMediator mediator, ILingoMovie lingoMovie)
    {
        _mediator = mediator;
        _lingoMovie = lingoMovie;
        _lingoMovie.PlayStateChanged += OnPlayStateChanged;

        AddChild(_menuBar);
        _menuBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        ComposeMenu(mediator);

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

    private void ComposeMenu(IDirectorEventMediator mediator)
    {
        // FileMenu
        _menuBar.AddChild(_fileMenu);
        var popupFile = _fileMenu.GetPopup();
        popupFile.AddItem("Save", 2);
        popupFile.AddItem("Load", 3);
        popupFile.AddItem("Quit", 1);
        popupFile.IdPressed += id =>
        {
            switch (id)
            {
                case 1:
                    // TODO: check project for unsaved changes before quitting
                    GetTree().Quit();
                    break;
                case 2: mediator.RaiseMenuSelected(MenuCodes.FileSave); break;
                case 3: mediator.RaiseMenuSelected(MenuCodes.FileLoad); break;
            }
        };

        _menuBar.AddChild(_editMenu);
        var popupEdit = _editMenu.GetPopup();
        popupEdit.AddItem("Project Settings", 20);
        popupEdit.IdPressed += id =>
        {
            if (id == 20) mediator.RaiseMenuSelected(MenuCodes.ProjectSettingsWindow);
        };

        // Window Menu
        _menuBar.AddChild(_WindowMenu);
        var popupWindow = _WindowMenu.GetPopup();
        //popupWindow.AddItem("Script", 5);
        popupWindow.AddItem("Stage", 6);
        //popupWindow.AddItem("Control Panel", 7);
        popupWindow.AddItem("Cast", 8);
        popupWindow.AddItem("Score", 9);
        popupWindow.AddItem("Object Inspector", 15);
        popupWindow.IdPressed += id =>
        {
            switch (id)
            {
                case 5: mediator.RaiseMenuSelected(MenuCodes.ScriptWindow); break;
                case 6: mediator.RaiseMenuSelected(MenuCodes.StageWindow); break;
                case 7: mediator.RaiseMenuSelected(MenuCodes.ControlPanel); break;
                case 8: mediator.RaiseMenuSelected(MenuCodes.CastWindow); break;
                case 9: mediator.RaiseMenuSelected(MenuCodes.ScoreWindow); break;
                case 15: mediator.RaiseMenuSelected(MenuCodes.ObjectInspector); break;
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
