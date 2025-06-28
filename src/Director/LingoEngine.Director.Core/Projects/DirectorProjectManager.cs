using LingoEngine.Core;
using LingoEngine.IO;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Director.Core.Menus;
using LingoEngine.Director.Core.Gfx;
using LingoEngine.Projects;

namespace LingoEngine.Director.Core.Projects;

/// <summary>
/// Handles project level operations such as saving and loading movies.
/// </summary>
public class DirectorProjectManager
{
    private readonly LingoProjectSettings _settings;
    private readonly LingoPlayer _player;
    private readonly JsonStateRepository _repo = new();
    private readonly IDirectorWindowManager _windowManager;

    public DirectorProjectManager(LingoProjectSettings settings, LingoPlayer player, IDirectorWindowManager windowManager)
    {
        _settings = settings;
        _player = player;
        _windowManager = windowManager;
    }

    public void SaveMovie()
    {
        if (!_settings.HasValidSettings)
        {
            _windowManager.OpenWindow(DirectorMenuCodes.ProjectSettingsWindow);
            return;
        }
        if (_player.ActiveMovie is not LingoMovie movie)
            return;

        Directory.CreateDirectory(_settings.ProjectFolder);
        var path = _settings.GetMoviePath(_settings.ProjectName);
        _repo.Save(path, movie);
    }

    public void LoadMovie()
    {
        if (!_settings.HasValidSettings)
        {
            _windowManager.OpenWindow(DirectorMenuCodes.ProjectSettingsWindow);
            return;
        }
        var path = _settings.GetMoviePath(_settings.ProjectName);
        if (!File.Exists(path))
            return;

        var movie = _repo.Load(path, _player);
        _player.SetActiveMovie(movie);
    }
}
