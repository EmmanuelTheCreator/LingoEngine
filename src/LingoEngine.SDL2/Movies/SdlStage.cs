using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;

namespace LingoEngine.SDL2.Movies;

public class SdlStage : ILingoFrameworkStage, IDisposable
{
    private readonly LingoClock _clock;
    private readonly SdlRootContext _rootContext;
    private LingoStage _stage = null!;
    private readonly HashSet<SdlMovie> _movies = new();
    private SdlMovie? _activeMovie;
    public float Scale { get ; set; }

    public SdlStage(SdlRootContext rootContext, LingoClock clock)
    {
        _rootContext = rootContext;
        _clock = clock;
    }

    internal SdlRootContext RootContext => _rootContext;


    internal void Init(LingoStage stage)
    {
        _stage = stage;
    }

    internal void ShowMovie(SdlMovie movie)
    {
        _movies.Add(movie);
    }
    internal void HideMovie(SdlMovie movie)
    {
        _movies.Remove(movie);
    }

    public void SetActiveMovie(LingoMovie? lingoMovie)
    {
        _activeMovie?.Hide();
        if (lingoMovie == null) { _activeMovie = null; return; }
        var movie = lingoMovie.Framework<SdlMovie>();
        _activeMovie = movie;
        movie.Show();
    }

    public void Dispose() { _movies.Clear(); }
}
