using AbstUI.Commands;
using AbstUI.Windowing;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.Core.Projects.Commands;
using BlingoEngine.Director.Core.Stages;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.IO;
using BlingoEngine.Movies;
using BlingoEngine.Projects;
using BlingoEngine.Net.RNetContracts;
using System.IO;

namespace BlingoEngine.Director.Core.Projects;

/// <summary>
/// Handles project level operations such as saving and loading movies.
/// </summary>
public class DirectorProjectManager : IAbstCommandHandler<SaveDirProjectSettingsCommand>
{
    private readonly BlingoProjectSettings _settings;
    private readonly BlingoPlayer _player;
    private readonly JsonStateRepository _repo = new();
    private readonly IAbstWindowManager _windowManager;
    private readonly DirectorProjectSettings _dirSettings;
    private readonly DirectorStageGuides _guides;
    private readonly RNetConfiguration _rnetConfig;
    private readonly DirectorProjectSettingsRepository _settingsRepo;
    private readonly BlingoProjectSettingsRepository _projectSettingsRepo;
    private bool _settingsHaveBeenLoaded = false;

    public DirectorProjectManager(
        BlingoProjectSettings settings,
        BlingoPlayer player,
        IAbstWindowManager windowManager,
        DirectorProjectSettings dirSettings,
        DirectorStageGuides guides,
        RNetConfiguration rnetConfig,
        DirectorProjectSettingsRepository settingsRepo,
        BlingoProjectSettingsRepository projectSettingsRepo)
    {
        _settings = settings;
        _player = player;
        _windowManager = windowManager;
        _dirSettings = dirSettings;
        _guides = guides;
        _rnetConfig = rnetConfig;
        _settingsRepo = settingsRepo;
        _projectSettingsRepo = projectSettingsRepo;
    }

    public void SaveMovie()
    {
        if (!_settings.HasValidSettings)
        {
            _windowManager.OpenWindow(DirectorMenuCodes.ProjectSettingsWindow);
            return;
        }
        if (_player.ActiveMovie is not BlingoMovie movie)
            return;
        var settingsPath = GetSettingsPath();
        if (!Directory.Exists(settingsPath))
            Directory.CreateDirectory(settingsPath);
        var path = GetMoviePath(_settings.ProjectName);
        var jsonTuple = _repo.Save(path, movie);

        SaveDirectorSettings();
        SaveProjectSettings();

        var generator = new DirectorMovieCodeGenerator();
        generator.Generate(jsonTuple.MovieDto, GetCodeSetupPath());
    }

    public void LoadMovie()
    {
        if (!_settings.HasValidSettings)
        {
            _windowManager.OpenWindow(DirectorMenuCodes.ProjectSettingsWindow);
            return;
        }
        var path = GetMoviePath(_settings.ProjectName);
        if (!File.Exists(path))
            return;

        var movie = _repo.Load(path, _player);
        _player.SetActiveMovie(movie);

        LoadProjectSettings();
        LoadDirectorSettings();
    }
    private string GetSettingsPath() => Path.Combine(_settings.ProjectFolder, "Settings");
    private string GetCodeSetupPath() => Path.Combine(_settings.CodeFolder, "Setup");

    public string GetMoviePath(string movieName)
    {
        var file = movieName + ".json";
        return Path.Combine(GetSettingsPath(), file);
    }

    private void SaveDirectorSettings()
    {
        // Copy grid and guide settings
        _dirSettings.GridColor = _guides.GridColor;
        _dirSettings.GridVisible = _guides.GridVisible;
        _dirSettings.GridSnap = _guides.GridSnap;
        _dirSettings.GridWidth = _guides.GridWidth;
        _dirSettings.GridHeight = _guides.GridHeight;

        _dirSettings.GuidesColor = _guides.GuidesColor;
        _dirSettings.GuidesVisible = _guides.GuidesVisible;
        _dirSettings.GuidesSnap = _guides.GuidesSnap;
        _dirSettings.GuidesLocked = _guides.GuidesLocked;

        _dirSettings.VerticalGuides = _guides.VerticalGuides.ToList();
        _dirSettings.HorizontalGuides = _guides.HorizontalGuides.ToList();

        _dirSettings.RNet.Port = _rnetConfig.Port;
        _dirSettings.RNet.AutoStartRNetHostOnStartup = _rnetConfig.AutoStartRNetHostOnStartup;
        _dirSettings.RNet.ClientType = _rnetConfig.ClientType;
        _dirSettings.RNet.RemoteRole = _rnetConfig.RemoteRole;

        var states = new Dictionary<string, DirectorWindowState>();
        if (_windowManager is AbstWindowManager dm)
        {
            foreach (var (code, rect) in dm.GetRects())
                states[code] = new DirectorWindowState { X = (int)rect.Left, Y = (int)rect.Top, Width = (int)rect.Width, Height = (int)rect.Height };
        }

        _dirSettings.WindowStates = states;

        var settingsPath = Path.Combine(GetSettingsPath(), _settings.ProjectName + ".director.json");
        _settingsRepo.Save(settingsPath, _dirSettings);
    }

    private void LoadDirectorSettings()
    {
        var settingsPath = Path.Combine(GetSettingsPath(), _settings.ProjectName + ".director.json");
        var loaded = _settingsRepo.Load(settingsPath);

        // Copy into runtime settings object
        _dirSettings.GuidesColor = loaded.GuidesColor;
        _dirSettings.GuidesVisible = loaded.GuidesVisible;
        _dirSettings.GuidesSnap = loaded.GuidesSnap;
        _dirSettings.GuidesLocked = loaded.GuidesLocked;
        _dirSettings.VerticalGuides = loaded.VerticalGuides.ToList();
        _dirSettings.HorizontalGuides = loaded.HorizontalGuides.ToList();

        _dirSettings.GridColor = loaded.GridColor;
        _dirSettings.GridVisible = loaded.GridVisible;
        _dirSettings.GridSnap = loaded.GridSnap;
        _dirSettings.GridWidth = loaded.GridWidth;
        _dirSettings.GridHeight = loaded.GridHeight;

        _dirSettings.RNet.Port = loaded.RNet.Port;
        _dirSettings.RNet.AutoStartRNetHostOnStartup = loaded.RNet.AutoStartRNetHostOnStartup;
        _dirSettings.RNet.ClientType = loaded.RNet.ClientType;
        _dirSettings.RNet.RemoteRole = loaded.RNet.RemoteRole;

        _rnetConfig.Port = loaded.RNet.Port;
        _rnetConfig.AutoStartRNetHostOnStartup = loaded.RNet.AutoStartRNetHostOnStartup;
        _rnetConfig.ClientType = loaded.RNet.ClientType;
        _rnetConfig.RemoteRole = loaded.RNet.RemoteRole;

        _dirSettings.WindowStates = loaded.WindowStates;

        // Apply to guides object
        _guides.GridColor = _dirSettings.GridColor;
        _guides.GridVisible = _dirSettings.GridVisible;
        _guides.GridSnap = _dirSettings.GridSnap;
        _guides.GridWidth = _dirSettings.GridWidth;
        _guides.GridHeight = _dirSettings.GridHeight;

        _guides.GuidesColor = _dirSettings.GuidesColor;
        _guides.GuidesVisible = _dirSettings.GuidesVisible;
        _guides.GuidesSnap = _dirSettings.GuidesSnap;
        _guides.GuidesLocked = _dirSettings.GuidesLocked;

        _guides.VerticalGuides.Clear();
        foreach (var v in _dirSettings.VerticalGuides)
            _guides.VerticalGuides.Add(v);
        _guides.HorizontalGuides.Clear();
        foreach (var h in _dirSettings.HorizontalGuides)
            _guides.HorizontalGuides.Add(h);
        _guides.Draw();

        // Apply window states
        if (_windowManager is AbstWindowManager dm)
        {
            foreach (var window in dm.GetWindows())
            {
                if (!_dirSettings.WindowStates.TryGetValue(window.WindowCode, out var st))
                    continue;

                window.SetPositionAndSize(st.X, st.Y, st.Width, st.Height);
            }
        }
    }

    private void SaveProjectSettings()
    {
        EnsureSettingsLoaded();
        var settingsPath = Path.Combine(GetSettingsPath(), _settings.ProjectName + ".lingo.json");
        _projectSettingsRepo.Save(settingsPath, _settings);
    }

    private void LoadProjectSettings()
    {
        var settingsPath = Path.Combine(GetSettingsPath(), _settings.ProjectName + ".lingo.json");
        var loaded = _projectSettingsRepo.Load(settingsPath);
        _settings.CodeFolder = loaded.CodeFolder;
        _settings.MaxSpriteChannelCount = loaded.MaxSpriteChannelCount;
    }
    private void EnsureSettingsLoaded()
    {
        if (_settingsHaveBeenLoaded)
            return;
        LoadProjectSettings();
        LoadDirectorSettings();
        _settingsHaveBeenLoaded = true;
    }
    public bool CanExecute(SaveDirProjectSettingsCommand command) => true;

    public bool Handle(SaveDirProjectSettingsCommand command)
    {
        if (command.ProjectSettings != null)
        {
            _settings.ProjectName = command.ProjectSettings.ProjectName;
            _settings.ProjectFolder = command.ProjectSettings.ProjectFolder;
        }
        if (string.IsNullOrWhiteSpace(_settings.ProjectName) || string.IsNullOrWhiteSpace(_settings.ProjectFolder))
        {
            // project needs to be set first
            return false;
        }
        SaveProjectSettings();

        if (command.StartNewProjectAfterSave)
            StartNewProject();

        return true;
    }

    private void StartNewProject()
    {
        if (!_settings.HasValidSettings)
            return;

        if (_player.ActiveMovie is BlingoMovie activeMovie)
        {
            _player.CloseMovie(activeMovie);
            _player.SetActiveMovie(null);
        }

        if (_settings.StageWidth > 0 && _settings.StageHeight > 0)
            _player.LoadStage(_settings.StageWidth, _settings.StageHeight, _player.Stage.BackgroundColor);

        var projectName = _settings.ProjectName;
        var newMovie = (BlingoMovie)_player.NewMovie(projectName);
        newMovie.MaxSpriteChannelCount = _settings.MaxSpriteChannelCount;

        var settingsPath = GetSettingsPath();
        if (!Directory.Exists(settingsPath))
            Directory.CreateDirectory(settingsPath);

        _repo.Save(GetMoviePath(projectName), newMovie);
        SaveDirectorSettings();
    }
}

