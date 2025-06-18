using LingoEngine.Core;
using LingoEngine.Director.Core.Events;
using LingoEngine.IO;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Movies;

namespace LingoEngine.Director.Core;

/// <summary>
/// Handles project level operations such as saving and loading movies.
/// Subscribes to menu events from <see cref="IDirectorEventMediator"/>.
/// </summary>
public class DirectorProjectManager
{
    private readonly ProjectSettings _settings;
    private readonly LingoPlayer _player;
    private readonly IDirectorEventMediator _mediator;
    private readonly JsonStateRepository _repo = new();

    public DirectorProjectManager(ProjectSettings settings, LingoPlayer player, IDirectorEventMediator mediator)
    {
        _settings = settings;
        _player = player;
        _mediator = mediator;

        _mediator.SubscribeToMenu(DirectorMenuCodes.FileSave, SaveMovie);
        _mediator.SubscribeToMenu(DirectorMenuCodes.FileLoad, LoadMovie);
    }

    private bool SaveMovie()
    {
        if (!_settings.HasValidSettings)
        {
            _mediator.RaiseMenuSelected(DirectorMenuCodes.ProjectSettingsWindow);
            return true;
        }
        if (_player.ActiveMovie is not LingoMovie movie)
            return true;

        Directory.CreateDirectory(_settings.ProjectFolder);
        var path = _settings.GetMoviePath(_settings.ProjectName);
        _repo.Save(path, movie);
        return true;
    }

    private bool LoadMovie()
    {
        if (!_settings.HasValidSettings)
        {
            _mediator.RaiseMenuSelected(DirectorMenuCodes.ProjectSettingsWindow);
            return true;
        }
        var path = _settings.GetMoviePath(_settings.ProjectName);
        if (!File.Exists(path))
            return true;

        var movie = _repo.Load(path, _player);
        _player.SetActiveMovie(movie);
        return true;
    }
}
