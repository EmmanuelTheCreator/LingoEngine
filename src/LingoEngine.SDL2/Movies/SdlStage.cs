using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;

namespace LingoEngineSDL2.Movies;

public class SdlStage : ILingoFrameworkStage, IDisposable
{
    private readonly LingoClock _clock;
    private LingoStage _stage = null!;
    private SdlMovie? _activeMovie;

    public SdlStage(LingoClock clock)
    {
        _clock = clock;
    }

    internal void Init(LingoStage stage)
    {
        _stage = stage;
    }

    internal void ShowMovie(SdlMovie movie)
    {
        // TODO: add to SDL window
    }
    internal void HideMovie(SdlMovie movie)
    {
        // TODO: remove from SDL window
    }

    public void SetActiveMovie(LingoMovie? lingoMovie)
    {
        _activeMovie?.Hide();
        if (lingoMovie == null) { _activeMovie = null; return; }
        var movie = lingoMovie.Framework<SdlMovie>();
        _activeMovie = movie;
        movie.Show();
    }

    public void Dispose() { }
}
