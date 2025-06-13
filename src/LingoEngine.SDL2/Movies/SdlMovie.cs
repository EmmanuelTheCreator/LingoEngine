using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Primitives;

namespace LingoEngineSDL2.Movies;

public class SdlMovie : ILingoFrameworkMovie, IDisposable
{
    private readonly SdlStage _stage;
    private readonly Action<SdlMovie> _removeMethod;
    private readonly HashSet<SdlSprite> _drawnSprites = new();
    private readonly HashSet<SdlSprite> _allSprites = new();
    private LingoMovie _movie;

    public SdlMovie(SdlStage stage, LingoMovie movie, Action<SdlMovie> removeMethod)
    {
        _stage = stage;
        _movie = movie;
        _removeMethod = removeMethod;
        stage.ShowMovie(this);
    }

    internal void Show() => _stage.ShowMovie(this);
    internal void Hide() => _stage.HideMovie(this);

    public void UpdateStage()
    {
        foreach(var s in _drawnSprites)
            s.Update();
    }

    internal void CreateSprite<T>(T lingoSprite) where T : LingoSprite
    {
        var sprite = new SdlSprite(lingoSprite, s => _drawnSprites.Add(s), s => _drawnSprites.Remove(s), s => { _drawnSprites.Remove(s); _allSprites.Remove(s); });
        _allSprites.Add(sprite);
    }

    public void RemoveMe()
    {
        _removeMethod(this);
    }

    LingoPoint ILingoFrameworkMovie.GetGlobalMousePosition() => (0,0);

    public void Dispose()
    {
        Hide();
        RemoveMe();
    }
}
